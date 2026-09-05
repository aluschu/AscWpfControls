namespace AscWpfControls.Controls;

/// <summary>
/// A field that can hide its content behind dots.
///
/// <b>A second, optional role.</b> Not every field can do this, and an action that needs it says so
/// rather than assuming - which is how a reveal button stays away from a field that could never show
/// its content in clear anyway.
///
/// <b>What this is not.</b> Hiding here means hiding from onlookers: the key on a shared screen, the
/// screenshot about to be sent. It is not protection from anyone with access to the machine. The
/// value is an ordinary string in memory, as in any text box, and no toggle changes that.
/// </summary>
public interface IMaskableFieldEditor : IFieldEditor
{
    /// <summary>
    /// Whether the content of this field is a secret at all. Decided by the field, not by the
    /// button: a value is secret or it is not, quite apart from whether anyone offers a way to look
    /// at it.
    /// </summary>
    bool IsSecret { get; }

    /// <summary>Whether the content is currently shown in clear.</summary>
    bool IsRevealed { get; set; }
}
