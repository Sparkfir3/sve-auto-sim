---
title: Card Declaration
parent: SVE Script
nav_order: 1
---
# Card Data Declaration

| Syntax                                                                | Description                                                                                                                             | Required                                 |
| --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| `name <name>;`                                                        | Card Name                                                                                                                               | ✅                                        |
| `id <ID>;`                                                            | Official Card ID                                                                                                                        | ✅                                        |
| `class <class>;`                                                      | Deck Construction Class                                                                                                                 | ✅                                        |
| `universe <universe>;`                                                | Deck Construction Universe                                                                                                              | ❌                                        |
| `type <type>;`                                                        | Card Type<br>`Follower`, `Evolved Follower`, `Spell`, `Amulet`, or `Leader`<br>Tokens should have `/ Token` included in their card type | ✅                                        |
| `trait <trait>;`                                                      | Card Trait(s), separated by backslashes (`<trait1> / <trait2>`)                                                                         | ✅ (except Leaders)                       |
| `cost <cost>;`                                                        | Play Point Cost                                                                                                                         | ✅ (except Evolved Followers and Leaders) |
| `stats <atk>/<def>;`                                                  | Attack and Defense                                                                                                                      | ✅ (only Followers and Evolved Followers) |
| `text "<text>";`                                                      | Card Text                                                                                                                               | ❌                                        |
| `evolve <cost>;`                                                      | Evolve Cost                                                                                                                             | ❌                                        |
| `keyword <keyword>;`<br>or<br>`keywords <keyword1>, <keyword2>, ...;` | [Keywords](#keywords)<br>`keyword` and `keywords` (with/without `s`) are freely interchangeable                                         | ❌                                        |
| `ability <parameters> {`<br>`  <effect parameters>`<br>`}`            | [Ability Definition](#ability-definitions)                                                                                              | ❌                                        |

Note that all lines, except ability declarations, end in a semicolon (`;`). Any invalid lines are ignored.

A card declaration must start with the card name. A card declaration does not end until a new card is declared (new card name), or until the end of the file.

## Text Formatting
In-line icons and symbols, such as for `Fanfare` and `Last Words`, should be formatted using text in brackets (case-sensitive). The list of available symbols is:

| Symbol            | Tag                                                                           |
| ----------------- | ----------------------------------------------------------------------------- |
| Attack            | `[attack]`                                                                    |
| Defense           | `[defense]`                                                                   |
| Fanfare           | `[fanfare]`                                                                   |
| Last Words        | `[lastwords]`                                                                 |
| Act               | `[act]`                                                                       |
| Evolve            | `[evolve]`                                                                    |
| Play Point Cost X | `[cost00]` `[cost01]` etc, up to 10                                           |
| Class             | `[forestcraft]` `[swordcraft]` etc<br>OR `[forest]` `[sword]` `[neutral]` etc |
| Engage/Rest       | `[engage]`                                                                    |
| Quick             | `[quick]`                                                                     |
