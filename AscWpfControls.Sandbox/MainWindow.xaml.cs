using System.Windows;
using System.Windows.Input;
using AscWpfControls.Controls;

namespace AscWpfControls.Sandbox;

/// <summary>
/// Probierstube fuer die Controls aus AscWpfControls. Zeigt beim RotaryEncoder vor allem eines:
/// Die Schrittweite gehoert nicht ins Control. Der Kurs-Regler leitet sie aus dem Kursniveau ab,
/// der Stueck-Regler nimmt eine Raste als ein Stueck - beide bekommen dieselben Rasten.
/// </summary>
public partial class MainWindow : Window
{
    private decimal _kurs = 100m;
    private int _stueck;
    private int _linear;

    public MainWindow()
    {
        InitializeComponent();

        StueckRegler.TickCommand = new TickCommand(rasten =>
        {
            _stueck = Math.Max(0, _stueck + rasten);
            StueckAnzeige.Text = _stueck.ToString("N0");
            Protokolliere("Stück", rasten, _stueck.ToString("N0"));
        });

        ZeigeKurs();
    }

    private void KursRegler_Ticked(object sender, RotaryEncoderTickEventArgs e)
    {
        _kurs = Math.Max(0, _kurs + e.Delta * Schrittweite(_kurs));
        ZeigeKurs();
        Protokolliere("Kurs", e.Delta, _kurs.ToString("N2"));
    }

    private void LinearRegler_Ticked(object sender, RotaryEncoderTickEventArgs e)
    {
        _linear += e.Delta;
        LinearAnzeige.Text = _linear.ToString("N0");
        Protokolliere("Linear", e.Delta, _linear.ToString("N0"));
    }

    private void Variante_Ticked(object sender, RotaryEncoderTickEventArgs e)
    {
        var name = (sender as FrameworkElement)?.Tag as string ?? "Variante";
        Protokolliere(name, e.Delta, "nur Optik");
    }

    /// <summary>
    /// Ein Cent ist bei einem 40-Euro-Papier richtig und bei einem 2000-Euro-Papier zaeh: Die
    /// Schrittweite waechst deshalb mit dem Kurs. Genau diese Entscheidung ist der Grund, warum das
    /// Control selbst keinen Wert kennt.
    /// </summary>
    private static decimal Schrittweite(decimal kurs) => kurs switch
    {
        < 10m => 0.01m,
        < 100m => 0.05m,
        < 1000m => 0.25m,
        _ => 1m,
    };

    private void ZeigeKurs()
    {
        KursAnzeige.Text = _kurs.ToString("N2");
        KursSchrittweite.Text = $"{Schrittweite(_kurs):N2} je Raste, abhängig vom Kursniveau";
    }

    private void Protokolliere(string quelle, int delta, string wert)
    {
        Protokoll.Items.Insert(0, $"{quelle,-7} {delta,+4} Rasten  →  {wert}");
        if (Protokoll.Items.Count > 200)
            Protokoll.Items.RemoveAt(Protokoll.Items.Count - 1);
    }

    private sealed class TickCommand : ICommand
    {
        private readonly Action<int> _ausfuehren;

        public TickCommand(Action<int> ausfuehren) => _ausfuehren = ausfuehren;

        // Der Regler fragt CanExecute bei jedem Tick neu - hier gibt es nichts zu melden.
        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (parameter is int rasten)
                _ausfuehren(rasten);
        }
    }
}
