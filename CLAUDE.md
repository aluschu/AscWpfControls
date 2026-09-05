# AscWpfControls

Eine WPF-Control-Bibliothek (.NET, C#) — eigenständige Bedienelemente, die in mehreren Projekten
benutzt werden. Erster Bewohner ist der `RotaryEncoder`, ein Endlos-Drehregler, der Ticks meldet.

## Zwei Sprachen, nach Publikum getrennt

Das ist die wichtigste Abweichung von den übrigen Projekten, und der Grund dafür ist der Zweck des
Repos: **Es soll eine Bibliothek sein, die andere benutzen — nicht eine, die andere mitentwickeln.**

| | Sprache | wer liest es |
|---|---|---|
| Öffentliche Namen (Typen, Eigenschaften, Ereignisse) | **Englisch** | wer die Bibliothek einbindet |
| README, Commit-Nachrichten, Release-Notizen | **Englisch** | dieselben |
| **Specs in `Data/Specs/`** | **Deutsch** | wir zwei, beim Bauen |
| XML-Doku an öffentlichen Membern | **Deutsch** | der Betreuer — siehe unten |

Eine Spec ist ein Arbeitspapier, kein Teil der Auslieferung. Auf Deutsch treffen wir die
Unterscheidungen genauer, auf die es dort ankommt — und niemand sonst muss sie lesen.

**Warum die XML-Doku trotzdem deutsch ist.** Sie erscheint zwar in der IntelliSense jedes
Aufrufers, also bei dem Publikum, für das die englischen Namen gewählt wurden. Nur: Leser der
Bibliothek gibt es heute keine, einen Betreuer schon — und der muss in einem halben Jahr lesen
können, *warum* etwas so gebaut ist. Ein Kommentar, den der Betreuer nicht mehr entziffert, hat
seinen Zweck verfehlt, ganz gleich wer ihn sonst noch sähe.

Der `RotaryEncoder` hält es bereits so. Wenn die Bibliothek einmal wirklich benutzt wird, wird die
Doku in einem Rutsch übersetzt — dieselbe Zurückhaltung wie überall: die Ebene einziehen, wenn der
zweite Fall da ist, nicht auf Verdacht.

Chat-Antworten sind Deutsch wie überall (siehe globale `CLAUDE.md`).

## Specs

Feature-Specs liegen in `Data/Specs/`, im Format aus der globalen `CLAUDE.md` — samt der
Unterscheidung zwischen `## Feature:` und `## Umbau:`. Erledigte wandern nach `Data/Specs/Done/`.

**Verweise:** Ein Markdown-Link verspricht ein Dokument, das es gibt. Eine Spec, die nur geplant
ist, wird als Name in Code-Auszeichnung geschrieben (`` `feld-mit-knopfleiste.md` ``).
`.\check-spec-links.ps1` prüft das; nach jedem Verschieben nach `Done/` laufen lassen, denn dabei
verschieben sich die relativen Pfade in beide Richtungen.

## Was hier hineingehört — und was nicht

Ein Control gehört hierher, wenn **nichts an ihm** die aufrufende Anwendung kennt. Kein Wissen über
Trades, Depots, Einstellungen; keine Fachbegriffe aus einem anderen Projekt. Die Probe: Würde das
Control in einer beliebigen anderen WPF-Anwendung Sinn ergeben?

Wer unsicher ist, baut es zuerst dort, wo es gebraucht wird, und zieht später um. Ein Control, das
verfrüht hierher wandert, nimmt die Annahmen seines ersten Aufrufers mit — und die stehen dann in
einer Bibliothek, die sie nicht erklären kann.

## Ausprobieren

`AscWpfControls.Sandbox` ist eine eigene Anwendung zum Anfassen der Controls. Sie ist der schnellere
Weg als der Umweg über ein Projekt, das die Bibliothek einbindet — und der ehrlichere, weil dort
keine fremden Styles mitwirken.

## Zusammenarbeit

**Committen darf Claude**, wann immer es angebracht scheint. **Gepusht wird nie ungefragt** — das
Repo ist öffentlich, ein Push ist eine Veröffentlichung.

Nutzt ein anderes Projekt die Bibliothek gerade über eine **Projektreferenz** statt über das
NuGet-Paket (TradeBook tut das), dann wirkt jede Änderung hier sofort dort. Das ist beim Bauen
gewollt und beim Veröffentlichen eine Falle: Vor einem `dotnet publish` des anderen Projekts gehört
die Referenz zurück auf das Paket, sonst fehlt die Bibliothek auf jeder Maschine, die dieses Repo
nicht ausgecheckt hat.
