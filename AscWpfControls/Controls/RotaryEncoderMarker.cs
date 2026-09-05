namespace AscWpfControls.Controls;

/// <summary>
/// The markers supplied with a <see cref="RotaryEncoder"/>. Anyone wanting something else still
/// replaces the ControlTemplate - these three are the shortcut there.
/// </summary>
public enum RotaryEncoderMarker
{
    /// <summary>A short line at the rim that stops short of the centre.</summary>
    Notch,

    /// <summary>A pointer from the hub to just short of the rim, like a clock hand.</summary>
    Pointer,

    /// <summary>A dot near the rim, as on many hardware encoders.</summary>
    Dot,
}
