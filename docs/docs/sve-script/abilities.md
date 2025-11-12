---
title: Card Abilities
parent: SVE Script
nav_order: 2
---
# Ability Definitions

```
ability <trigger> { <ability parameters> }
OR
ability <trigger> name <name> { <ability parameters> }
```

The ability name is optional, and primarily used internally to help differentiate abilities in scripts. However, it can also be used by certain effects (`Sequence` and `ChooseFromList`),
which reference abilities by internal name.

Surrounding the name in quotation marks is optional. If no name is provided, it defaults to the same text as the trigger. It is recommended to give abilities with the same trigger different names, so as to not have multiple abilities with the same name.

Note that some effects are defined as special [keywords](#keywords), such as "this card ignores Ward" being implemented as the `IgnoreWard` keyword.

## Triggers
An ability's trigger defines when it activates.

| Syntax                    | Uses Filter | Description                                                                                                                                                          |
| ------------------------- | ----------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Fanfare`                 | ❌           | Fanfare                                                                                                                                                              |
| `LastWords`               | ❌           | Last Words                                                                                                                                                           |
| `OnLeaveField`            | ❌           | When this card leaves the field                                                                                                                                      |
| `OnReturnToHandFromField` | ❌           | When this card goes from the field to the player's hand                                                                                                              |
| `OnEvolve`                | ❌           | On Evolve                                                                                                                                                            |
| `Strike`                  | ❌           | Strike                                                                                                                                                               |
| `OnOtherEnterField`       | ✅           | When a card (that is not this card) enters this player's field                                                                                                       |
| `OnOtherLeaveField`       | ✅           | When a card (that is not this card) leaves this player's field                                                                                                       |
| `OnOtherAttack`           | ✅           | When a card (that is not this card) on the player's field attacks                                                                                                    |
| `OnPlaySpell`             | ❌           |                                                                                                                                                                      |
| `StartEndPhase`           | ❌           | Start of your end phase                                                                                                                                              |
| `Spell`                   | ❌           | Not triggered automatically. Used for spells and specific effects<br>**NOTE:** Spell cards *always* start with the first ability in the list, regardless of trigger. |
| `Passive`                 | ✅           | Passive Ability, which is always active while this card is on the field.<br>Has unique effects and functionality.                                                    |
| `Activate`                | ❌           | Activated Ability                                                                                                                                                    |

## Ability Parameters

| Syntax                        | Description                                                                                                                       | Notes                                                                                                                                                |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `effect <effect type>;`       | [Effect Definition](#effect-definitions)                                                                                          |                                                                                                                                                      |
| `cost (cost1), (cost2), ...;` | [Effect Costs](#effect-costs)                                                                                                     |                                                                                                                                                      |
| `text <text>;`                | Effect Text<br>(Quotation marks optional)                                                                                         | Displayed when:<br>- Performing any effect that has user interaction (targeting, check top deck, etc.)<br>- In the menu for selecting an Act ability |
| `condition <condition>;`      | Condition that must be met in order to perform the effect. If not met, the effect is skipped/can't be performed.<br>              | `<condition>` is a [value function](#value-functions) that is true if the returned number is greater than 0.                                         |
| `filter <filter>`             | [Filter Function](#filter-functions)<br>Used by specific effect triggers to filter which cards can cause this ability to trigger. | Only supported by specific triggers (see [list of triggers](#triggers))                                                                              |

All parameter lines must end in a semicolon (`;`).

---
# Effect Definitions
Effect definitions have the following syntax:

```
effect <effect name>(<args>);
OR
effect <effect name>(<args>) to <target> <filter>;
```

- `<effect name>` is the [type of effect](#list-of-standard-effects) to perform
- `<args>` is the list of arguments for the effect type, specific to each effect
- The `to` keyword should only be included if a `target` or `filter` is provided
- `<target>` is the target of the effect. Some effects do not use a target, or are limited to specific targets
    - If no target is specified, the default target is `Self`
- `<filter>` is a [filter function](#filter-functions) that controls which cards can be targeted, when applicable
    - A filter can only be used if a target is provided
    - If no filter is provided, it defaults to a blank filter (allows for any card to be targeted)
    - For passive abilities, the filter can be provided as an ability parameter (it's own line) or as a target modifier

## List of Effect Targets
Unless specified otherwise, all target modes only target cards on the field by default

| Target Name                  | Notes                                                                                                                                            |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| `Self`                       | Usually the card performing the effect<br>For some effects (such as `DrawCard`), targets corresponding player<br>Ignores any filter, if provided |
| `Leader`                     | Owner player's leader                                                                                                                            |
| `AllPlayerCards`             | All cards on owner player's field                                                                                                                |
| `AllPlayerCardsEx`           | All cards in owner player's EX area                                                                                                              |
| `AllPlayerCardsFieldAndEx`   | All cards on owner player's field and EX area                                                                                                    |
| `TargetPlayerCard`           | Target card(s) on owner player's field                                                                                                           |
| `TargetPlayerCardEx`         | Target card(s) in owner player's EX area                                                                                                         |
| `TargetPlayerCardOrLeader`   | ⚠️ Not implemented ⚠️                                                                                                                            |
| `Opponent`                   |                                                                                                                                                  |
| `OpponentLeader`             |                                                                                                                                                  |
| `AllOpponentCards`           | All cards on opponent's field                                                                                                                    |
| `TargetOpponentCard`         | Target card(s) on opponent player's field                                                                                                        |
| `TargetOpponentCardOrLeader` | Target card(s) on opponent player's field, or opponent leader<br>Filters do not affect being able to target a leader                             |
| `AllCards`                   | All cards on both player's fields                                                                                                                |
| `AllPlayers`                 |                                                                                                                                                  |
| `AllLeaders`                 |                                                                                                                                                  |
| `TriggerCard`                | Card that triggered this effect<br>Used by the `OnOther` family of ability triggers                                                              |
