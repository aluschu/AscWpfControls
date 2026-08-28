# AscWpfControls

Eigene WPF-Controls für .NET 10. Ohne Abhängigkeiten außer WPF selbst — die Bibliothek lässt sich also auch in Projekte einbinden, die kein Control-Paket eines Herstellers verwenden.

## RotaryEncoder

Ein Endlos-Drehregler nach dem Vorbild eines Hardware-Encoders am Mischpult.

**Das Control kennt keinen Wert.** Keine Ober- oder Untergrenze, kein Zahlenformat, keine Schrittweite — es meldet ausschließlich, dass gedreht wurde, mit einer vorzeichenbehafteten Anzahl von Rasten. Was eine Raste bedeutet, entscheidet der Empfänger: ein Cent bei einem Kurs, ein Stück bei einer Menge. Die Schrittweite darf sich dort am Wertniveau orientieren, ohne dass das Control davon weiß.

Das ist nicht nur eine saubere Trennung: Ein echter Hardware-Encoder liefert genauso Rasten und keinen formatierten Wert. Wer später einen anschließen will, tauscht die Quelle und nicht den Empfänger.

```xml
<asc:RotaryEncoder Diameter="48" MarkerKind="Pointer" Ticked="Kurs_Ticked" />
```

```csharp
private void Kurs_Ticked(object sender, RotaryEncoderTickEventArgs e)
{
    // Ein Cent ist bei einem 40-Euro-Papier richtig und bei einem 2000-Euro-Papier zäh -
    // deshalb entscheidet hier die Schrittweite, nicht im Control.
    _kurs = Math.Max(0, _kurs + e.Delta * Schrittweite(_kurs));
}
```

Ohne Code-Behind geht es genauso: `TickCommand` bekommt die Anzahl der Rasten als Parameter.

### Bedienung

| Eingabe | Wirkung |
|---------|---------|
| Ziehen mit der linken Maustaste | Der Zeiger verschwindet und bleibt stehen, gedreht wird beliebig weit — kein Bildschirmrand, kein Anschlag |
| Mausrad | Eine Raste je Rastung |
| Pfeiltasten hoch/runter | Eine Raste |
| Bild auf/ab | `TicksPerPage` Rasten |

Langsames Ziehen trifft eine einzelne Raste, schnelles Ziehen überbrückt große Abstände: Das Delta wächst mit der Ziehgeschwindigkeit. Ein Ereignis je Impuls trägt die Anzahl — der Empfänger rechnet `Wert += Delta * Schrittweite` und muss keine Ereignisse zählen.

Modifiertasten wertet das Control bewusst **nicht** aus. Ein Encoder am Mischpult hat keine Umschalttaste; wer feiner drehen will, liest `Keyboard.Modifiers` dort, wo ohnehin über die Schrittweite entschieden wird.

### Eigenschaften

| Eigenschaft | Standard | Bedeutung |
|-------------|----------|-----------|
| `Diameter` | `26` | Die Größe des Knopfes — eine Zahl statt `Width` und `Height` |
| `MarkerKind` | `Notch` | Die Markierung: `Notch`, `Pointer` oder `Dot` |
| `MarkerBrush` | Blau | Farbe der Markierung, unabhängig vom `Foreground` des Knopfrands |
| `FocusRingBrush` | Blau | Farbe des Fokusrings; `Transparent` blendet ihn aus |
| `PixelsPerTick` | `6` | Ziehstrecke je Raste bei ruhiger Bewegung |
| `AccelerationFactor` | `3` | Wie stark schnelles Ziehen die Rasten vervielfacht; `0` schaltet die Beschleunigung ab |
| `MaxTicksPerEvent` | `32` | Obergrenze je Ereignis, damit ein Ruck nicht den halben Wertebereich überspringt |
| `TicksPerPage` | `10` | Rasten für Bild auf/ab |
| `DegreesPerTick` | `12` | Drehung der Markierung je Raste — reine Optik |

`Ticked` als Ereignis und `TickCommand` als Command melden dasselbe. Die Nur-Lese-Werte `Angle` und `IsTurning` sind für eigene Templates gedacht.

Der Knopf bleibt bei jeder Größe rund: Gezeichnet wird auf einer festen Fläche, die eine `Viewbox` gleichmäßig skaliert. Auch ungleiche `Width`/`Height` oder ein streckender Container ergeben kein Ei, sondern einen zentrierten Kreis. Der Fokusring liegt innerhalb der eigenen Fläche und überlappt deshalb keine Nachbarn.

## Probierstube

`AscWpfControls.Sandbox` ist eine kleine WPF-Anwendung zum Ausprobieren: verschiedene Größen, alle Markierungen, ein Regler mit und einer ohne Beschleunigung, dazu ein Protokoll der gemeldeten Rasten. Ein Bedienelement muss man anfassen können, um es zu beurteilen.

## Paket bauen und lokal veröffentlichen

```
dotnet pack AscWpfControls\AscWpfControls.csproj -c Release
dotnet nuget push AscWpfControls\bin\Release\Aluschu.WpfControls.1.0.0.nupkg -s C:\LocalNuget
```

`C:\LocalNuget` ist der als *Local Dev Feed* eingerichtete Ordner-Feed.

## Entstehung

Diese Bibliothek ist im Dialog mit **Claude** (Anthropic, über Claude Code) entstanden. Spezifikation, Code und diese Dokumentation hat Claude geschrieben; Anforderungen, Entwurfsentscheidungen und das Erproben am laufenden Programm kamen von Axel Schubert.

Die tragende Idee stammt dabei nicht von der Maschine: dass das Control **keinen Wert kennt**, sondern nur Rasten meldet, war die Vorgabe des Autors — und erst dadurch ist aus einem Zahlenfeld mit Drehknopf ein Bedienelement geworden, das später auch ein echter Hardware-Encoder bedienen kann.

## Lizenz

MIT — siehe [LICENSE.txt](LICENSE.txt).
