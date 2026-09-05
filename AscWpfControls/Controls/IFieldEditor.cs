using System;

namespace AscWpfControls.Controls;

/// <summary>
/// What a <see cref="FieldAction"/> needs to know about the field it sits in.
///
/// <b>A narrow role, not a type.</b> A clear button does not want a <c>TextBox</c>; it wants
/// something that can say whether it is empty and can empty itself. Keeping it to that means the
/// same button works in a field built on an entirely different editor - a password box, say, whose
/// content never passes through a <c>Text</c> property at all.
///
/// Everything here is about state a button might have to react to. There is deliberately no way to
/// read the content: an action that needed the value would be doing something other than acting on
/// the field.
/// </summary>
public interface IFieldEditor
{
    /// <summary>Whether the field currently holds nothing.</summary>
    bool IsEmpty { get; }

    /// <summary>
    /// Whether the field refuses edits. Actions that change the content hide themselves here rather
    /// than sitting there greyed out - a control that cannot work is better absent than disabled.
    /// </summary>
    bool IsReadOnly { get; }

    /// <summary>
    /// Whether the user is currently at this field - it has keyboard focus, or the pointer is over
    /// it. Only of interest to actions that appear on approach.
    /// </summary>
    bool IsActive { get; }

    /// <summary>Empties the field.</summary>
    void Clear();

    /// <summary>
    /// Puts the caret back into the field. Called after an action has run: whoever clears a field is
    /// about to type, and leaving the focus on the button would cost them a click.
    /// </summary>
    void FocusEditor();

    /// <summary>
    /// Raised whenever any of the above may have changed. One event for all of it, because an action
    /// re-evaluates its whole situation anyway - splitting it into four would only mean four
    /// subscriptions ending in the same method.
    /// </summary>
    event EventHandler? StateChanged;
}
