using System.Windows;

namespace AscWpfControls.Controls;

/// <summary>
/// Reports that a <see cref="RotaryEncoder"/> was turned.
/// </summary>
public sealed class RotaryEncoderTickEventArgs : RoutedEventArgs
{
    public RotaryEncoderTickEventArgs(RoutedEvent routedEvent, object source, int delta)
        : base(routedEvent, source)
    {
        Delta = delta;
    }

    /// <summary>
    /// The number of detents, signed: positive upwards, negative downwards.
    ///
    /// Usually 1 or -1. On a fast turn one event bundles several detents rather than sending many
    /// events - the receiver computes <c>value += Delta * stepSize</c> and never has to count
    /// events.
    /// </summary>
    public int Delta { get; }
}
