---
title: SVE Script
nav_order: 2
---
## Card Data Declaration

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

### Text Formatting
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

---
## Ability Definitions
```
ability <trigger> { <ability parameters> }
OR
ability <trigger> name <name> { <ability parameters> }
```

The ability name is optional, and primarily used internally to help differentiate abilities in scripts. However, it can also be used by certain effects (`Sequence` and `ChooseFromList`),
which reference abilities by internal name.

Surrounding the name in quotation marks is optional. If no name is provided, it defaults to the same text as the trigger. It is recommended to give abilities with the same trigger different names, so as to not have multiple abilities with the same name.

Note that some effects are defined as special [keywords](#keywords), such as "this card ignores Ward" being implemented as the `IgnoreWard` keyword.

### Triggers
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

### Ability Parameters

| Syntax                        | Description                                                                                                                       | Notes                                                                                                                                                |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `effect <effect type>;`       | [Effect Definition](#effect-definitions)                                                                                          |                                                                                                                                                      |
| `cost (cost1), (cost2), ...;` | [Effect Costs](#effect-costs)                                                                                                     |                                                                                                                                                      |
| `text <text>;`                | Effect Text<br>(Quotation marks optional)                                                                                         | Displayed when:<br>- Performing any effect that has user interaction (targeting, check top deck, etc.)<br>- In the menu for selecting an Act ability |
| `condition <condition>;`      | Condition that must be met in order to perform the effect. If not met, the effect is skipped/can't be performed.<br>              | `<condition>` is a [value function](#value-functions) that is true if the returned number is greater than 0.                                         |
| `filter <filter>`             | [Filter Function](#filter-functions)<br>Used by specific effect triggers to filter which cards can cause this ability to trigger. | Only supported by specific triggers (see [list of triggers](#triggers))                                                                              |

All parameter lines must end in a semicolon (`;`).

---
## Effect Definitions
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

### List of Effect Targets
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

### List of Effects
See [[SVE Script Effect List]] page

### Effect Costs
The cost for an effect is defined as such:
```
cost (cost1), (cost2), ...;
```
After the `cost` keyword, any number of cost parameters can be defined, with each cost being wrapped in parentheses and separated by commas.
Remember that all ability parameters must end with a semicolon.

List of Costs:

| Syntax                                                                                                 | Description                                                                                                                                          |
| ------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| `(PP, <amount>)`                                                                                       | Pay X play points                                                                                                                                    |
| `(EngageSelf)`                                                                                         | Engage this card (that is performing the effect)<br>\*will (probably) break things if trying to pay this cost outside of an Act ability              |
| `(RemoveCounters, <keyword>, <amount>)`<br>`(RemoveCounters, <keyword>, <amount>, <target>, <filter>)` | Remove X counters from target card, matching the given [filter function](#filter-functions) if provided ¹<br>If `amount` is not given, defaults to 1 |
| `(EarthRite)`<br>`(EarthRite, <amount>)`                                                               | Remove X Stack counters from an amulet on your field<br>If `amount` is not given, defaults to 1                                                      |
| `(Discard, <amount>)`<br>`(Discard, <amount>, <filter>)`                                               | Discard X cards, matching the given [filter function](#filter-functions) if provided                                                                 |
| `(BanishFromCemetery, <amount>)`<br>`(BanishFromCemetery, <amount>, <filter>)`                         | Banish X cards from cemetery, matching the given [filter function](#filter-functions) if provided                                                    |
| `(LeaderDefense, <amount>)`                                                                            | Give your leader -X defense                                                                                                                          |
| `(SendToCemetery, <target>)`<br>`(SendToCemetery, <target>, <filter>)`                                 | Send target card(s) from field to cemetery, matching the given [filter function](#filter-functions) if provided ¹                                    |
| `(ReturnToHand, <target>)`<br>`(ReturnToHand, <target>, <filter>)`                                     | Return target card(s) from field to owner's hand, matching the given [filter function](#filter-functions) if provided ¹                              |
| `(Banish, <target>)`<br>`(Banish, <target>, <filter>)`                                                 | Banishes target card(s) from field, matching the given [filter function](#filter-functions) if provided ¹                                            |
| `(OncePerTurn)`                                                                                        | This ability can be used once per turn                                                                                                               |

¹ Only supports the following target modes:
- `Self`
- `AllPlayerCards`
- `TargetPlayerCard`

---
# Script Examples

### [Aria, Fairy Princess](https://en.shadowverse-evolve.com/cards/?cardno=SD01-001EN)
Original Text:
```
Ward.
[fanfare] Put up to 9 Fairy tokens onto your field or into your EX area.
While this card is on your field, your other Pixie followers have Rush.
```
Script:
```
name Aria, Fairy Princess;
id SD01-001;
class Forest;
type Follower;
trait Pixie / Princess;
rarity Legendary;
cost 6;
stats 5/5;

text "Ward.
[fanfare] Put up to 9 Fairy tokens onto your field or into your EX area.
While this card is on your field, your other Pixie followers have Rush.";

keywords Ward;
ability Fanfare name "Fanfare" {
    text "Put up to 9 Fairy tokens onto your field or into your EX area.";  
    effect SummonToken(Fairy, ChooseForEachFieldOrEx, 9);
}
ability Passive name "Give Rush" {
    effect GiveKeyword(Rush, WhileOnField) to AllPlayerCards Ft(Pixie)X;
}
```

### [Titania's Sanctuary](https://en.shadowverse-evolve.com/cards/?cardno=SD01-002EN)
Original Text:
```
[fanfare] Give each Pixie token on your field [attack]+1/[defense]+1.
While this card is on your field, your Pixie tokens have Assail.
Whenever a Pixie token is put onto your field, give it [attack]+1/[defense]+1.
```
Script:
```
name Titania's Sanctuary;
id SD01-002;
class Forest;
type Amulet;
trait Pixie / Princess;
rarity Gold;
cost 2;

text "[fanfare] Give each Pixie token on your field [attack]+1/[defense]+1.
While this card is on your field, your Pixie tokens have Assail. Whenever a Pixie token is put onto your field, give it [attack]+1/[defense]+1.";

ability Fanfare {
    text "Give each Pixie token on your field [attack]+1/[defense]+1.";
    effect GiveStat(AtkDef, 1) to AllPlayerCards Kt(Pixie);
}
ability Passive name "Give Assail" {
    effect GiveKeyword(Assail, WhileOnField) to Kt(Pixie);
}
ability OnOtherEnterField {
    filter Kt(Pixie);  
    text "Whenever a Pixie token is put onto your field, give it [attack]+1/[defense]+1.";
    effect GiveStat(AtkDef, 1) to TriggerCard;
}
```

### [Lightning Shooter](https://en.shadowverse-evolve.com/cards/?cardno=SD03-007EN)
Original Text:
```
[fanfare] Select an enemy follower on the field and deal it 2 damage. Spellchain (5): Deal 4 damage instead.
SC (10): Deal 2 damage to that follower's leader.
```
Script:
```
name Lightning Shooter;
id BP01-070;
class Rune;
type Follower;
trait Mage;
rarity Bronze;
cost 4;
stats 3/3;

text "[fanfare] Select an enemy follower on the field and deal it 2 damage. Spellchain (5): Deal 4 damage instead. SC (10): Deal 2 damage to that follower's leader. (If you have at least 5 spells in your cemetery, perform Spellchain (5). Then, if you have at least 10 spells, perform SC (10).)";

ability Fanfare {
    text "Select an enemy follower on the field and deal it 2 damage. Spellchain (5): Deal 4 damage instead. SC (10): Deal 2 damage to that follower's leader.";
    effect Sequence("Damage Follower", "Damage Leader");
}
ability Spell name "Damage Follower" {
    text "Select an enemy follower on the field and deal it 2 damage. Spellchain (5): Deal 4 damage instead. SC (10): Deal 2 damage to that follower's leader.";
    effect DealDamage(2s(5)4) to TargetOpponentCard F;
}
ability Spell name "Damage Leader" {
    condition s(10);
    effect DealDamage(2) to OpponentLeader;
}
```
