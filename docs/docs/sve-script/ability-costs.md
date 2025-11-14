---
title: Ability Costs
parent: Card Abilities
nav_order: 2
---
# Effect Costs
The cost for an ability is defined as such:
```
cost (cost1), (cost2), ...;
```
After the `cost` keyword, any number of cost parameters can be defined, with each cost being wrapped in parentheses and separated by commas.
Remember that all ability parameters must end with a semicolon.

## List of Costs

| Syntax                                                                                                 | Description                                                                                                                                        |
| ------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `(PP, <amount>)`                                                                                       | Pay X play points                                                                                                                                  |
| `(EngageSelf)`                                                                                         | Engage this card (that is performing the effect)<br>\*will (probably) break things if trying to pay this cost outside of an Act ability            |
| `(RemoveCounters, <keyword>, <amount>)`<br>`(RemoveCounters, <keyword>, <amount>, <target>, <filter>)` | Remove X counters from target card, matching the given [filter function][Filter Function] if provided ¹<br>If `amount` is not given, defaults to 1 |
| `(EarthRite)`<br>`(EarthRite, <amount>)`                                                               | Remove X Stack counters from an amulet on your field<br>If `amount` is not given, defaults to 1                                                    |
| `(Discard, <amount>)`<br>`(Discard, <amount>, <filter>)`                                               | Discard X cards, matching the given filter function if provided                                                                                    |
| `(BanishFromCemetery, <amount>)`<br>`(BanishFromCemetery, <amount>, <filter>)`                         | Banish X cards from cemetery, matching the given filter function if provided                                                                       |
| `(LeaderDefense, <amount>)`                                                                            | Give your leader -X defense                                                                                                                        |
| `(SendToCemetery, <target>)`<br>`(SendToCemetery, <target>, <filter>)`                                 | Send target card(s) from field to cemetery, matching the given filter function if provided ¹                                                       |
| `(ReturnToHand, <target>)`<br>`(ReturnToHand, <target>, <filter>)`                                     | Return target card(s) from field to owner's hand, matching the given filter function if provided ¹                                                 |
| `(Banish, <target>)`<br>`(Banish, <target>, <filter>)`                                                 | Banishes target card(s) from field, matching the given filter function if provided ¹                                                               |
| `(OncePerTurn)`                                                                                        | This ability can be used once per turn                                                                                                             |

¹ Only supports the following target modes:
- `Self`
- `AllPlayerCards`
- `TargetPlayerCard`

[Filter Function]: ./filter-functions.html
