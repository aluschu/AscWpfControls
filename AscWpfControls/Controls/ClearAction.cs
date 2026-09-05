using System.Windows;
using System.Windows.Media;

namespace AscWpfControls.Controls;

/// <summary>
/// Empties the field. The cross at the right-hand end.
///
/// It stays away from a read-only field: there it would promise an effect it does not have.
/// </summary>
public class ClearAction : FieldAction
{
    static ClearAction()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ClearAction),
            new FrameworkPropertyMetadata(typeof(ClearAction)));
    }

    /// <summary>The built-in cross - two strokes, drawn on a 10x10 square.</summary>
    public static readonly Geometry DefaultIcon =
        Geometry.Parse("M 0,0 L 10,10 M 10,0 L 0,10");

    protected override void Run(IFieldEditor editor) => editor.Clear();
}
