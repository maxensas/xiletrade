# Stat rules

`StatRules.json` (Path of Exile 1) and `StatRulesTwo.json` (Path of Exile 2) choose the **stat id sent to the trade site** for each mod of a price-checked item.

No code change is needed to add, change or remove a rule: edit the file and restart Xiletrade.

## Why these rules exist

When Xiletrade parses a mod, it looks for its text in the trade filters. The filters often list **several entries with the same text**, but the trade site accepts only one of them for a given item. For example:

- a local or global stat (accuracy on a weapon or on a ring),
- the tincture or charm version of a mod,
- a mod that only exists on one specific unique,
- an entry that has nothing to do with the item (a sanctum entry on another item, a temple room on a chronicle...).

Some ids are also **deprecated by the trade site without notice**, so an equivalent id must be sent instead.

## How it works

1. The entries are still found by their text in the filters, as they are. Nothing is indexed or frozen, so the filters can change freely.
2. For each entry found, the rules receive its id, its type and the item.
3. The rules return the id to use, or tell that the entry must be **skipped** for this item. When an entry is skipped, the next entry with the same text is tried.

Rules never modify the filters. An id that no rule mentions is kept as it is.

## Where the files are

| Location | Role |
|---|---|
| `Data/` folder next to `Xiletrade.exe` | Files read when Xiletrade starts. |
| `Xiletrade/Data/` of the GitHub repository (master branch) | Source of the automatic data update. |

> **Warning:** the data update downloads the file of the current game again from GitHub, so a local change is overwritten. To keep a change, propose it on the GitHub repository.

## When a file is invalid

The files are checked when Xiletrade starts. A mistake stops the loading and gives the file and the location of the rule, for example:

```
StatRules.json, root.rules[0].rules[2] : unknown condition 'weapons' ...
```

Mistakes that stop the loading:

- an unknown `mode`,
- an unknown flag or condition in `when`,
- a condition without a value (`"unique:"`).

## A rule

The root of the file is itself a rule. Every field is optional.

| Field | Value | Effect |
|---|---|---|
| `comment` | text | Free text, ignored. |
| `match` | object | Conditions on the **current id**, see [match](#match). |
| `when` | list of conditions | Conditions on the **item**, see [when](#when). |
| `id` | id | Replaces the current id. |
| `suffixId` | end of id | Replaces the current id but keeps its first part, up to and including the `.`: with `"suffixId": "stat_2"`, `implicit.stat_1` becomes `implicit.stat_2`. |
| `skip` | `true` | The entry is not used for this item. This is final: no other rule can undo it. |
| `rules` | list of rules | Child rules, tried only when this rule applies. |
| `mode` | `"all"` or `"first"` | How the child rules are tried, see [mode](#mode). |

A rule **applies** when all the conditions of its `match` and of its `when` are true. When it applies, its actions run in this order:

1. `id`,
2. `suffixId`,
3. `skip`,
4. its child `rules`.

A rule without `match` and without `when` always applies.

### match

All the conditions given must be true. Each list is true when **one** of its values fits.

| Condition | True when the id... | Example |
|---|---|---|
| `ids` | is one of these ids | `"ids": [ "explicit.stat_803737631" ]` |
| `suffix` | ends with one of these values | `"suffix": [ "stat_1379411836" ]` |
| `contains` | contains one of these values | `"contains": [ "indexable_support" ]` |
| `notContains` | contains none of these values | `"notContains": [ "pseudo_temple_apex" ]` |
| `stat` | has one of these stat parts (the part between the first and the second `.`) | `"stat": [ "stat_1920370417" ]` matches `explicit.stat_1920370417` |
| `minLength` | is at least this long | `"minLength": 2` |

Comparisons are exact and case sensitive.

### when

A list of conditions on the item. **All** of them must be true, so they work as AND. For an OR, write one rule per condition.

| Condition | True when |
|---|---|
| a flag (see below) | the item has this flag |
| `unique:<english name>` | the item is this unique, e.g. `"unique:Bones of Ullr"` |
| `typeEn:<english type>` | the english base type is this one, e.g. `"typeEn:Overseer Tablet"` |
| `entryType:<type>` | the filter entry has this type, e.g. `"entryType:sanctum"` |

A `!` before any condition negates it, e.g. `"!unique:Forbidden Shako"`.

Unique names (`unique:`) are always written **in english**, whatever the language of the game: Xiletrade translates them.

Flags:

| Flag | Item |
|---|---|
| `weapon` | weapon |
| `armour` | armour |
| `unique` | unique rarity |
| `jewel` | jewel |
| `cluster` | cluster jewel |
| `tincture` | tincture |
| `charm` | charm |
| `flask` | flask |
| `amulet` | amulet |
| `shield` | shield |
| `waystone` | waystone |
| `tablet` | tablet |
| `chronicle` | Chronicle of Atzoatl |
| `logbook` | expedition logbook |
| `sanctumRelic` | sanctum relic |

### mode

| Mode | Behaviour |
|---|---|
| `all` (default) | Every child rule is tried, in order. Each one works on the id left by the previous ones. |
| `first` | Only the **first** child rule that applies is used, like `if / else if / else`. |

A `skip` stops everything, whatever the mode.

In a `first` list, a last child without `match` and without `when` is the `else`.

## Examples

**Deprecated id:**

```json
{ "match": { "ids": [ "explicit.stat_1" ] }, "id": "explicit.stat_2" }
```

**Local or global stat.** Accuracy is local on a weapon and global elsewhere:

```json
{
	"match": { "ids": [ "explicit.stat_803737631", "explicit.stat_691932474" ] },
	"mode": "first",
	"rules": [
		{ "when": [ "weapon" ], "id": "explicit.stat_691932474" },
		{ "id": "explicit.stat_803737631" }
	]
}
```

**Mod of one unique only:**

```json
{ "match": { "ids": [ "explicit.indexable_support_33" ] }, "when": [ "unique:Lioneye's Vision" ], "id": "explicit.stat_254728692" }
```

**Entries that do not belong to the item.** On a sanctum relic, only the sanctum entries are used:

```json
{ "when": [ "sanctumRelic" ], "rules": [ { "when": [ "!entryType:sanctum" ], "skip": true } ] }
```

**Same end of id, any first part** (`explicit.`, `implicit.`...):

```json
{ "match": { "suffix": [ "stat_1379411836" ] }, "suffixId": "stat_2897413282" }
```

## Order of the rules

The order matters:

- **Path of Exile 1:** after the guard on the id length, three things are tried in order:
  1. the list of mods that are never searchable,
  2. the socketed gems rules,
  3. a `first` list.

  In that `first` list, the rules on specific ids come before the rules on item kinds. Only the first rule that applies is used.

  The logbook rule comes last, outside the guard.
- **Path of Exile 2:** every rule is applied in order, each one on the id left by the previous ones.

When adding a rule, place it next to the rules of the same kind and use `comment` to say why it exists.
