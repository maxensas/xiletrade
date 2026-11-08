# Parsing rules

Sometimes the text of a mod copied from the game is **not exactly the text used by the trade site**. `ParsingRules.json` rewrites that text so that Xiletrade can find the mod in the trade filters. Common differences are:

- singular or plural,
- a different word order,
- a different wording for the same effect.

When a mod of your item is **not detected**, a parsing rule is usually the fix, and no code change is needed. **Please propose it**, see [Propose a rule](#propose-a-rule).

## Where the files are

There is one file per language, used for Path of Exile 1 and Path of Exile 2:

| Location | Role |
|---|---|
| `Data/Lang/<language>/ParsingRules.json` next to `Xiletrade.exe` (`en-US`, `fr-FR`, `de-DE`, `ko-KR`...) | File read for the language selected in Xiletrade. |
| `Xiletrade/Data/Lang/<language>/ParsingRules.json` of the GitHub repository (master branch) | Source of the automatic data update. |

> **Warning:** the data update downloads the files from GitHub again, so a local change is overwritten. To keep a rule, propose it on the GitHub repository.

## How it works

1. Xiletrade reads a mod line of the item and, in most cases, replaces its numeric values with `#`. For example, `Has 1 Abyssal Socket` becomes `Has # Abyssal Socket`.
2. The rules are tried **in the order of the file**. The first rule that fits is the only one used.
3. The rewritten text is searched in the trade filters, then the values are put back in place of the `#`.

The rules are tried **before** Xiletrade's own search, even for mods that would be found without them. Keep `old` as precise as possible, so that a rule never changes a mod that was already detected.

Comparisons are exact and case sensitive, accents included.

## Parsing layers

The parsing rules are only one of the **parsing layers** of Xiletrade. Each mod line goes through several layers, in this order, until one of them finds the mod in the trade filters:

1. **Special mods:** veiled mods and delirium rewards are recognized directly.
2. **Values:**
   - the mod is searched with its values;
   - when it has several values, it is also searched with only some of them replaced by `#`;
   - otherwise all the values are replaced by `#`.
3. **Reduced mods:** a `reduced` mod that is not found is searched as the `increased` mod with a negative value.
4. **Mods on two lines:** the line is joined with the next one when the joined text is a mod of the trade site.
5. **Parsing rules:** this file.
6. **Exact text:** the text is found as it is in the trade filters.
7. **Closest text:** when the text is still not found, Xiletrade takes the closest text of the trade filters, as long as only a few characters differ.
8. **Item kind:** some mods are resolved with the kind of item, for example the rooms of a Chronicle of Atzoatl or the local stats of weapons and armours.

These layers **fix many differences automatically, without anyone noticing**. A mod whose text differs a little from the trade site is often found by the closest-text layer, so it does not need a rule.

A rule is needed when:

- the mod is **not detected at all**,
- or the mod is detected as **another mod**. The closest text can be a different mod with a similar wording.

Before proposing a rule, check in the price check window that the mod found is really the mod of your item.

## A rule

```json
{ "id": 7, "stat": "null", "replace": "contains", "old": "Areas are Alluring", "new": "Area is Alluring" }
```

| Field | Value | Effect |
|---|---|---|
| `id` | number | Number of the rule. Informative only. |
| `stat` | text | Not used by the parsing. Keep `"null"`. |
| `replace` | `"equals"` or `"contains"` | How `old` is compared with the mod, see below. |
| `old` | text | Text of the mod as Xiletrade sees it, with `#` in place of the values. |
| `new` | text | Text used instead. |
| `disabled` | `true` | The rule is ignored. Optional. |

The two values of `replace`:

| `replace` | The rule fits when... | Result |
|---|---|---|
| `equals` | the whole mod is `old` | the whole mod becomes `new` |
| `contains` | the mod contains `old` | every occurrence of `old` is replaced with `new`, and the rest of the mod is kept |

### Mods on two lines

When the trade site writes a mod on two lines, put `\n` between the two lines in `new`. The `#` values are then taken from the **next line** of the item.

## When a file is invalid

The file of the selected language is checked when Xiletrade starts. A mistake stops the loading and gives the file and the rule, for example:

```
Lang/fr-FR/ParsingRules.json, mods[3] (id 3) : 'old' is empty.
```

Mistakes that stop the loading:

- a missing `mods` list,
- a `replace` other than `equals` or `contains`,
- an empty `old` or `new`.

## Examples

### `equals` with a value (English)

```json
{ "id": 1, "stat": "null", "replace": "equals", "old": "Has # Abyssal Socket", "new": "Has # Abyssal Sockets" }
```

`Has 1 Abyssal Socket` is searched as `Has # Abyssal Sockets`, the text of the trade site.

### `contains`, the same rule in two languages

English:

```json
{ "id": 7, "stat": "null", "replace": "contains", "old": "Areas are Alluring", "new": "Area is Alluring" }
```

French:

```json
{ "id": 7, "stat": "null", "replace": "contains", "old": "Les Zones sont attrayantes", "new": "La Zone est attrayante" }
```

Each language has its own wording and its own file. A rule written for one language does nothing in the others.

### `contains` on a part of the mod (Korean)

```json
{ "id": 1, "stat": "null", "replace": "contains", "old": "의 손에 넘어감", "new": "의 영향을 받음" }
```

Only the end of the mod is replaced. The beginning (the name of the influence) is kept, so one rule covers several mods: `지역이 엘더의 손에 넘어감` becomes `지역이 엘더의 영향을 받음`.

### Mod on two lines (Japanese)

```json
{ "id": 2, "stat": "null", "replace": "equals", "old": "マップにリチュアルの祭壇を追加する", "new": "マップにリチュアルの祭壇を追加する\n残り使用回数 #回" }
```

The trade site joins this mod and the number of remaining uses into one mod on two lines. The `#` takes its value from the next line of the item.

## Edit or test a rule

The rules of the selected language can be edited in Xiletrade, in the **Editor** opened from the settings window. Saving writes `Data/Lang/<language>/ParsingRules.json` and the rules are used at once.

The editor only keeps a rule that has:

- a `replace` value of `equals` or `contains`,
- a non-empty `old`,
- a non-empty `new`.

## Propose a rule

A mod not detected by Xiletrade is usually fixed by a rule, for **any language**. Your help is welcome, especially for the languages other than English, which have far fewer rules.

- **Open an issue:** https://github.com/maxensas/xiletrade/issues. Give:
  - the game (Path of Exile 1 or 2) and the language,
  - the item copied from the game (`Ctrl+C` on the item, then paste the whole text),
  - the mod that is not detected,
  - if you know it, the text of the same mod on the trade site.
- **Or open a pull request** that changes `Xiletrade/Data/Lang/<language>/ParsingRules.json`:
  - add the rule at the end of the file with the next free `id`,
  - make sure no rule before it already fits the same mod, since only the first rule that fits is used.

Rules that are wrong or no longer needed (the game or the trade site changed its text) can be reported the same way, or set to `"disabled": true`.
