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

## TextField

Ein Textfeld mit **Platzhalter** und einer **Knopfleiste am rechten Innenrand**. Es erbt von
`TextBox` — Bindung, Auswahl, Rückgängig, Zwischenablage und Rechtschreibprüfung sind also
unverändert da, und wer `<TextBox>` durch `<asc:TextField>` ersetzt, verliert nichts.

```xml
<asc:TextField Text="{Binding Symbol}" PlaceholderText="Symbol oder ISIN">
    <asc:ClearAction />
</asc:TextField>
```

Der **Platzhalter** ist kein Wert: Er wird nicht gebunden, nicht gespeichert und nicht ausgelesen.
Wo er sagt, was gemeint ist, kann die Beschriftung daneben kurz ausfallen oder ganz entfallen.

Die Knöpfe sind eine **Sammlung**, kein Satz von Schaltern — der dritte kostet eine kleine Klasse
und keine weitere Eigenschaft am Feld.

### Verdecken ist kein Schutz

> `IsSecret` verdeckt vor **Blicken**: dem Schlüssel auf einem geteilten Bildschirm, dem Screenshot,
> der gleich verschickt wird. **Es ist keine Sicherheitskomponente.** Der Wert steht als gewöhnliche
> Zeichenkette im Speicher wie in jedem Textfeld, und keine Einstellung ändert daran etwas.
>
> Wer Schutz vor jemandem mit Zugriff auf die Maschine braucht, nimmt die native `PasswordBox` — und
> verzichtet dafür aufs Aufdecken, denn die kann grundsätzlich keinen Klartext zeigen.

Zwei Dinge tut das Feld trotzdem, damit die Maske nicht bloß gemalt ist: Der **Rückgängig-Verlauf
ist aus**, solange ein Feld geheim ist (sonst holt Strg+Z den Klartext zurück, lange nachdem
zugeklappt wurde), und die **Barrierefreiheits-Schnittstelle bekommt den Wert nicht** — verdeckt
meldet sich das Feld als Passwortfeld und gibt sein Wertmuster gar nicht erst heraus.

`MaskLength="0"` zeigt so viele Zeichen, wie der Wert lang ist. Das sieht gewohnt aus und **gibt die
Länge preis** — das eine, was an einem Geheimnis sonst nicht zu raten ist. Eine legitime Wahl, aber
eine bewusste.

### Eigenschaften

| Eigenschaft | Standard | Bedeutung |
|-------------|----------|-----------|
| `PlaceholderText` | — | Text, solange das Feld leer ist |
| `IsSecret` | `false` | Inhalt als Punkte zeigen, bis aufgedeckt wird |
| `IsRevealed` | `false` | Ob ein Geheimnis gerade im Klartext steht |
| `MaskChar` | `●` | Zeichen, das den Inhalt vertritt |
| `MaskLength` | `16` | Anzahl der Zeichen; `0` spiegelt die echte Länge |
| `EmptySecretText` | `(not set)` | Steht statt der Punkte, wenn ein geheimes Feld leer ist |
| `Actions` | leer | Die Knöpfe, in der angegebenen Reihenfolge |

### Knöpfe

`ClearAction` leert das Feld und gibt den Fokus zurück — wer löscht, will tippen. Sie bleibt weg,
wo das Feld schreibgeschützt ist. Wann sie erscheint, sagt `ShowWhen`:

| `ShowWhen` | Wirkung |
|---|---|
| `WhenNotEmpty` | Sobald etwas im Feld steht (Vorgabe) |
| `WhenFocusedOrHovered` | Nur beim Hinfassen — ruhiger in einem Formular mit vielen Zeilen |
| `Always` | Immer, damit beim ersten Zeichen nichts springt |

`RevealAction` schaltet zwischen Punkten und Klartext. Sie erscheint nur an einem Feld mit
`IsSecret` — eine Aktion sagt selbst, was sie braucht, und blendet sich aus, wo sie es nicht bekommt.

### Symbole in vier Stufen

*„Simple things should be simple, complex things should be possible"* (Alan Kay):

| Stufe | Was gesetzt wird | Was man bekommt |
|---|---|---|
| 1 | nichts | Das eingebaute Symbol |
| 2 | `IconGeometry` | Ein eigener Pfad; Größe, Farbe und Verhalten bleiben |
| 3 | `Content` | Ein beliebiger Inhalt — Bild, Wort, Glyph einer Symbolschrift |
| 4 | `Style` / `ContentTemplate` | Vollständige Kontrolle |

**Eigene Symbole für die ganze Anwendung** setzt man über einen Style — Stufe 4, einmal statt an
jedem Feld:

```xml
<Style TargetType="asc:ClearAction" BasedOn="{StaticResource {x:Type asc:FieldAction}}">
    <Setter Property="IconGeometry" Value="{StaticResource MeinKreuz}" />
</Style>
```

Die eingebauten Symbole sind **gefüllte Umrisse**. Eine Geometrie aus bloßen Linien
(`M 0,5 L 10,5`) hat keine Fläche und bliebe unsichtbar — dafür gibt es `IconStrokeThickness`,
Vorgabe `0`.

## Probierstube

`AscWpfControls.Sandbox` ist eine kleine WPF-Anwendung zum Ausprobieren, ein Reiter je Control: beim Regler verschiedene Größen, alle Markierungen sowie einer mit und einer ohne Beschleunigung; beim Textfeld alle Sichtbarkeitsregeln, die vier Symbolstufen und ein Geheimnis zum Auf- und Zuklappen. Ein Bedienelement muss man anfassen können, um es zu beurteilen.

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

Die Pfaddaten der eingebauten Symbole stammen aus Microsofts **Fluent UI System Icons** (ebenfalls MIT) — siehe [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). Übernommen ist nur die Geometrie: Die Schriftart wird **nicht** mitgeliefert, damit aus der einzelnen Assembly kein Paket mit Anhang wird.
