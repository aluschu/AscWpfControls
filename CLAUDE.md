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
| XML-Doku und Code-Kommentare | **Englisch** | wer die Bibliothek einbindet oder hineinsieht |

Eine Spec ist ein Arbeitspapier, kein Teil der Auslieferung. Auf Deutsch treffen wir die
Unterscheidungen genauer, auf die es dort ankommt — und niemand sonst muss sie lesen.

**Auch die Doku ist englisch, und zwar ganz.** Sie erscheint in der IntelliSense jedes Aufrufers,
also bei genau dem Publikum, für das die englischen Namen gewählt wurden. Das gilt ebenso für die
Kommentare im Rumpf und für private Namen: Das Repo ist öffentlich, und eine Datei mit englischer
Doku über deutschen Variablennamen ist halb übersetzt, nicht zweisprachig.

Deutsch bleibt allein die Spec. Sie ist das einzige, was wirklich nur wir zwei lesen.

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

## Zurückhaltung gilt hier nicht

In einer Anwendung ist es richtig, eine Ebene erst einzuziehen, wenn der zweite Fall da ist — sonst
baut man auf Verdacht. **In einer Bibliothek gilt das Gegenteil**, und der Grund ist einfach: Der
zweite Fall ist hier das Projekt eines Fremden, und das bekommt man nie zu sehen.

Wo über eine Einstellung ernsthaft gestritten werden kann — steht ein Löschkreuz immer da oder nur
bei Fokus? — bekommt sie eine Option statt einer Entscheidung. Eine Vorgabe wählen wir trotzdem,
und zwar die, die wir selbst wollen; wer sie nicht mag, stellt sie um, statt das Control zu meiden.

Das kostet einmal Arbeit. Die Alternative kostet dauerhaft: **Ein Control, das sich nicht biegen
lässt, zieht Behelfslösungen nach sich** — und die stehen dann verstreut in den Anwendungen, wo
niemand sie mehr findet.

Der Maßstab dafür stammt von Alan Kay: „Simple things should be simple, complex things should be
possible." Beide Hälften zählen. Optionen anzubieten heißt **nicht**, den einfachen Fall mit Fragen
zu belasten — wer nichts einstellt, bekommt eine Vorgabe, die für sich schon eine Meinung ist. Die
Flexibilität liegt darunter und meldet sich erst, wenn jemand sie sucht.

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
