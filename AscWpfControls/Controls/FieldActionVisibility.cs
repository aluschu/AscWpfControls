namespace AscWpfControls.Controls;

/// <summary>
/// When an action inside a field shows itself.
///
/// There is no right answer here, which is why it is an option rather than a decision. A form with
/// twelve filled fields wants something different from a single search box, and both are right.
/// </summary>
public enum FieldActionVisibility
{
    /// <summary>
    /// While the field holds something. The default: a text box exists to receive information, so
    /// a filled field is the normal case - and a visible button along with it.
    /// </summary>
    WhenNotEmpty,

    /// <summary>
    /// While the field holds something <b>and</b> the user is at it - focus or pointer. Quieter in a
    /// long form, at the price of a button nobody finds who is looking for it.
    /// </summary>
    WhenFocusedOrHovered,

    /// <summary>
    /// Always - even on an empty field. For layouts where things must not move: the button keeps its
    /// place instead of appearing with the first character and shifting everything beside it.
    /// </summary>
    Always,
}
