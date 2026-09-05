using System.Windows.Automation.Peers;

namespace AscWpfControls.Controls;

/// <summary>
/// Keeps a masked value out of the accessibility interface.
///
/// A text box hands its content to any process that asks over that interface. Without this, hiding
/// the value on screen would be theatre: the onlooker cannot read it, but any program can. The
/// native <c>PasswordBox</c> announces itself as a password control and stays silent for the same
/// reason.
///
/// While masked the peer declares itself a password and withholds the value pattern altogether -
/// nothing to read, and nothing to write either. Revealed, it behaves like an ordinary text box
/// again, which is the point of revealing.
/// </summary>
public class TextFieldAutomationPeer : TextBoxAutomationPeer
{
    public TextFieldAutomationPeer(TextField owner) : base(owner)
    {
    }

    private TextField Field => (TextField)Owner;

    protected override bool IsPasswordCore() => Field.IsMasked;

    protected override string GetClassNameCore() => nameof(TextField);

    public override object? GetPattern(PatternInterface patternInterface) =>
        Field.IsMasked && patternInterface is PatternInterface.Value or PatternInterface.Text
            ? null
            : base.GetPattern(patternInterface);
}
