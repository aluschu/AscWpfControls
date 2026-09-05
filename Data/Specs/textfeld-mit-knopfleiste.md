## Feature: Textfeld mit Vorgabe-Text und Knopfleiste

### Status
approved

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
- `AscWpfControls.Sandbox` — eine Seite zum Anfassen, mit allen Stufen nebeneinander: ohne
  Knöpfe, mit Löschen, mit Aufdecken, mit beidem, mit eigenem Symbol und mit jeder
  Sichtbarkeitsregel

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

#### Das Aussehen eines Knopfes — vier Stufen
Der Maßstab ist Alan Kays Satz: „Simple things should be simple, complex things should be
possible." Jeder Knopf bringt sein Symbol mit und lässt es sich abnehmen — in vier Stufen, von
„nichts tun" bis „alles selbst":

| Stufe | Was der Aufrufer setzt | Was er dafür bekommt |
|---|---|---|
| 1 | nichts | Das eingebaute Symbol — Kreuz beim Löschen, Auge beim Aufdecken |
| 2 | `IconGeometry` | Ein eigener Pfad, alles andere bleibt: Größe, Farbe, Verhalten beim Überfahren |
| 3 | `Content` | Ein beliebiger Inhalt — ein Bild, ein Wort, was auch immer |
| 4 | `Style` / `ContentTemplate` | Vollständige Kontrolle, wie bei jedem WPF-Control |

Der Punkt der Staffelung: **Wer nur ein anderes Kreuz will, soll nicht ein Template schreiben
müssen.** Stufe 2 ist der Fall, der in der Praxis am häufigsten gebraucht wird — ein Haus hat seinen
eigenen Symbolsatz, aber nicht seine eigene Vorstellung davon, wie groß ein Knopf im Feld ist.

Die eingebauten Symbole sind als `Geometry` im Control gezeichnet, nicht als Bilddateien: Sie
skalieren mit, färben sich mit, und die Bibliothek bleibt eine einzelne DLL ohne Anhang.

#### Löschen
- Erscheint nur, wenn Text im Feld steht. Ein Kreuz an einem leeren Feld verspricht eine Wirkung,
  die es nicht hat.
- Erscheint nicht, wenn das Feld schreibgeschützt ist — dort wäre es eine Lüge.
- Nach dem Klick liegt der Fokus **im Feld**, nicht auf dem Knopf: Wer löscht, will tippen.
- **Wann es erscheint, ist einstellbar.** Vorgabe ist „sobald Text im Feld steht"; wählbar sind
  außerdem „nur bei Fokus oder Überfahren" und „immer". Ein Formular mit zwölf gefüllten Feldern
  will etwas anderes als ein einzelnes Suchfeld, und beide haben recht.

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

   Antwort: **(a)** — `TextField`, mit einer `Actions`-Sammlung. Und es **erbt von `TextBox`**,
   statt eine zu umschließen: Bindung, Auswahl, Rückgängig, Zwischenablage, Rechtschreibprüfung und
   die Eingabemethoden sind damit geschenkt, und ein Aufrufer kann `<TextBox>` durch
   `<asc:TextField>` ersetzen, ohne dass etwas ausfällt. Beim Umschließen müsste man Dutzende
   Mitglieder durchreichen, und was man vergisst, fehlt still.

   **Warum (c) ausscheidet, im Detail:** Eine angehängte Eigenschaft kann von außen nichts *in* ein
   fremdes Feld legen, sondern nur eine Schicht darüber (`Adorner`). Die schwebt und reserviert
   keinen Platz — der Text liefe unter dem Kreuz hindurch. Um Platz zu schaffen, müsste sie das
   `Padding` der fremden `TextBox` verstellen, also einen Wert überschreiben, den die Anwendung
   womöglich selbst gesetzt hat. Mit dem Vorgabe-Text käme eine zweite Schicht dazu, die von der
   ersten nichts weiß und am selben `Padding` zieht. Ein Control mit eigener Vorlage plant den Platz
   ein, statt ihn zu erschleichen.

2. **Sitzen die Knöpfe im Feld oder daneben?**
   - **(a)** Innerhalb des Rahmens, rechts — wie in Browsern und in der `AutoCompleteBox`.
   - **(b)** Außerhalb, rechts neben dem Feld.
   *Empfehlung:* **(a)**. Sie gehören zum Feld, also stehen sie in ihm; daneben sähen sie aus wie
   eigenständige Befehle und würden außerdem die Spaltenbreite in Formularen zerreißen.

   Antwort: ja, auf jeden Fall a, das macht es ja gerade so einfach und kompakt

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

   Antwort: c) ja, wir verbieten uns zwar Dinge in der Konkreten Umsetzung in TradeBook und das ist gut so, aber hier haben wir ein Control, das praktisch jeder irgendwie benutzen können soll, und das kann gar nicht flexibel genug sein. Es ist super frustrierend, wenn eine Einstellung über die man offensichtlich streiten/diskutieren kann keine Optionen bietet. Auch wenn das in der Implementierung mehr Arbeit bedeutet. Die Arbeit macht man nur einmal und ich will kein Control, was dann ständig Workarounds nach sich zieht, so wie deine Passwortlösung aktuell.

4. **Wie sieht der Aufdecken-Knopf aus — Auge oder Wort?**
   TradeBook hat heute einen Umschaltknopf mit den Wörtern „Anzeigen"/„Verbergen".
   - **(a)** Ein Augensymbol, wie überall sonst üblich. Kurz, sprachfrei, in einer Bibliothek
     mit englischen Namen das Naheliegende.
   - **(b)** Wörter, vom Aufrufer gesetzt.
   - **(c)** Symbol als Vorgabe, Inhalt überschreibbar.
   *Empfehlung:* **(c)**. Ein Symbol als Vorgabe hält die Bibliothek sprachfrei; wer Wörter will,
   setzt sie. Dazu gehört dann allerdings die Frage, woher das Symbol kommt — als Pfad im Control
   gezeichnet oder vom Aufrufer geliefert.

   Antwort: alles mit "Icons" die Frage ist hier, wie kriegen wir das hin, dass es eine Vorgabe gibt, aber der Entwickler diese gegen seine eigenen Vorgaben austauschen kann? Erst dann wird es wirklich brauchbar. c trifft es nicht so ganz, ich glaube es muss noch flexibler sein. Default sind auf jeden fall die intern die "Icons", die man so kennt (Kreuz, Auge).
5. **Wird es eine Sandbox geben?**
  Antwort: ja, bitte bitte
