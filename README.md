# AscWpfControls

WPF controls for .NET 10. No dependencies beyond WPF itself, so the library also fits into projects
that use no vendor's control suite.

## RotaryEncoder

An endless dial, modelled on the hardware encoders found on a mixing desk.

**The control knows no value.** No upper or lower bound, no number format, no step size — all it
reports is that it was turned, with a signed number of detents. What a detent means is up to the
receiver: a cent on a price, one unit on a quantity. The step size may well depend on the magnitude
of the value, and the control never has to know.

That is not only a clean separation. A real hardware encoder delivers detents in exactly the same
way and never a formatted value; anyone wiring one in later replaces the source, not the receiver.

```xml
<asc:RotaryEncoder Diameter="48" MarkerKind="Pointer" Ticked="Price_Ticked" />
```

```csharp
private void Price_Ticked(object sender, RotaryEncoderTickEventArgs e)
{
    // A cent is right on a 40-euro share and tedious on a 2000-euro one -
    // which is why the step size is decided here and not in the control.
    _price = Math.Max(0, _price + e.Delta * StepSize(_price));
}
```

Without code-behind it works just the same: `TickCommand` receives the number of detents as its
parameter.

### Interaction

| Input | Effect |
|-------|--------|
| Dragging with the left mouse button | The pointer disappears and stays put; the turn goes on as far as you like — no edge of the screen, no end stop |
| Mouse wheel | One detent per notch |
| Up and down arrows | One detent |
| Page Up and Page Down | `TicksPerPage` detents |

A slow drag hits a single detent, a fast one covers ground: the delta grows with the drag speed. One
event per impulse carries the count — the receiver computes `value += Delta * stepSize` and never has
to count events.

Modifier keys are deliberately **not** interpreted. An encoder on a mixing desk has no shift key;
anyone wanting a finer turn reads `Keyboard.Modifiers` where the step size is decided anyway.

### Properties

| Property | Default | Meaning |
|----------|---------|---------|
| `Diameter` | `26` | The size of the knob — one number instead of `Width` and `Height` |
| `MarkerKind` | `Notch` | The marker: `Notch`, `Pointer` or `Dot` |
| `MarkerBrush` | Blue | Colour of the marker, independent of the rim's `Foreground` |
| `FocusRingBrush` | Blue | Colour of the focus ring; `Transparent` hides it |
| `PixelsPerTick` | `6` | Drag distance per detent at a calm speed |
| `AccelerationFactor` | `3` | How strongly a fast drag multiplies the detents; `0` turns acceleration off |
| `MaxTicksPerEvent` | `32` | Upper bound per event, so a jerk does not skip half the range |
| `TicksPerPage` | `10` | Detents for Page Up and Page Down |
| `DegreesPerTick` | `12` | Rotation of the marker per detent — purely cosmetic |

`Ticked` as an event and `TickCommand` as a command report the same thing. The read-only values
`Angle` and `IsTurning` are there for your own templates.

The knob stays round at any size: it is drawn on a fixed area that a `Viewbox` scales evenly. Uneven
`Width`/`Height` or a stretching container produce no egg, just a centred circle. The focus ring lies
inside the control's own area and therefore overlaps no neighbours.

## TextField

A text box with a **placeholder** and a **strip of buttons inside it**, at the right-hand end. It
inherits from `TextBox`, so binding, selection, undo, the clipboard and spell checking are all
unchanged — swap `<TextBox>` for `<asc:TextField>` and nothing is lost.

```xml
<asc:TextField Text="{Binding Symbol}" PlaceholderText="Symbol or ISIN">
    <asc:ClearAction />
</asc:TextField>
```

The **placeholder is not a value**. It is never bound to the source, never saved and never read back.
Where it says what is meant, the label beside the field can be short or absent altogether — which is
the point of it.

The buttons are a **collection**, not a set of switches: the third kind costs a small class rather
than another property on the field.

### Masking is not protection

> `IsSecret` hides a value from **onlookers**: the key on a shared screen, the screenshot about to be
> sent. **It is not a security component.** The value is an ordinary string in memory, as in any text
> box, and no setting changes that.
>
> If you need protection from someone with access to the machine, use the native `PasswordBox` — and
> give up revealing, because it can never show its content in clear.

Two things the field does anyway, so that the mask is not merely painted on: the **undo history is
off** while a field is secret (otherwise Ctrl+Z brings the cleartext back long after it was hidden
again), and the **accessibility interface does not get the value** — while masked the field declares
itself a password control and withholds its value pattern entirely.

`MaskLength="0"` shows as many characters as the value is long. That looks familiar and **gives the
length away** — the one thing about a secret that is otherwise not free to guess. A legitimate
choice, but a deliberate one.

### Properties

| Property | Default | Meaning |
|----------|---------|---------|
| `PlaceholderText` | — | Text shown while the field is empty |
| `IsSecret` | `false` | Show the content as dots until it is revealed |
| `IsRevealed` | `false` | Whether a secret is currently shown in clear |
| `MaskChar` | `●` | The character standing in for the content |
| `MaskLength` | `16` | How many characters are shown; `0` mirrors the real length |
| `EmptySecretText` | `(not set)` | Shown instead of dots while a secret field is empty |
| `Actions` | empty | The buttons, in the order given |

### Buttons

`ClearAction` empties the field and puts the focus back — whoever clears is about to type. It stays
away from a read-only field. When it appears is up to `ShowWhen`:

| `ShowWhen` | Effect |
|---|---|
| `WhenNotEmpty` | As soon as there is content (default) |
| `WhenFocusedOrHovered` | Only on approach — quieter in a form with many rows |
| `Always` | Always, so nothing shifts when the first character arrives |

`RevealAction` toggles between dots and cleartext. It appears only on a field with `IsSecret` — an
action states what it needs and hides where it cannot get it.

### Icons in four stages

*"Simple things should be simple, complex things should be possible"* (Alan Kay):

| Stage | What you set | What you get |
|---|---|---|
| 1 | nothing | The built-in icon |
| 2 | `IconGeometry` | Your own path; size, colour and behaviour stay |
| 3 | `Content` | Any visual at all — an image, a word, a glyph from an icon font |
| 4 | `Style` / `ContentTemplate` | Full control |

**Your own icons for a whole application** go into a style — stage 4, once instead of on every
field:

```xml
<Style TargetType="asc:ClearAction" BasedOn="{StaticResource {x:Type asc:FieldAction}}">
    <Setter Property="IconGeometry" Value="{StaticResource MyCross}" />
</Style>
```

The built-in icons are **filled outlines**. A geometry made of bare lines (`M 0,5 L 10,5`) has no
area and would stay invisible — that is what `IconStrokeThickness` is for, default `0`.

## Sandbox

`AscWpfControls.Sandbox` is a small WPF application for trying things out, one tab per control: for
the dial, various sizes, every marker and one encoder with and one without acceleration; for the text
field, every visibility rule, all four icon stages and a secret to open and close. A control has to
be handled to be judged.

## Building and publishing the package locally

```
dotnet pack AscWpfControls\AscWpfControls.csproj -c Release
dotnet nuget push AscWpfControls\bin\Release\Aluschu.WpfControls.1.0.0.nupkg -s C:\LocalNuget
```

`C:\LocalNuget` is the folder feed set up as the *Local Dev Feed*.

## How this came about

This library was written in dialogue with **Claude** (Anthropic, through Claude Code). Claude wrote
the specifications, the code and this documentation; the requirements, the design decisions and the
testing against the running program came from Axel Schubert.

The load-bearing idea did not come from the machine: that the control **knows no value** and only
reports detents was the author's stipulation — and only that turned a number box with a knob into a
control a real hardware encoder could drive one day.

## Licence

MIT — see [LICENSE.txt](LICENSE.txt).

The path data of the built-in icons comes from Microsoft's **Fluent UI System Icons** (MIT as well) —
see [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md). Only the geometry was taken: the font is **not**
shipped, so a single assembly stays a single assembly.
