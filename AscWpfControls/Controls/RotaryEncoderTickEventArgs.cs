using System.Windows;

namespace AscWpfControls.Controls;

/// <summary>
/// Meldet, dass an einem <see cref="RotaryEncoder"/> gedreht wurde.
/// </summary>
public sealed class RotaryEncoderTickEventArgs : RoutedEventArgs
{
    public RotaryEncoderTickEventArgs(RoutedEvent routedEvent, object source, int delta)
        : base(routedEvent, source)
    {
        Delta = delta;
    }

    /// <summary>
    /// Die Anzahl der Rasten, vorzeichenbehaftet: positiv nach oben, negativ nach unten.
    ///
    /// Normalerweise 1 beziehungsweise -1. Bei schnellem Drehen fasst ein Ereignis mehrere Rasten
    /// zusammen, statt viele Ereignisse zu schicken - der Empfaenger rechnet
    /// <c>Wert += Delta * Schrittweite</c> und muss keine Ereignisse zaehlen.
    /// </summary>
    public int Delta { get; }
}
