using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AscWpfControls.Controls;

/// <summary>
/// Ein Endlos-Drehregler nach dem Vorbild eines Hardware-Encoders am Mischpult.
///
/// Das Control kennt bewusst <b>keinen Wert</b>: keine Ober- oder Untergrenze, kein Zahlenformat,
/// keine Schrittweite. Es meldet ausschliesslich, dass gedreht wurde - per <see cref="Ticked"/> und
/// <see cref="TickCommand"/>, mit einer vorzeichenbehafteten Anzahl von Rasten. Was eine Raste
/// bedeutet, entscheidet der Empfaenger: ein Cent bei einem Kurs, ein Stueck bei einer Menge, und
/// die Schrittweite darf sich dort am Wertniveau orientieren.
///
/// Diese Trennung haelt die Tuer offen, spaeter einen echten Hardware-Encoder (etwa ueber MIDI) an
/// dieselbe Stelle zu haengen: Rasten liefert der genauso, einen formatierten Wert nicht. Aus dem
/// gleichen Grund wertet das Control auch keine Modifiertasten aus - ein Encoder am Mischpult hat
/// keine Umschalttaste. Wer feiner drehen will, liest <see cref="Keyboard.Modifiers"/> dort, wo
/// ohnehin ueber die Schrittweite entschieden wird.
/// </summary>
public class RotaryEncoder : Control
{
    /// <summary>
    /// Sammelt die eigenen Eigenschaften im Eigenschaftenfenster von Visual Studio unter einer
    /// gemeinsamen Ueberschrift - ohne Kategorie landen sie alle unter "Sonstiges" und sind zwischen
    /// den geerbten Eigenschaften eines Controls kaum zu finden.
    /// </summary>
    private const string Kategorie = "Rotary Encoder";

    /// <summary>
    /// Bezugstempo der Beschleunigung in DIP je Sekunde: Bei dieser Ziehgeschwindigkeit wirkt
    /// <see cref="AccelerationFactor"/> genau einfach. Bewusst eine Konstante - eine weitere
    /// Stellschraube haette den Regler nur schwerer einstellbar gemacht.
    /// </summary>
    private const double TempoBezug = 1000.0;

    private POINT _anker;
    private bool _dreht;
    private double _restRasten;
    private long _letzterZeitstempel;

    static RotaryEncoder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(typeof(RotaryEncoder)));
        FocusableProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(true));
        // Ohne diese beiden Vorgaben zieht eine Grid-Zelle den Regler auf ihre volle Groesse - er
        // bliebe zwar rund, haette aber eine viel zu grosse Klickflaeche.
        HorizontalAlignmentProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(HorizontalAlignment.Center));
        VerticalAlignmentProperty.OverrideMetadata(typeof(RotaryEncoder),
            new FrameworkPropertyMetadata(VerticalAlignment.Center));
    }

    // ----- Ereignis und Command -----

    public static readonly RoutedEvent TickedEvent = EventManager.RegisterRoutedEvent(
        nameof(Ticked), RoutingStrategy.Bubble,
        typeof(EventHandler<RotaryEncoderTickEventArgs>), typeof(RotaryEncoder));

    /// <summary>Wird bei jeder Drehung ausgeloest - siehe <see cref="RotaryEncoderTickEventArgs.Delta"/>.</summary>
    [Category(Kategorie)]
    [Description("Meldet eine Drehung mit der vorzeichenbehafteten Anzahl von Rasten.")]
    public event EventHandler<RotaryEncoderTickEventArgs> Ticked
    {
        add => AddHandler(TickedEvent, value);
        remove => RemoveHandler(TickedEvent, value);
    }

    public static readonly DependencyProperty TickCommandProperty = DependencyProperty.Register(
        nameof(TickCommand), typeof(ICommand), typeof(RotaryEncoder));

    /// <summary>
    /// Derselbe Anstoss wie <see cref="Ticked"/>, nur als Command - damit sich der Regler ohne
    /// Code-Behind an ein ViewModel binden laesst. Der Parameter ist das Delta als <c>int</c>.
    /// </summary>
    [Category(Kategorie)]
    [Description("Wird bei jeder Drehung ausgefuehrt; der Parameter ist die Anzahl der Rasten.")]
    public ICommand? TickCommand
    {
        get => (ICommand?)GetValue(TickCommandProperty);
        set => SetValue(TickCommandProperty, value);
    }

    // ----- Einstellungen -----

    public static readonly DependencyProperty PixelsPerTickProperty = DependencyProperty.Register(
        nameof(PixelsPerTick), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(6.0));

    /// <summary>Wie weit bei ruhiger Bewegung gezogen werden muss, bis eine Raste entsteht (in DIP).</summary>
    [Category(Kategorie)]
    [Description("Wie weit bei ruhiger Bewegung gezogen werden muss, bis eine Raste entsteht.")]
    public double PixelsPerTick
    {
        get => (double)GetValue(PixelsPerTickProperty);
        set => SetValue(PixelsPerTickProperty, value);
    }

    public static readonly DependencyProperty AccelerationFactorProperty = DependencyProperty.Register(
        nameof(AccelerationFactor), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(3.0));

    /// <summary>
    /// Wie stark schnelles Ziehen die Rasten vervielfacht. <c>0</c> schaltet die Beschleunigung ab
    /// und macht den Regler streng linear.
    /// </summary>
    [Category(Kategorie)]
    [Description("Wie stark schnelles Ziehen die Rasten vervielfacht. 0 schaltet die Beschleunigung ab.")]
    public double AccelerationFactor
    {
        get => (double)GetValue(AccelerationFactorProperty);
        set => SetValue(AccelerationFactorProperty, value);
    }

    public static readonly DependencyProperty MaxTicksPerEventProperty = DependencyProperty.Register(
        nameof(MaxTicksPerEvent), typeof(int), typeof(RotaryEncoder), new PropertyMetadata(32));

    /// <summary>
    /// Obergrenze je Ereignis, damit ein Ruck mit der Maus nicht den halben Wertebereich
    /// ueberspringt. Was darueber hinausgeht, wird verworfen - das ist der Zweck der Grenze.
    /// </summary>
    [Category(Kategorie)]
    [Description("Obergrenze der Rasten je Ereignis, damit ein Ruck nicht den halben Wertebereich ueberspringt.")]
    public int MaxTicksPerEvent
    {
        get => (int)GetValue(MaxTicksPerEventProperty);
        set => SetValue(MaxTicksPerEventProperty, value);
    }

    public static readonly DependencyProperty TicksPerPageProperty = DependencyProperty.Register(
        nameof(TicksPerPage), typeof(int), typeof(RotaryEncoder), new PropertyMetadata(10));

    /// <summary>Wie viele Rasten Bild auf/ab meldet.</summary>
    [Category(Kategorie)]
    [Description("Wie viele Rasten die Tasten Bild auf und Bild ab melden.")]
    public int TicksPerPage
    {
        get => (int)GetValue(TicksPerPageProperty);
        set => SetValue(TicksPerPageProperty, value);
    }

    public static readonly DependencyProperty DegreesPerTickProperty = DependencyProperty.Register(
        nameof(DegreesPerTick), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(12.0));

    /// <summary>Um wie viel Grad sich die Markierung je Raste weiterdreht - reine Optik.</summary>
    [Category(Kategorie)]
    [Description("Um wie viel Grad sich die Markierung je Raste weiterdreht.")]
    public double DegreesPerTick
    {
        get => (double)GetValue(DegreesPerTickProperty);
        set => SetValue(DegreesPerTickProperty, value);
    }

    public static readonly DependencyProperty DiameterProperty = DependencyProperty.Register(
        nameof(Diameter), typeof(double), typeof(RotaryEncoder),
        new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Die Groesse des Knopfes - eine Zahl statt zweier, denn ein Drehregler ist rund.
    ///
    /// Wer stattdessen <see cref="FrameworkElement.Width"/> und <see cref="FrameworkElement.Height"/>
    /// setzt, bekommt trotzdem kein Ei: Das Template zeichnet den Knopf immer gleichmaessig skaliert
    /// und mittig in die Flaeche, die das Layout ihm zuweist.
    /// </summary>
    [Category(Kategorie)]
    [Description("Die Groesse des runden Knopfes - eine Zahl statt Width und Height.")]
    public double Diameter
    {
        get => (double)GetValue(DiameterProperty);
        set => SetValue(DiameterProperty, value);
    }

    public static readonly DependencyProperty MarkerKindProperty = DependencyProperty.Register(
        nameof(MarkerKind), typeof(RotaryEncoderMarker), typeof(RotaryEncoder),
        new PropertyMetadata(RotaryEncoderMarker.Notch));

    /// <summary>Welche der mitgelieferten Markierungen der Knopf traegt.</summary>
    [Category(Kategorie)]
    [Description("Die Markierung auf dem Knopf: Kerbe, Zeiger oder Punkt.")]
    public RotaryEncoderMarker MarkerKind
    {
        get => (RotaryEncoderMarker)GetValue(MarkerKindProperty);
        set => SetValue(MarkerKindProperty, value);
    }

    public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(
        nameof(MarkerBrush), typeof(Brush), typeof(RotaryEncoder));

    /// <summary>
    /// Die Farbe der Markierung. Eine eigene Eigenschaft statt <see cref="Control.Foreground"/>, damit
    /// sich die Markierung faerben laesst, ohne dass der Knopfrand mitgeht.
    ///
    /// Heisst <c>Brush</c> und nicht <c>Color</c>, weil ein Pinsel auch ein Verlauf sein darf - und
    /// weil WPF es bei <see cref="Control.BorderBrush"/> und Konsorten genauso haelt.
    /// </summary>
    [Category(Kategorie)]
    [Description("Die Farbe der Markierung auf dem Knopf.")]
    public Brush? MarkerBrush
    {
        get => (Brush?)GetValue(MarkerBrushProperty);
        set => SetValue(MarkerBrushProperty, value);
    }

    public static readonly DependencyProperty FocusRingBrushProperty = DependencyProperty.Register(
        nameof(FocusRingBrush), typeof(Brush), typeof(RotaryEncoder));

    /// <summary>
    /// Die Farbe des gepunkteten Rings, der den Tastaturfokus anzeigt. Auf <c>Transparent</c> gesetzt
    /// verschwindet er - fuer alle, die es unauffaelliger moegen.
    /// </summary>
    [Category(Kategorie)]
    [Description("Die Farbe des Fokusrings. Transparent blendet ihn aus.")]
    public Brush? FocusRingBrush
    {
        get => (Brush?)GetValue(FocusRingBrushProperty);
        set => SetValue(FocusRingBrushProperty, value);
    }

    /// <summary>
    /// Die natuerliche Groesse ist immer quadratisch. Ein <see cref="FrameworkElement.Width"/> oder
    /// eine gestreckte Ausrichtung koennen sie von aussen ueberstimmen - dagegen haelt dann das
    /// Template dagegen, nicht die Messung.
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
        var seite = Math.Max(0, Diameter);
        base.MeasureOverride(new Size(seite, seite));
        return new Size(seite, seite);
    }

    // ----- Rueckmeldung an das Template -----

    private static readonly DependencyPropertyKey AnglePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Angle), typeof(double), typeof(RotaryEncoder), new PropertyMetadata(0.0));

    public static readonly DependencyProperty AngleProperty = AnglePropertyKey.DependencyProperty;

    /// <summary>
    /// Die Stellung der Markierung in Grad. Rein relativ: Sie zeigt, dass und wie schnell gedreht
    /// wird, und niemals einen Wert - das Control kennt keinen.
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

    /// <summary>Ob gerade gezogen wird - fuer Template-Trigger.</summary>
    [Browsable(false)]
    public bool IsTurning
    {
        get => (bool)GetValue(IsTurningProperty);
        private set => SetValue(IsTurningPropertyKey, value);
    }

    // ----- Bedienung -----

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (!GetCursorPos(out _anker) || !CaptureMouse())
            return;

        _dreht = true;
        _restRasten = 0;
        _letzterZeitstempel = Stopwatch.GetTimestamp();
        IsTurning = true;
        // Der Zeiger verschwindet fuer die Dauer der Drehung: Er bleibt ohnehin auf der Stelle,
        // ein stehender Mauszeiger waehrend einer Bewegung waere nur verwirrend.
        Cursor = Cursors.None;
        Focus();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_dreht || !GetCursorPos(out var jetzt))
            return;

        // Nach oben ziehen soll aufwaerts zaehlen, die Bildschirmachse zeigt nach unten.
        var geraetepixel = _anker.Y - jetzt.Y;
        if (geraetepixel == 0)
            return;

        // Der Zeiger geht sofort an seinen Ausgangspunkt zurueck. Genau das macht den Regler
        // endlos: Es gibt keinen Bildschirmrand, an dem die Bewegung aufhoert.
        SetCursorPos(_anker.X, _anker.Y);

        var skalierung = VisualTreeHelper.GetDpi(this).DpiScaleY;
        var strecke = geraetepixel / (skalierung > 0 ? skalierung : 1.0);

        var zeitstempel = Stopwatch.GetTimestamp();
        var sekunden = (zeitstempel - _letzterZeitstempel) / (double)Stopwatch.Frequency;
        _letzterZeitstempel = zeitstempel;

        var tempo = sekunden > 0 ? Math.Abs(strecke) / sekunden : 0;
        var beschleunigung = 1 + AccelerationFactor * (tempo / TempoBezug);

        // Der Rest bleibt stehen, damit auch langsames Ziehen irgendwann eine Raste ergibt, statt
        // Bruchteile wegzuwerfen.
        _restRasten += strecke * beschleunigung / Math.Max(PixelsPerTick, 0.1);
        var rasten = (int)Math.Truncate(_restRasten);
        if (rasten == 0)
            return;

        _restRasten -= rasten;
        Melde(rasten);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_dreht)
            return;

        ReleaseMouseCapture();
        e.Handled = true;
    }

    /// <summary>
    /// Das Aufraeumen haengt am Verlust der Maus, nicht am Loslassen der Taste: So kommen Zeiger und
    /// Zustand auch dann zurueck, wenn die Erfassung anderweitig endet.
    /// </summary>
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);

        if (!_dreht)
            return;

        _dreht = false;
        IsTurning = false;
        SetCursorPos(_anker.X, _anker.Y);
        ClearValue(CursorProperty);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        var rasten = e.Delta / Mouse.MouseWheelDeltaForOneLine;
        if (rasten == 0)
            return;

        Melde(rasten);
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var rasten = e.Key switch
        {
            Key.Up => 1,
            Key.Down => -1,
            Key.PageUp => TicksPerPage,
            Key.PageDown => -TicksPerPage,
            _ => 0,
        };

        if (rasten == 0)
            return;

        Melde(rasten);
        e.Handled = true;
    }

    private void Melde(int rasten)
    {
        rasten = Math.Clamp(rasten, -MaxTicksPerEvent, MaxTicksPerEvent);
        if (rasten == 0)
            return;

        Angle = (Angle + rasten * DegreesPerTick) % 360;

        RaiseEvent(new RotaryEncoderTickEventArgs(TickedEvent, this, rasten));

        if (TickCommand is { } command && command.CanExecute(rasten))
            command.Execute(rasten);
    }

    // ----- Win32 -----

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    // Bewusst DllImport statt LibraryImport: Letzteres verlangt AllowUnsafeBlocks im ganzen
    // Projekt, und dafuer sind zwei Aufrufe ein zu duenner Anlass.
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out POINT punkt);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetCursorPos(int x, int y);
}
