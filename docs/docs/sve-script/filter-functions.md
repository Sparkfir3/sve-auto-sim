---
title: Filter Functions
parent: SVE Script
nav_order: 5
---
# Filter Functions
Filter functions are strings used to filter effect targets. A blank filter function matches all cards. Filter functions are case-sensitive

| Syntax                        | Filter                 | Notes                                                                                                                                                                                                                                 |
| ----------------------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `F`                           | <u>F</u>ollower        |                                                                                                                                                                                                                                       |
| `S`                           | <u>S</u>pell           |                                                                                                                                                                                                                                       |
| `A`                           | <u>A</u>mulet          |                                                                                                                                                                                                                                       |
| `E`                           | <u>E</u>volved         |                                                                                                                                                                                                                                       |
| `K`                           | To<u>k</u>en           |                                                                                                                                                                                                                                       |
| `R`                           | <u>R</u>eserved        | Untapped. All cards not on the field are considered reserved                                                                                                                                                                          |
| `N`                           | E<u>n</u>gaged         | Tapped. All cards not on the field are considered not engaged                                                                                                                                                                         |
| `a(<amount>)`                 | <u>A</u>ttack          | Supports min/max amount `a(m(<min>,<max>))`                                                                                                                                                                                           |
| `d(<amount>)`                 | <u>D</u>efense         | Supports min/max amount `d(m(<min>,<max>))`                                                                                                                                                                                           |
| `e(<amount>)`                 | <u>E</u>volve Cost     | All cards that can't evolve have an evolve cost of -1<br>Supports min/max amount `e(m(<min>,<max>))`                                                                                                                                  |
| `p(<amount>)`                 | <u>P</u>lay Point Cost | Supports min/max amount `p(m(<min>,<max>))`                                                                                                                                                                                           |
| `X`                           | E<u>x</u>clude Self    | Excludes the card that the effect is originating from                                                                                                                                                                                 |
| `t(<trait>)`                  | <u>T</u>rait           | Cards with given trait (case-sensitive)<br>Example: `t(Pixie)` checks for "Pixie" trait                                                                                                                                               |
| `k(<keyword>)`                | <u>K</u>eyword         | Cards with given keyword (case-sensitive)<br>Example: `k(Ward)` checks for cards with Ward                                                                                                                                            |
| `r(<counter>,`<br>`<amount>)` | Counte<u>r</u>         | Cards with at least given amount of counter (case-sensitive)<br>If no amount is specified, defaults to 1<br>Example: `r(Stack)` checks for cards with a Stack counter<br>Example: `r(Spell,3)` checks for cards with 3 Spell counters |
| `c(<class>)`                  | Class                  | "-craft" suffix is optional                                                                                                                                                                                                           |
| `n(<name>)`                   | Card <u>N</u>ame       | Cards with given name (case-sensitive)                                                                                                                                                                                                |
| `i(<int>[])`                  | Instance IDs           | ⚠️ FOR INTERNAL/BACKEND USE ONLY<br>Filters for cards with given instance IDs                                                                                                                                                         |

Special Parameters:

| Syntax           | Filter  | Notes                                                                                                           |
| ---------------- | ------- | --------------------------------------------------------------------------------------------------------------- |
| `m(<min>,<max>)` | Min/Max | Specifies a specific minimum and maximum amount of cards that can be targeted.<br>Default is a min and max of 1 |
| `!`              | Inverse | Applies an inverse/NOT check to the next filter entry<br>Example: `F!E` = Non-evolved Followers                 |

## Shortcut Filters
Shortcuts can be used as shorthand for commonly used filters. All shortcuts start with a `#` symbol.

Internally, these are replaced with it's corresponding value, as if the full filter was typed out, and *does not* have unique logic for handling. Because of this, shortcuts do not support inverse filters `!`.

| Shortcut | Value        | Notes                  |
| -------- | ------------ | ---------------------- |
| `#F`     | `F!k(Aura)`  | Followers without Aura |
| `#a`     | `!k(Aura)`   | Card without Aura      |
| `#e`     | `e(m(0,99))` | Card with Evolve       |

# Notes
All effects that select a target automatically ignore cards with Aura on the opponent's field, so Aura does not need to be specified for in lines such as `TargetOpponentCard F`. However, Aura does need to be checked for in other items such as `condition` lines or other scenarios where a filter is used outside of a target modifier.
