using System;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace AscWpfControls.Controls;

/// <summary>
/// A text box with a placeholder and a strip of buttons inside it.
///
/// <b>It inherits from <see cref="TextBox"/> and does not wrap one.</b> That hands it binding,
/// selection, undo, the clipboard, spell checking and the input method editors without a line of
/// code, and it lets a caller swap <c>TextBox</c> for <c>TextField</c> without losing anything.
/// Wrapping would have meant forwarding dozens of members by hand, and whatever was forgotten would
/// have failed silently.
///
/// <example>
/// <code>
/// &lt;asc:TextField Text="{Binding Symbol}" PlaceholderText="Symbol or ISIN"&gt;
///     &lt;asc:ClearAction /&gt;
/// &lt;/asc:TextField&gt;
/// </code>
/// </example>
/// </summary>
[ContentProperty(nameof(Actions))]
public class TextField : TextBox, IFieldEditor
{
    private const string CategoryName = "Text Field";

    static TextField()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TextField),
            new FrameworkPropertyMetadata(typeof(TextField)));
    }

    public TextField()
    {
        Actions = new FieldActionCollection();
        Actions.CollectionChanged += OnActionsChanged;
    }

    // ----- Placeholder -----

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText), typeof(string), typeof(TextField),
        new PropertyMetadata(null, (d, _) => ((TextField)d).UpdatePlaceholder()));

    /// <summary>
    /// The text shown while the field is empty. Named as in WinUI, UWP and Actipro.
    ///
    /// <b>It is not a value.</b> It is never bound to the source, never saved and never read back;
    /// leaving it standing and saving saves nothing. Where it says what is meant, the label beside
    /// the field can be short or absent altogether - which is the point of it.
    /// </summary>
    [Category(CategoryName)]
    [Description("The text shown while the field is empty. Not a value.")]
    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    private static readonly DependencyPropertyKey IsPlaceholderVisiblePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPlaceholderVisible), typeof(bool), typeof(TextField),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsPlaceholderVisibleProperty =
        IsPlaceholderVisiblePropertyKey.DependencyProperty;

    /// <summary>Whether the placeholder is showing - for the template.</summary>
    [Browsable(false)]
    public bool IsPlaceholderVisible
    {
        get => (bool)GetValue(IsPlaceholderVisibleProperty);
        private set => SetValue(IsPlaceholderVisiblePropertyKey, value);
    }

    private void UpdatePlaceholder() =>
        IsPlaceholderVisible = !string.IsNullOrEmpty(PlaceholderText) && IsEmpty;

    // ----- Actions -----

    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
        nameof(Actions), typeof(FieldActionCollection), typeof(TextField));

    /// <summary>
    /// The buttons inside the field, at the right-hand end. A collection rather than a set of
    /// switches: the third kind of button costs a small class, not another flag in the constructor.
    /// </summary>
    [Browsable(false)]
    public FieldActionCollection Actions
    {
        get => (FieldActionCollection)GetValue(ActionsProperty);
        private set => SetValue(ActionsProperty, value);
    }

    private void OnActionsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        foreach (var gone in e.OldItems?.OfType<FieldAction>() ?? [])
            gone.Attach(null);

        foreach (var added in e.NewItems?.OfType<FieldAction>() ?? [])
            added.Attach(this);
    }

    // ----- IFieldEditor -----

    /// <summary>Whether the field holds nothing.</summary>
    public bool IsEmpty => string.IsNullOrEmpty(Text);

    bool IFieldEditor.IsReadOnly => IsReadOnly;

    /// <summary>Whether the user is at this field - keyboard focus or pointer.</summary>
    public bool IsActive => IsKeyboardFocusWithin || IsMouseOver;

    void IFieldEditor.Clear() => Clear();

    void IFieldEditor.FocusEditor() => Focus();

    /// <inheritdoc />
    public event EventHandler? StateChanged;

    private void ReportState()
    {
        UpdatePlaceholder();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);
        ReportState();
    }

    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        ReportState();
    }

    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        ReportState();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        ReportState();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        ReportState();
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // A read-only field hides the actions that would change its content, so a change here has to
        // reach them.
        if (e.Property == IsReadOnlyProperty)
            ReportState();
    }
}
