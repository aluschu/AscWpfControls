using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AscWpfControls.Controls;

/// <summary>
/// An endless rotary dial, modelled on the hardware encoders found on a mixing desk.
///
/// The control deliberately knows <b>no value</b>: no upper or lower bound, no number format, no
/// step size. All it reports is that it was turned - through <see cref="Ticked"/> and
/// <see cref="TickCommand"/>, with a signed number of detents. What a detent means is up to the
/// receiver: a cent on a price, one unit on a quantity, and the step size may well depend on the
/// magnitude of the value.
///
/// That separation keeps the door open for wiring a real hardware encoder (over MIDI, say) into the
/// same place later: it delivers detents just as readily, but never a formatted value. For the same
/// reason the control ignores modifier keys - an encoder on a mixing desk has no shift key. Anyone
/// wanting a finer turn reads <see cref="Keyboard.Modifiers"/> where the step size is decided
/// anyway.
/// </summary>
public class RotaryEncoder : Control
{
    /// <summary>
    /// Groups this control's own properties under a single heading in the Visual Studio property
    /// window. Without a category they all end up under "Misc", where they are nearly impossible to
    /// find among a control's inherited properties.
    /// </summary>
    private const string CategoryName = "Rotary Encoder";

    /// <summary>
    /// Reference speed for acceleration, in DIP per second: at this drag speed
    /// <see cref="AccelerationFactor"/> applies exactly once. Deliberately a constant - one more
    /// adjustment screw would only have made the dial harder to tune.
    /// </summary>
    private const double ReferenceSpeed = 1000.0;

    private POINT _anchor;
    private bool _turning;
    private double _pendingTicks;
    private long _lastTimestamp;

    static RotaryEncoder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(typeof(RotaryEncoder)));
        FocusableProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(true));
        // Without these two defaults a grid cell stretches the dial to its full size - it would
        // stay round, but its click area would be far too large.
        HorizontalAlignmentProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(HorizontalAlignment.Center));
        VerticalAlignmentProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(VerticalAlignment.Center));
    }

    // ----- Event and command -----

    public static readonly RoutedEvent TickedEvent = EventManager.RegisterRoutedEvent(
        nameof(Ticked), RoutingStrategy.Bubble,
        typeof(EventHandler<RotaryEncoderTickEventArgs>), typeof(RotaryEncoder));

    /// <summary>Raised on every turn - see <see cref="RotaryEncoderTickEventArgs.Delta"/>.</summary>
    [Category(CategoryName)]
    [Description("Reports a turn with the signed number of detents.")]
    public event EventHandler<RotaryEncoderTickEventArgs> Ticked
    {
        add => AddHandler(TickedEvent, value);
        remove => RemoveHandler(TickedEvent, value);
    }

    public static readonly DependencyProperty TickCommandProperty = DependencyProperty.Register(
        nameof(TickCommand), typeof(ICommand), typeof(RotaryEncoder));

    /// <summary>
    /// The same signal as <see cref="Ticked"/>, only as a command - so the dial can be bound to a
    /// view model without any code-behind. The parameter is the delta as an <c>int</c>.
    /// </summary>
    [Category(CategoryName)]
    [Description("Executed on every turn; the parameter is the number of detents.")]
    public ICommand? TickCommand
    {
        get => (ICommand?)GetValue(TickCommandProperty);
        set => SetValue(TickCommandProperty, value);
    }

    // ----- Settings -----

    public static readonly DependencyProperty PixelsPerTickProperty = DependencyProperty.Register(
        nameof(PixelsPerTick), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(6.0));

    /// <summary>How far a calm drag has to travel before one detent occurs (in DIP).</summary>
    [Category(CategoryName)]
    [Description("How far a calm drag has to travel before one detent occurs.")]
    public double PixelsPerTick
    {
        get => (double)GetValue(PixelsPerTickProperty);
        set => SetValue(PixelsPerTickProperty, value);
    }

    public static readonly DependencyProperty AccelerationFactorProperty = DependencyProperty.Register(
        nameof(AccelerationFactor), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(3.0));

    /// <summary>
    /// How strongly a fast drag multiplies the detents. <c>0</c> turns acceleration off and makes
    /// the dial strictly linear.
    /// </summary>
    [Category(CategoryName)]
    [Description("How strongly a fast drag multiplies the detents. 0 turns acceleration off.")]
    public double AccelerationFactor
    {
        get => (double)GetValue(AccelerationFactorProperty);
        set => SetValue(AccelerationFactorProperty, value);
    }

    public static readonly DependencyProperty MaxTicksPerEventProperty = DependencyProperty.Register(
        nameof(MaxTicksPerEvent), typeof(int), typeof(RotaryEncoder), new PropertyMetadata(32));

    /// <summary>
    /// An upper bound per event, so that one jerk of the mouse does not skip half the range.
    /// Anything beyond it is discarded - that is what the limit is for.
    /// </summary>
    [Category(CategoryName)]
    [Description("Upper bound of detents per event, so a jerk does not skip half the range.")]
    public int MaxTicksPerEvent
    {
        get => (int)GetValue(MaxTicksPerEventProperty);
        set => SetValue(MaxTicksPerEventProperty, value);
    }

    public static readonly DependencyProperty TicksPerPageProperty = DependencyProperty.Register(
        nameof(TicksPerPage), typeof(int), typeof(RotaryEncoder), new PropertyMetadata(10));

    /// <summary>How many detents Page Up and Page Down report.</summary>
    [Category(CategoryName)]
    [Description("How many detents the Page Up and Page Down keys report.")]
    public int TicksPerPage
    {
        get => (int)GetValue(TicksPerPageProperty);
        set => SetValue(TicksPerPageProperty, value);
    }

    public static readonly DependencyProperty DegreesPerTickProperty = DependencyProperty.Register(
        nameof(DegreesPerTick), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(12.0));

    /// <summary>How far the marker rotates per detent, in degrees - purely cosmetic.</summary>
    [Category(CategoryName)]
    [Description("How far the marker rotates per detent, in degrees.")]
    public double DegreesPerTick
    {
        get => (double)GetValue(DegreesPerTickProperty);
        set => SetValue(DegreesPerTickProperty, value);
    }

    public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
        nameof(Diameter), typeof(double), typeof(RotaryEncoder),
        new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// The size of the knob - one number instead of two, because a dial is round.
    ///
    /// Setting <see cref="FrameworkElement.Width"/> and <see cref="FrameworkElement.Height"/>
    /// instead still does not produce an egg: the template always draws the knob scaled evenly and
    /// centred within whatever area the layout hands it.
    /// </summary>
    [Category(CategoryName)]
    [Description("The size of the round knob - one number instead of Width and Height.")]
    public double Diameter
    {
        get => (double)GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public static readonly DependencyProperty MarkerKindProperty = DependencyProperty.Register(
        nameof(MarkerKind), typeof(RotaryEncoderMarker), typeof(RotaryEncoder),
        new PropertyMetadata(RotaryEncoderMarker.Notch));

    /// <summary>Which of the supplied markers the knob carries.</summary>
    [Category(CategoryName)]
    [Description("The marker on the knob: notch, pointer or dot.")]
    public RotaryEncoderMarker MarkerKind
    {
        get => (RotaryEncoderMarker)GetValue(MarkerKindProperty);
        set => SetValue(MarkerKindProperty, value);
    }

    public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(
        nameof(MarkerBrush), typeof(Brush), typeof(RotaryEncoder));

    /// <summary>
    /// The colour of the marker. A property of its own rather than <see cref="Control.Foreground"/>,
    /// so that the marker can be tinted without the knob's rim following along.
    ///
    /// Named <c>Brush</c> and not <c>Color</c>, because a brush may also be a gradient - and because
    /// WPF does the same with <see cref="Control.BorderBrush"/> and its kin.
    /// </summary>
    [Category(CategoryName)]
    [Description("The colour of the marker on the knob.")]
    public Brush? MarkerBrush
    {
        get => (Brush?)GetValue(MarkerBrushProperty);
        set => SetValue(MarkerBrushProperty, value);
    }

    public static readonly DependencyProperty FocusRingBrushProperty = DependencyProperty.Register(
        nameof(FocusRingBrush), typeof(Brush), typeof(RotaryEncoder));

    /// <summary>
    /// The colour of the dotted ring that indicates keyboard focus. Set it to <c>Transparent</c> and
    /// it disappears - for anyone who prefers it less conspicuous.
    /// </summary>
    [Category(CategoryName)]
    [Description("The colour of the focus ring. Transparent hides it.")]
    public Brush? FocusRingBrush
    {
        get => (Brush?)GetValue(FocusRingBrushProperty);
        set => SetValue(FocusRingBrushProperty, value);
    }

    /// <summary>
    /// The natural size is always square. A <see cref="FrameworkElement.Width"/> or a stretching
    /// alignment can override it from the outside - what pushes back then is the template, not the
    /// measurement.
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
        var side = Math.Max(0, Diameter);
        base.MeasureOverride(new Size(side, side));
        return new Size(side, side);
    }

    // ----- Feedback to the template -----

    private static readonly DependencyPropertyKey AnglePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Angle), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(0.0));

    public static readonly DependencyProperty AngleProperty = AnglePropertyKey.DependencyProperty;

    /// <summary>
    /// The position of the marker in degrees. Purely relative: it shows that a turn is happening and
    /// how fast, and never a value - the control knows none.
    /// </summary>
    [Browsable(false)]
    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        private set => SetValue(AnglePropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsTurningPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsTurning), typeof(bool), typeof(RotaryEncoder), new PropertyMetadata(false));

    public static readonly DependencyProperty IsTurningProperty = IsTurningPropertyKey.DependencyProperty;

    /// <summary>Whether a drag is in progress - for template triggers.</summary>
    [Browsable(false)]
    public bool IsTurning
    {
        get => (bool)GetValue(IsTurningProperty);
        private set => SetValue(IsTurningPropertyKey, value);
    }

    // ----- Interaction -----

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (!GetCursorPos(out _anchor) || !CaptureMouse())
            return;

        _turning = true;
        _pendingTicks = 0;
        _lastTimestamp = Stopwatch.GetTimestamp();
        IsTurning = true;
        // The pointer disappears for the duration of the turn: it stays put anyway, and a motionless
        // mouse pointer during a movement would only be confusing.
        Cursor = Cursors.None;
        Focus();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_turning || !GetCursorPos(out var current))
            return;

        // Dragging upwards should count up, while the screen axis points down.
        var devicePixels = _anchor.Y - current.Y;
        if (devicePixels == 0)
            return;

        // The pointer goes straight back to where it started. That is exactly what makes the dial
        // endless: there is no edge of the screen where the movement stops.
        SetCursorPos(_anchor.X, _anchor.Y);

        var scale = VisualTreeHelper.GetDpi(this).DpiScaleY;
        var distance = devicePixels / (scale > 0 ? scale : 1.0);

        var timestamp = Stopwatch.GetTimestamp();
        var seconds = (timestamp - _lastTimestamp) / (double)Stopwatch.Frequency;
        _lastTimestamp = timestamp;

        var speed = seconds > 0 ? Math.Abs(distance) / seconds : 0;
        var acceleration = 1 + AccelerationFactor * (speed / ReferenceSpeed);

        // The remainder is carried over, so that even a slow drag eventually yields a detent instead
        // of throwing fractions away.
        _pendingTicks += distance * acceleration / Math.Max(PixelsPerTick, 0.1);
        var ticks = (int)Math.Truncate(_pendingTicks);
        if (ticks == 0)
            return;

        _pendingTicks -= ticks;
        Report(ticks);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_turning)
            return;

        ReleaseMouseCapture();
        e.Handled = true;
    }

    /// <summary>
    /// Cleaning up hangs off the loss of the mouse, not off the button being released: that way the
    /// pointer and the state come back even when the capture ends some other way.
    /// </summary>
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);

        if (!_turning)
            return;

        _turning = false;
        IsTurning = false;
        SetCursorPos(_anchor.X, _anchor.Y);
        ClearValue(CursorProperty);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        var ticks = e.Delta / Mouse.MouseWheelDeltaForOneLine;
        if (ticks == 0)
            return;

        Report(ticks);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var ticks = e.Key switch
        {
            Key.Up => 1,
            Key.Down => -1,
            Key.PageUp => TicksPerPage,
            Key.PageDown => -TicksPerPage,
            _ => 0,
        };

        if (ticks == 0)
            return;

        Report(ticks);
        e.Handled = true;
    }

    private void Report(int ticks)
    {
        ticks = Math.Clamp(ticks, -MaxTicksPerEvent, MaxTicksPerEvent);
        if (ticks == 0)
            return;

        Angle = (Angle + ticks * DegreesPerTick) % 360;

        RaiseEvent(new RotaryEncoderTickEventArgs(TickedEvent, this, ticks));

        if (TickCommand is { } command && command.CanExecute(ticks))
            command.Execute(ticks);
    }

    // ----- Win32 -----

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // Deliberately DllImport rather than LibraryImport: the latter demands AllowUnsafeBlocks across
    // the whole project, and two calls are too thin a reason for that.
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);
}
