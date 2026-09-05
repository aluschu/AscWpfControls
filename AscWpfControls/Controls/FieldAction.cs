using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace AscWpfControls.Controls;

/// <summary>
/// A button that sits inside a field and acts on its content.
///
/// <b>The icon comes in four stages</b>, after Alan Kay's "simple things should be simple, complex
/// things should be possible":
/// <list type="number">
/// <item>Set nothing - the action draws the icon it was born with.</item>
/// <item>Set <see cref="IconGeometry"/> - your own path, everything else stays: size, colour, hover.</item>
/// <item>Set <c>Content</c> - any visual at all.</item>
/// <item>Set <c>Style</c> or <c>ContentTemplate</c> - full control, as with any WPF control.</item>
/// </list>
/// Stage 2 is the one most often needed in practice: a house has its own icon set, but not its own
/// idea of how large a button inside a field should be.
/// </summary>
public abstract class FieldAction : ButtonBase
{
    private const string CategoryName = "Field Action";

    private IFieldEditor? _editor;

    static FieldAction()
    {
        FocusableProperty.OverrideMetadata(typeof(FieldAction), new FrameworkPropertyMetadata(false));
    }

    public static readonly DependencyProperty IconGeometryProperty = DependencyProperty.Register(
        nameof(IconGeometry), typeof(Geometry), typeof(FieldAction));

    /// <summary>
    /// The path drawn on the button. Leave it unset and the action supplies its own - a cross for
    /// clearing, an eye for revealing.
    ///
    /// A <see cref="Geometry"/> rather than an image, so it scales and takes its colour from the
    /// button. It also keeps the library a single assembly with nothing trailing behind it.
    /// </summary>
    [Category(CategoryName)]
    [Description("The path drawn on the button. Unset means the action's own icon.")]
    public Geometry? IconGeometry
    {
        get => (Geometry?)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    public static readonly DependencyProperty ShowWhenProperty = DependencyProperty.Register(
        nameof(ShowWhen), typeof(FieldActionVisibility), typeof(FieldAction),
        new PropertyMetadata(FieldActionVisibility.WhenNotEmpty, (d, _) => ((FieldAction)d).UpdateVisibility()));

    /// <summary>When the button shows itself - see <see cref="FieldActionVisibility"/>.</summary>
    [Category(CategoryName)]
    [Description("When the button shows itself.")]
    public FieldActionVisibility ShowWhen
    {
        get => (FieldActionVisibility)GetValue(ShowWhenProperty);
        set => SetValue(ShowWhenProperty, value);
    }

    /// <summary>The field this action belongs to, or <c>null</c> while it belongs to none.</summary>
    protected IFieldEditor? Editor => _editor;

    /// <summary>
    /// Hands the action its field. Called by the field itself when the action enters or leaves its
    /// collection - an action never goes looking for its field in the visual tree.
    /// </summary>
    internal void Attach(IFieldEditor? editor)
    {
        if (_editor is { } previous)
            previous.StateChanged -= OnStateChanged;

        _editor = editor;

        if (_editor is { } current)
            current.StateChanged += OnStateChanged;

        OnEditorStateChanged();
    }

    private void OnStateChanged(object? sender, EventArgs e) => OnEditorStateChanged();

    /// <summary>
    /// Anything about the field may have changed. The base class re-decides whether to show itself;
    /// an action with state of its own - a toggle, say - overrides this and calls back.
    /// </summary>
    protected virtual void OnEditorStateChanged() => UpdateVisibility();

    /// <summary>
    /// Whether this action can do anything at all in the field it was given. An action that cannot
    /// stays away entirely - revealing, for one, has no meaning in a field that can never show its
    /// content in clear.
    /// </summary>
    protected virtual bool IsSupported(IFieldEditor editor) => true;

    /// <summary>
    /// Whether the action changes the content. Such actions hide themselves in a read-only field;
    /// ones that only change the view stay.
    /// </summary>
    protected virtual bool ChangesContent => true;

    private void UpdateVisibility()
    {
        Visibility = ShouldBeVisible() ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool ShouldBeVisible()
    {
        if (_editor is not { } editor || !IsSupported(editor))
            return false;

        if (ChangesContent && editor.IsReadOnly)
            return false;

        return ShowWhen switch
        {
            FieldActionVisibility.Always => true,
            FieldActionVisibility.WhenFocusedOrHovered => !editor.IsEmpty && editor.IsActive,
            _ => !editor.IsEmpty,
        };
    }

    protected override void OnClick()
    {
        base.OnClick();

        if (_editor is not { } editor)
            return;

        Run(editor);
        // Whoever pressed a button in a field wants to carry on typing in it, not stand on the
        // button afterwards.
        editor.FocusEditor();
    }

    /// <summary>What this action does. Called with the field it belongs to.</summary>
    protected abstract void Run(IFieldEditor editor);

    /// <summary>
    /// Reports the action to assistive technology as a button. Without this it is invisible to a
    /// screen reader - <c>ButtonBase</c> supplies no peer of its own.
    /// </summary>
    protected override AutomationPeer OnCreateAutomationPeer() => new FieldActionAutomationPeer(this);

    internal void InvokeFromAutomation() => OnClick();
}
