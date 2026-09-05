using System;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Automation.Peers;
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
public class TextField : TextBox, IMaskableFieldEditor
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
        // A masked field shows its own stand-in for "empty"; two texts for one statement would be
        // one too many.
        IsPlaceholderVisible = !string.IsNullOrEmpty(PlaceholderText) && IsEmpty && !IsMasked;

    // ----- Secrets -----

    public static readonly DependencyProperty IsSecretProperty = DependencyProperty.Register(
        nameof(IsSecret), typeof(bool), typeof(TextField),
        new PropertyMetadata(false, (d, _) => ((TextField)d).OnSecrecyChanged()));

    /// <summary>
    /// Whether the content of this field is a secret - an API key, a password, a token.
    ///
    /// <b>The field decides this, not the button.</b> A value is secret or it is not, quite apart
    /// from whether anyone offers a way to look at it; a <see cref="RevealAction"/> only asks the
    /// field what it already knows, and stays away where the answer is no.
    ///
    /// Setting it hides the content behind dots and turns off the undo history - otherwise Ctrl+Z
    /// would bring the cleartext back long after it was hidden again.
    ///
    /// <b>This hides, it does not protect.</b> The value is an ordinary string in memory, as in any
    /// text box. Good against onlookers and screenshots, useless against anyone who can read the
    /// machine.
    /// </summary>
    [Category(CategoryName)]
    [Description("Whether the content is a secret and shown as dots until revealed.")]
    public bool IsSecret
    {
        get => (bool)GetValue(IsSecretProperty);
        set => SetValue(IsSecretProperty, value);
    }

    public static readonly DependencyProperty IsRevealedProperty = DependencyProperty.Register(
        nameof(IsRevealed), typeof(bool), typeof(TextField),
        new PropertyMetadata(false, (d, _) => ((TextField)d).OnSecrecyChanged()));

    /// <summary>Whether a secret is currently shown in clear. Meaningless unless <see cref="IsSecret"/>.</summary>
    [Category(CategoryName)]
    [Description("Whether a secret is currently shown in clear.")]
    public bool IsRevealed
    {
        get => (bool)GetValue(IsRevealedProperty);
        set => SetValue(IsRevealedProperty, value);
    }

    public static readonly DependencyProperty EmptySecretTextProperty = DependencyProperty.Register(
        nameof(EmptySecretText), typeof(string), typeof(TextField),
        new PropertyMetadata("(not set)", (d, _) => ((TextField)d).OnSecrecyChanged()));

    /// <summary>
    /// What stands in for the dots while a secret field is empty. Without it an empty secret would
    /// look exactly like a filled one, and nobody could tell whether anything had ever been entered.
    /// </summary>
    [Category(CategoryName)]
    [Description("Shown instead of dots while a secret field is empty.")]
    public string? EmptySecretText
    {
        get => (string?)GetValue(EmptySecretTextProperty);
        set => SetValue(EmptySecretTextProperty, value);
    }

    private static readonly DependencyPropertyKey IsMaskedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsMasked), typeof(bool), typeof(TextField),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsMaskedProperty = IsMaskedPropertyKey.DependencyProperty;

    /// <summary>Whether the content is hidden right now - secret and not revealed.</summary>
    [Browsable(false)]
    public bool IsMasked
    {
        get => (bool)GetValue(IsMaskedProperty);
        private set => SetValue(IsMaskedPropertyKey, value);
    }

    private static readonly DependencyPropertyKey MaskTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(MaskText), typeof(string), typeof(TextField),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MaskTextProperty = MaskTextPropertyKey.DependencyProperty;

    /// <summary>
    /// What is shown in place of the content - for the template.
    ///
    /// Always the same number of dots, whatever the value. A row of dots that grew with the secret
    /// would give away its length, which is the one thing about a secret that is free to guess.
    /// </summary>
    [Browsable(false)]
    public string MaskText
    {
        get => (string)GetValue(MaskTextProperty);
        private set => SetValue(MaskTextPropertyKey, value);
    }

    private const int MaskLength = 16;

    private void OnSecrecyChanged()
    {
        IsMasked = IsSecret && !IsRevealed;
        MaskText = IsEmpty ? EmptySecretText ?? string.Empty : new string('•', MaskLength);

        // Undo keeps every earlier value. On a secret that means the cleartext survives long after
        // it was hidden again, one Ctrl+Z away.
        IsUndoEnabled = !IsSecret;

        UpdatePlaceholder();
        ReportState();
    }

    /// <summary>
    /// While masked the field takes no input. The editor is hidden, so there is nothing to type
    /// into and nowhere for the caret to be - but focus can still land here by keyboard, and
    /// unhandled keys would then change a value nobody can see.
    /// </summary>
    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        if (IsMasked)
            e.Handled = true;

        base.OnPreviewTextInput(e);
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (IsMasked && e.Key is Key.Back or Key.Delete or Key.Space or Key.Enter)
            e.Handled = true;

        base.OnPreviewKeyDown(e);
    }

    /// <summary>Keeps a masked value out of the accessibility interface - see the peer.</summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new TextFieldAutomationPeer(this);

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
        // The dots stand in for "empty" too, so a change of content can change them.
        MaskText = IsEmpty ? EmptySecretText ?? string.Empty : new string('•', MaskLength);
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
