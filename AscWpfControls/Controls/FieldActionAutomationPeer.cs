using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace AscWpfControls.Controls;

/// <summary>
/// Makes a <see cref="FieldAction"/> visible to assistive technology.
///
/// <b>Why this exists at all.</b> <c>ButtonBase</c> brings no automation peer of its own - only
/// <c>Button</c> does. Without one, the action is a button a screen reader cannot see and no test
/// can drive; it was found exactly that way, by an automation script that reported three buttons in
/// a window full of them.
///
/// Deriving from <c>Button</c> instead would have been shorter, but drags <c>IsDefault</c> and
/// <c>IsCancel</c> along - and a clear button that answers the Enter key is not a good idea.
/// </summary>
public class FieldActionAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
{
    public FieldActionAutomationPeer(FieldAction owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

    protected override string GetClassNameCore() => Owner.GetType().Name;

    public override object? GetPattern(PatternInterface patternInterface) =>
        patternInterface == PatternInterface.Invoke ? this : base.GetPattern(patternInterface);

    /// <summary>Presses the button, exactly as a click would.</summary>
    public void Invoke() => ((FieldAction)Owner).InvokeFromAutomation();
}
