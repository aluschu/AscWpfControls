namespace AscWpfControls.Controls;

/// <summary>
/// Die mitgelieferten Markierungen eines <see cref="RotaryEncoder"/>. Wer etwas anderes will,
/// tauscht weiterhin das ControlTemplate aus - diese drei sind die Abkuerzung dahin.
/// </summary>
public enum RotaryEncoderMarker
{
    /// <summary>Ein kurzer Strich am Rand, der die Mitte nicht erreicht.</summary>
    Notch,

    /// <summary>Ein Zeiger von der Nabe bis kurz vor den Rand, wie bei einer Uhr.</summary>
    Pointer,

    /// <summary>Ein Punkt nahe dem Rand, wie an vielen Hardware-Encodern.</summary>
    Dot,
}
