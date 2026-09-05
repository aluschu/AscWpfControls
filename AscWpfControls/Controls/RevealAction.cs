using System.Windows;
using System.Windows.Media;

namespace AscWpfControls.Controls;

/// <summary>
/// Shows and hides the content of a field that holds a secret. The eye at the right-hand end.
///
/// <b>It hides from onlookers, it does not protect.</b> Good for the API key on a shared screen or
/// the screenshot about to be sent; useless against anyone who can read the machine's memory. A
/// field that has to withstand that cannot be built on a text box at all - see
/// <see cref="IMaskableFieldEditor"/>.
///
/// The action stays away from a field whose content is not secret: there it would toggle nothing.
/// </summary>
public class RevealAction : FieldAction
{
    static RevealAction()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RevealAction),
            new FrameworkPropertyMetadata(typeof(RevealAction)));

        // An eye belongs beside a masked field whether or not anything has been typed yet - unlike
        // a clear button, it does not promise to act on content that is there.
        ShowWhenProperty.OverrideMetadata(typeof(RevealAction),
            new FrameworkPropertyMetadata(FieldActionVisibility.Always));
    }

    /// <summary>The built-in eye, drawn on a 16x10 area.</summary>
    public static readonly Geometry DefaultIcon = Geometry.Parse(
        "M 0,5 C 4,0 12,0 16,5 C 12,10 4,10 0,5 Z M 8,2.6 A 2.4,2.4 0 1 0 8.01,2.6 Z");

    /// <summary>The same eye with a stroke through it, shown while the content is in clear.</summary>
    public static readonly Geometry RevealedIcon = Geometry.Parse(
        "M 0,5 C 4,0 12,0 16,5 C 12,10 4,10 0,5 Z M 8,2.6 A 2.4,2.4 0 1 0 8.01,2.6 Z M 1,10 L 15,0");

    private static readonly DependencyPropertyKey IsRevealedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsRevealed), typeof(bool), typeof(RevealAction),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsRevealedProperty = IsRevealedPropertyKey.DependencyProperty;

    /// <summary>
    /// Whether the field is showing its content right now. Read-only and only here for the template,
    /// which swaps the icon on it - the state itself belongs to the field.
    /// </summary>
    public bool IsRevealed
    {
        get => (bool)GetValue(IsRevealedProperty);
        private set => SetValue(IsRevealedPropertyKey, value);
    }

    protected override bool IsSupported(IFieldEditor editor) =>
        editor is IMaskableFieldEditor { IsSecret: true };

    /// <summary>Revealing changes the view, not the content - so it stays in a read-only field.</summary>
    protected override bool ChangesContent => false;

    protected override void Run(IFieldEditor editor)
    {
        if (editor is not IMaskableFieldEditor maskable)
            return;

        maskable.IsRevealed = !maskable.IsRevealed;
    }

    protected override void OnEditorStateChanged()
    {
        base.OnEditorStateChanged();
        IsRevealed = Editor is IMaskableFieldEditor { IsRevealed: true };
    }
}
