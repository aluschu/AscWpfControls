using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace AscWpfControls.Controls;

/// <summary>
/// The trim around an editor: border, placeholder, and a strip of actions at the right-hand end -
/// with a hole in the middle for whatever does the actual editing.
///
/// <b>Why this is a control of its own and not simply part of the field.</b> A field with real
/// password security cannot inherit from a plain text field: inheritance only ever adds, so an
/// inherited, bindable <c>Text</c> would remain the convenient way around the protection. Such a
/// control has to stand beside the text field and wrap a <c>PasswordBox</c> instead. If the trim is
/// a separate piece, it gets the placeholder and the clear button for free; if it were baked into
/// the text field, that would be a rebuild.
///
/// The trim knows nothing about what sits in its hole. It only knows there is one.
/// </summary>
[TemplatePart(Name = ActionHostName, Type = typeof(Panel))]
public class FieldChrome : ContentControl
{
    private const string CategoryName = "Field Chrome";
    private const string ActionHostName = "PART_Actions";

    private Panel? _actionHost;

    static FieldChrome()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FieldChrome),
            new FrameworkPropertyMetadata(typeof(FieldChrome)));
    }

    public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
        nameof(PlaceholderText), typeof(string), typeof(FieldChrome));

    /// <summary>
    /// The text shown while the field is empty. Named as in WinUI, UWP and Actipro, so that anyone
    /// arriving from those does not have to look it up.
    /// </summary>
    [Category(CategoryName)]
    [Description("The text shown while the field is empty.")]
    public string? PlaceholderText
    {
        get => (string?)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly DependencyProperty IsPlaceholderVisibleProperty = DependencyProperty.Register(
        nameof(IsPlaceholderVisible), typeof(bool), typeof(FieldChrome), new PropertyMetadata(false));

    /// <summary>
    /// Whether the placeholder is showing right now. The trim cannot work this out for itself - it
    /// does not know what is in its hole - so the field tells it.
    /// </summary>
    [Browsable(false)]
    public bool IsPlaceholderVisible
    {
        get => (bool)GetValue(IsPlaceholderVisibleProperty);
        set => SetValue(IsPlaceholderVisibleProperty, value);
    }

    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(
        nameof(Actions), typeof(FieldActionCollection), typeof(FieldChrome),
        new PropertyMetadata(null, (d, e) => ((FieldChrome)d).OnActionsReplaced(e)));

    /// <summary>The buttons at the right-hand end, in the order they were given.</summary>
    [Browsable(false)]
    public FieldActionCollection? Actions
    {
        get => (FieldActionCollection?)GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _actionHost = GetTemplateChild(ActionHostName) as Panel;
        FillActionHost();
    }

    private void OnActionsReplaced(DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is FieldActionCollection previous)
            previous.CollectionChanged -= OnActionsChanged;

        if (e.NewValue is FieldActionCollection current)
            current.CollectionChanged += OnActionsChanged;

        FillActionHost();
    }

    private void OnActionsChanged(object? sender, NotifyCollectionChangedEventArgs e) => FillActionHost();

    /// <summary>
    /// Puts the actions into the strip as real children.
    ///
    /// <b>Not an ItemsControl</b>, and that is not a matter of taste: an ItemsControl lays its own
    /// automation peer over every item, and the action's own peer never surfaces beneath it. The
    /// buttons then exist for the eye but not for a screen reader - which is how this was found, by
    /// an automation script that reported three buttons in a window full of them.
    /// </summary>
    private void FillActionHost()
    {
        if (_actionHost is null)
            return;

        _actionHost.Children.Clear();

        if (Actions is not { } actions)
            return;

        foreach (var action in actions)
        {
            // An element has one visual parent. Without this, re-applying the template or moving a
            // field would throw instead of moving the button.
            if (action.Parent is Panel previousHost)
                previousHost.Children.Remove(action);

            _actionHost.Children.Add(action);
        }
    }
}
