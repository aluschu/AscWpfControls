## Feature: Textfeld mit Vorgabe-Text und Knopfleiste

### Status
draft

### Ziel
Ein Eingabefeld, an dessen rechtem Innenrand Knöpfe sitzen, die auf seinen Inhalt wirken. Zwei
davon werden gebraucht:

- **Löschen** — ein Kreuz, das erscheint, sobald etwas im Feld steht, und den Inhalt wegräumt.
- **Aufdecken** — für Werte, die verdeckt angezeigt werden: Punkte statt Text, ein Knopf, der
  umschaltet.

Dazu ein **Vorgabe-Text**, der erscheint, solange das Feld leer ist. Kein Knopf, aber dieselbe
Bauart: etwas, das in das Feld hineingezeichnet wird. Er ist der Grund, warum aus dem Vorhaben ein
richtiges Control wird und keine angehängte Eigenschaft — mit zwei Zutaten im Feld will man **eine**
Vorlage, die beide kennt, und nicht zwei Verzierungen, die sich um denselben Platz streiten.

Was er bringt: aussagekräftige Oberflächen auf wenig Raum. Wo der Vorgabe-Text sagt, was gemeint
ist, kann die Beschriftung daneben kurz ausfallen oder ganz entfallen.

Beides sind aus Sicht des Feldes dieselbe Sache: *ein Knopf, der zum Feld gehört und etwas mit
seinem Inhalt macht*. Sie einzeln zu bauen hieße, die Mechanik zweimal zu haben — Knopf innerhalb
des Rahmens platzieren, Platz dafür freihalten, Fokus beim Klick nicht verlieren, Tastaturbedienung,
Verhalten bei gesperrtem Feld.

WPF bringt nichts davon mit. Actipro auch nicht: Ein Löschkreuz gibt es dort nur an der
`AutoCompleteBox` (`HasClearButton`), also an einem Such-/Kombinationsfeld mit Aufklappliste,
Filterung und `ItemsSource` — zu viel Apparat für ein Textfeld, und sein Löschknopf verschwindet
bei `IsReadOnly`. Zum Verdecken gibt es dort nur einen Theme-Style für die native `PasswordBox`,
und die kann grundsätzlich keinen Klartext zeigen; für einen Aufdecken-Knopf müsste man zwischen
zwei Bedienelementen umschalten.

### Betroffene Klassen/Controls
Alles neu, in `AscWpfControls/Controls/`:

- Das Feld selbst (Name siehe Frage 1) — eine `TextBox`-Ableitung mit einer Ablage für Knöpfe
- Ein Knopf-Stück fürs Löschen
- Ein Knopf-Stück fürs Aufdecken
- `AscWpfControls/Themes/Generic.xaml` — Standardaussehen
- `AscWpfControls.Sandbox` — eine Seite zum Anfassen

### Verhalten

#### Das Feld
- Es ist ein Textfeld und verhält sich wie eines: Bindung, Auswahl, Tastatur, Styles. Wer die Knöpfe
  weglässt, bekommt eine gewöhnliche `TextBox` und merkt vom Control nichts.
- Am rechten Innenrand liegt eine **Leiste**, in die beliebig viele Knöpfe eingehängt werden. Der
  Text endet vor der Leiste, statt darunter zu verschwinden.
- Die Leiste ist leer, solange niemand etwas hineinlegt — und kostet dann auch keinen Platz.
- Ist das Feld gesperrt (`IsEnabled="False"`), sind es die Knöpfe auch.

#### Vorgabe-Text
- Erscheint, solange das Feld leer ist, und verschwindet mit dem ersten Zeichen.
- Er ist **kein Wert**: Er wird nicht gebunden, nicht gespeichert und nicht ausgelesen. Wer ihn
  stehen lässt und speichert, speichert nichts.
- Er sieht anders aus als echter Text — gedämpft, damit niemand ihn für einen Inhalt hält.
- Er verträgt sich mit den Knöpfen: Ist das Feld leer, steht kein Löschkreuz da, also ist der Platz
  frei. Bei einem verdeckten Wert gewinnt der Vertretungstext für „leer" (siehe *Aufdecken*) — zwei
  Texte für dieselbe Aussage wären einer zu viel.

#### Löschen
- Erscheint nur, wenn Text im Feld steht. Ein Kreuz an einem leeren Feld verspricht eine Wirkung,
  die es nicht hat.
- Erscheint nicht, wenn das Feld schreibgeschützt ist — dort wäre es eine Lüge.
- Nach dem Klick liegt der Fokus **im Feld**, nicht auf dem Knopf: Wer löscht, will tippen.

#### Aufdecken
- Solange verdeckt, zeigt das Feld **Punkte statt des Werts** und ist schreibgeschützt. Sonst tippte
  jemand über die Punkte und speicherte am Ende die Punkte.
- Die Anzahl der Punkte sagt nichts über die Länge des Werts.
- Ist der Wert leer, steht dort ein Vertretungstext statt Punkten — sonst sieht ein leeres Geheimnis
  aus wie ein gefülltes.
- Der Knopf schaltet um und sagt, was er tut.

### Nicht im Scope
- **Ein Passwortfeld im Sinne der Sicherheit.** Der Wert ist eine gewöhnliche Zeichenkette im
  Speicher, wie bei jeder `TextBox`. Wer `SecureString` und Schutz vor Speicherabbildern braucht,
  nimmt die native `PasswordBox` — die kann dafür kein Aufdecken.
- **Eingabemasken und Wertprüfung.** Ein anderes Thema mit eigenem Zuschnitt.
- **Die Umstellung von TradeBook.** Dass die Einstellungsseite dieses Feld benutzt, ist ein eigener
  Umbau im TradeBook-Repo; er räumt dort sechs Mitglieder aus `EinstellungZeileViewModel` weg, die
  heute nur den Anzeigezustand eines Geheimnisses verwalten.

### Offene Fragen

1. **Wie heißt das Feld, und wie hängt man die Knöpfe ein?**
   Öffentliche Namen sind englisch (siehe `CLAUDE.md`). Mit dem Vorgabe-Text ist `ActionTextBox` zu
   eng geworden — das Control kann jetzt mehr als Aktionen.
   - **(a)** `TextField` mit `Placeholder` und einer Sammlung `Actions`:
     ```xml
     <asc:TextField Text="{Binding Wert}" Placeholder="Symbol oder ISIN">
         <asc:ClearAction />
         <asc:RevealAction />
     </asc:TextField>
     ```
   - **(b)** `EnhancedTextBox` / `SmartTextBox` — sagt nur, dass es mehr kann, nicht was.
   - **(c)** Kein eigenes Control, sondern **angehängte Eigenschaften** an der gewöhnlichen
     `TextBox` (`asc:TextBoxActions.HasClear="True"`). Dann bleibt jede vorhandene `TextBox`
     im Bestand nutzbar, ohne sie auszutauschen.
   *Empfehlung:* **(a)**. „Field" ist in Oberflächen-Bausätzen der übliche Name für genau das —
   Eingabe plus Vorgabe-Text plus Symbole am Rand — und kollidiert nicht mit WPFs `TextBox`. (b)
   sagt nichts. (c) war vor dem Vorgabe-Text noch verlockend, ist es jetzt nicht mehr: Zwei
   Verzierungen, die sich um denselben Platz im Feld streiten, brauchen eine gemeinsame Vorlage,
   und die hat nur ein echtes Control.

   Antwort:

2. **Sitzen die Knöpfe im Feld oder daneben?**
   - **(a)** Innerhalb des Rahmens, rechts — wie in Browsern und in der `AutoCompleteBox`.
   - **(b)** Außerhalb, rechts neben dem Feld.
   *Empfehlung:* **(a)**. Sie gehören zum Feld, also stehen sie in ihm; daneben sähen sie aus wie
   eigenständige Befehle und würden außerdem die Spaltenbreite in Formularen zerreißen.

   Antwort:

3. **Steht das Löschkreuz immer da, oder nur bei Fokus?**
   Der Anlass ist unsere eigene Einstellungsseite in TradeBook: zwölf Felder untereinander, alle
   gefüllt — also zwölf Kreuze in einer Spalte.
   - **(a)** Immer, sobald Text im Feld steht. Ehrlich und vorhersehbar; ein Bedienelement, das
     erst beim Hinfassen erscheint, findet man nicht, wenn man es sucht.
   - **(b)** Nur, wenn das Feld den Fokus hat oder der Zeiger darüber steht — so machen es die
     Browser. Ruhiger im Formular, aber versteckt.
   - **(c)** Vorgabe (a), umschaltbar auf (b) durch den Aufrufer.
   *Empfehlung:* **(a)** als Verhalten des Controls, und die Frage nach zwölf Kreuzen an der
   Einstellungsseite entscheiden, wenn wir sie sehen. Ein Textfeld gibt es, um eine Information zu
   bekommen — gefüllt ist der Normalfall, nicht die Ausnahme, und dann ist ein sichtbares Kreuz
   auch der Normalfall. (c) ist der Ausweg, falls das Bild später dagegenspricht; ein Schalter auf
   Verdacht wäre aber genau die Vorwegnahme, die wir uns sonst verbieten.

   Antwort:

4. **Wie sieht der Aufdecken-Knopf aus — Auge oder Wort?**
   TradeBook hat heute einen Umschaltknopf mit den Wörtern „Anzeigen"/„Verbergen".
   - **(a)** Ein Augensymbol, wie überall sonst üblich. Kurz, sprachfrei, in einer Bibliothek
     mit englischen Namen das Naheliegende.
   - **(b)** Wörter, vom Aufrufer gesetzt.
   - **(c)** Symbol als Vorgabe, Inhalt überschreibbar.
   *Empfehlung:* **(c)**. Ein Symbol als Vorgabe hält die Bibliothek sprachfrei; wer Wörter will,
   setzt sie. Dazu gehört dann allerdings die Frage, woher das Symbol kommt — als Pfad im Control
   gezeichnet oder vom Aufrufer geliefert.

   Antwort:
