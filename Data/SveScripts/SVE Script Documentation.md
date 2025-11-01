# SVE Script Documentation

## Card Data Declaration

| Syntax                                                                | Description                                                                                                                             | Required                                 |
| --------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------- |
| `name <name>;`                                                        | Card Name                                                                                                                               | ✅                                        |
| `id <ID>;`                                                            | Official Card ID                                                                                                                        | ✅                                        |
| `class <class>;`                                                      | Deck Construction Class                                                                                                                 | ✅                                        |
| `universe <universe>;`                                                | Deck Construction Universe                                                                                                              | ❌                                        |
| `type <type>;`                                                        | Card Type<br>`Follower`, `Evolved Follower`, `Spell`, `Amulet`, or `Leader`<br>Tokens should have `/ Token` included in their card type | ✅                                        |
| `trait <trait>;`                                                      | Card Trait(s), separated by ` / `<br>                                                                                                   | ✅ (except Leaders)                       |
| `cost <cost>;`                                                        | Play Point Cost                                                                                                                         | ✅ (except Evolved Followers and Leaders) |
| `stats <atk>/<def>;`                                                  | Attack and Defense                                                                                                                      | ✅ (only Followers and Evolved Followers) |
| `text "<text>";`                                                      | Card Text                                                                                                                               | ❌                                        |
| `evolve <cost>;`                                                      | Evolve Cost                                                                                                                             | ❌                                        |
| `keyword <keyword>;`<br>or<br>`keywords <keyword1>, <keyword2>, ...;` | [Keywords](#keywords)<br>`keyword` and `keywords` (with/without `s`) are freely interchangeable                                         | ❌                                        |
| `ability <parameters> {`<br>`  <effect parameters>`<br>`}`            | [Ability Definition](#ability-definitions)                                                                                              | ❌                                        |

Note that all lines, except ability declarations, end in a semicolon (`;`). Any invalid lines are ignored.

A card declaration must start with the card name. A card declaration does not end until a new card is declared (new card name), or until the end of the file.

### Text Formatting
~~For card and effect text, keywords and key phrases such as `Ward`, `Storm`, or `On Evolve` are automatically bolded. Any other text that should be formatted (such as token or card names) need to be manually formatted using bold and/or italic markdown tags `<b></b>` and `<i></i>`. The custom tag `<bi></bi>` can be used as shorthand for simultaneous bold and italics.~~

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
| `StartEndPhase`           | ❌           | Start of end phase                                                                                                                                                   |
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
Unless specified otherwise, all effects only target cards on the field by default

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

### List of Standard Effects
The following is a list of effects that can be used for all abilities, except for abilities with a `Passive` trigger, which have their own separate list.

| Name                    | Args                                                 | Target | Filter | Description                                                                                                 |
| ----------------------- | ---------------------------------------------------- | ------ | ------ | ----------------------------------------------------------------------------------------------------------- |
| `DealDamage`            | `amount`                                             | ✅      | ✅      | Deals X damage to card or leader                                                                            |
| `DealDamageDivided`     | `amount`                                             | ❌      | ✅¹     | Deals X damage between specified number of target cards                                                     |
| `DrawThenDamage`        | `amount` (to draw), `amount` (to damage)             | ✅      | ✅      | Target a card, draw a card, and then deal damage to targeted card                                           |
| `GiveKeyword`           | `keyword`                                            | ✅      | ✅      | Gives X keyword to card                                                                                     |
| `GiveKeywordEndOfTurn`  | `keyword`                                            | ✅      | ✅      | Gives X keyword to card until end of turn                                                                   |
| `GiveStat`              | `stat`, `amount`                                     | ✅      | ✅      | Gives X stat boost to card or leader                                                                        |
| `ReturnToHand`          |                                                      | ✅      | ✅      | Returns target card (on field) to owner's hand                                                              |
| `BottomDeck`            |                                                      | ✅      | ✅      | Returns target card (on field) to bottom of owner's deck                                                    |
| `TopDeck`               |                                                      | ✅      | ✅      | Returns target card (on field) to top of owner's deck                                                       |
| `TopOrBottomDeck`       |                                                      | ✅      | ✅      | Player chooses to put target card to the top or bottom of it's owner's deck                                 |
| `SetStat`               | `stat`, `amount`                                     | ✅      | ✅      | Directly sets X stat to Y                                                                                   |
| `DestroyCard`           |                                                      | ✅      | ✅      | Destroys target card (send to cemetery)                                                                     |
| `Banish`                |                                                      | ✅      | ✅      | Banishes target card                                                                                        |
| `EngageCard`            |                                                      | ✅      | ✅      | Engages (taps) target card                                                                                  |
| `ReserveCard`           |                                                      | ✅      | ✅      | Reserves (untaps) target card                                                                               |
| `Transform`             | `tokenName`                                          | ✅      | ✅      | Transform target card into specified token                                                                  |
| `OpponentPerformEffect` | `effectName`                                         | ✅      | ✅      | Opponent performs the given effect⁴                                                                         |
| `PerformAsEachTarget`   | `effectName`                                         | ✅      | ✅      | Perform the given effect once for each target card, as if each card performed the effect with target `Self` |
| `DrawCard`              | `amount`                                             | ✅      | ❌      | Target player draws X cards                                                                                 |
| `Mill`                  | `amount`                                             | ✅      | ❌      | Target player sends X cards from top deck to cemetery                                                       |
| `Search`                | `amount`, `filter`, `searchAction`²                  | ❌      | ❌³     | Search X cards from deck, and send them to specified zone                                                   |
| `Salvage`               | `amount`, `filter`²                                  | ❌      | ❌³     | Add X cards from cemetery to hand                                                                           |
| `CemeteryToField`       | `amount`, `filter`²                                  | ❌      | ❌³     | Place X cards from cemetery onto the field                                                                  |
| `CheckTop`              | `amount`, `cardCheckActions[]`                       | ❌      | ❌      | See [Check Top Deck](#check-top-deck) section                                                               |
| `ChooseFromList`        | `effectName[]`                                       | ❌      | ❌      | Player chooses 1 effect from the given list, max 5                                                          |
| `Sequence`              | `effectName[]`                                       | ❌      | ❌      | Performs all given effects in order, max 5                                                                  |
| `SummonToken`           | `tokenName`, `tokenOption`, `amount`                 | ❌      | ❌      | Summon token(s) with given arguments                                                                        |
| `SummonTokenAndTarget`  | `tokenName`, `tokenOption`, `amount`, `effectName[]` | ❌      | ❌      | Summon token(s) with given arguments, then perform the given effects on the summoned tokens in order⁵       |
¹ Explicit number of targets as a min/max in the `filter` is required
² The min/max target amount should be included in the `amount` and not the `filter`
³ `filter` is used as an effect arg instead of a target modifier, meaning it is included in the effect parentheses and not after the `to` keyword
⁴ Detailed breakdown coming soon (tm)
⁵ The effects are performed as normal, with the `filter` of the effect updated to only accept the summoned tokens (i.e. `AllPlayerCards` and `TargetPlayerCard` work as normal but will only affect the summoned tokens)
### List of Passive Effects
Passive abilities have a separate list of effects:

| Name          | Args                                | Has Target | Has Filter | Description                                                                       |
| ------------- | ----------------------------------- | ---------- | ---------- | --------------------------------------------------------------------------------- |
| `GiveKeyword` | `keyword`, `passiveDuration`        | ✅          | ✅          | Gives X [keyword](#keywords) to valid cards on field                              |
| `GiveStat`    | `stat`, `amount`, `passiveDuration` | ✅          | ✅          | Gives X stat boost to valid cards on field<br>`amount` must be a constant integer |
Passive effects only support the following targets:
- `Self`
- `AllPlayerCards`
### List of Effect Args
| Name                 | Type                                 | Notes                                                                                                                                                    |
| -------------------- | ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `amount`             | [Value Function](#value-functions)   |                                                                                                                                                          |
| `keyword`            | [Keyword](#keywords)                 | Limit one keyword                                                                                                                                        |
| `stat`               | Enum                                 | - `Attack` or `Atk`<br>- `Defense` or `Def`<br>- `AttackDefense` or `AtkDef`<br>- `EvolveCost`<br>- `MaxPlayPoints` or `MaxPP`<br>- `PlayPoints` or `PP` |
| `filter`             | [Filter Function](#filter-functions) | Only applicable when used as an arg and *not* a target modifier (when inside parentheses, and not after the `to` keyword)                                |
| `tokenName`          | String                               | Must surround in quotes if the name contains a comma                                                                                                     |
| `tokenOption`        | Enum                                 | - `Field`<br>- `ExArea`<br>- `FieldOverflowToEx`<br>- `ChooseForEachFieldOrEx`                                                                           |
| `searchAction`       | Enum                                 | - `Hand`<br>- `Cemetery`                                                                                                                                 |
| `cardCheckActions[]` | List of Actions                      | See [Check Top Deck](#check-top-deck) section<br>Minimum of 1, maximum of 3                                                                              |
| `effectName`         | String                               | The defined `name` of an ability on this card                                                                                                            |
| `effectName[]`       | List of Strings                      | List of strings, each one being the defined `name` of an ability, separated by commas. Quotation marks optional.<br>Minimum of 1, maximum of 5           |
| `passiveDuration`    | Enum                                 | - `WhileOnField`<br>- `OpponentTurn`                                                                                                                     |

#### Check Top Deck
The `CheckTop` effect and its unique argument, `cardCheckActions[]`, is used to perform actions from the top of the deck, such as scrying or milling. A maximum of 3 check actions can be provided.

The `amount` parameter is the initial amount of cards revealed for the effect.

Card check actions are defined using one of 3 syntaxes, with each action enclosed in parentheses:
```
(<action>)
(<action>, <amount>)
(<action>, <filter>, <amount>)
```

- `action` is the type of action to perform (see list below)
- `amount` is the amount of cards selected to perform the effect on. Can be a raw `int`, [value function](#value-functions), or a min/max value (format: `m(<min>,<max>)`)
   - If an amount is not required and not specified, the action targets all remaining cards
- `filter` is the [filter function](#filter-functions) to use to determine valid action targets

| Action               | Requires Amount | Description                                                                                                              |
| -------------------- | --------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `Hand`               | ❌               | Adds selected cards to hand                                                                                              |
| `Cemetery`           | ✅               | Sends selected cards to the cemetery                                                                                     |
| `Field`              | ❌               | Puts selected cards onto the field                                                                                       |
| `TopDeckSameOrder`   | ❌               | Returns all remaining cards to top of the deck, retaining card order                                                     |
| `TopDeckAnyOrder`    | ❌               | ⚠️ Not implemented ⚠️                                                                                                    |
| `BottomDeckAnyOrder` | ❌               | Sends all remaining cards to the bottom of the deck, retaining card order<br>Players can reorder cards before confirming |

After all given actions are performed, any remaining cards revealed are returned to the top of the deck in their initial order (as if `TopDeckSameOrder` was called as the last action).

Examples:
```
// "Look at the top card of your deck. You may put it into your cemetery."

effect CheckTop(1, (Cemetery, m(0,1)));


// "Look at the top 4 cards of your deck. You may reveal a spell from among them and put it into your hand. You may put a spell from among them into your cemetery. Put the rest on the bottom of your deck in any order."

effect CheckTop(4, (Hand, S, m(0,1)), (Cemetery, S, m(0,1)), (BottomDeckAnyOrder));
```

---
### Effect Costs
The cost for an effect is defined as such:
```
cost (cost1), (cost2), ...;
```
After the `cost` keyword, any number of cost parameters can be defined, with each cost being wrapped in parentheses and separated by commas.
Remember that all ability parameters must end with a semicolon.

List of Costs:

| Syntax                                                                 | Description                                                                                                                             |
| ---------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------- |
| `(PP, <amount>)`                                                       | Pay X play points                                                                                                                       |
| `(EngageSelf)`                                                         | Engage this card (that is performing the effect)<br>\*will (probably) break things if trying to pay this cost outside of an Act ability |
| `(Discard, <amount>)`<br>`(Discard, <amount>, <filter>)`               | Discard X cards, matching the given [filter function](#filter-functions) if provided                                                    |
| `(LeaderDefense, <amount>)`                                            | Give your leader -X defense                                                                                                             |
| `(SendToCemetery, <target>)`<br>`(SendToCemetery, <target>, <filter>)` | Send target card(s) from field to cemetery, matching the given [filter function](#filter-functions) if provided ¹                       |
| `(ReturnToHand, <target>)`<br>`(ReturnToHand, <target>, <filter>)`     | Return target card(s) from field to owner's hand, matching the given [filter function](#filter-functions) if provided ¹                 |
| `(Banish, <target>)`<br>`(Banish, <target>, <filter>)`                 | Banishes target card(s) from field, matching the given [filter function](#filter-functions) if provided ¹                               |
| `(OncePerTurn)`                                                        | This ability can be used once per turn                                                                                                  |
¹ Only supports the following target modes:
- `Self`
- `AllPlayerCards`
- `TargetPlayerCard`

---
## Value Functions
Returns a value `n1`. If given `condition` is met, returns `n2` instead.

```
<n1>
OR
<n1><function><n2>
```

Where:
- `<n1>` is the initial value. **Defaults to 0** if blank
- `<function>` is a value function, either conditional or arithmetic (see below)
- `<n2>` is the value to use based on the given conditional function. **Defaults to 1** if blank
	- Conditional - Returns `<n2>` if the conditional function is true
	- Arithmetic - Applies a math function between `<n1>` and `<n2>`
To perform nested value functions, either `n` value can be replaced with another value function inside of brackets `[ ]`
### Dynamic Number Values
The following can be used as `<n1>`  or `<n2>` in place of a constant `int` to return a dynamic value.

| Syntax        | Returns                                                             | Example                                          |
| ------------- | ------------------------------------------------------------------- | ------------------------------------------------ |
| `f(<filter>)` | Count of cards on <u>F</u>ield<br>with filter `<filter>`            | `f(FE)`<br>Number of Evolved Followers on field  |
| `x(<filter>)` | Count of cards in E<u>X</u> area with filter `<filter>`             | `x(KS)`<br>Number of spell tokens in EX area<br> |
| `p(<filter>)` | Count of cards on O<u>p</u>ponent's Field<br>with filter `<filter>` | `p(A)`<br>Number of Amulets on opponent's field  |
| `C`           | <u>C</u>ombo (number of cards played this turn)                     |                                                  |
| `H`           | Number of cards in <u>h</u>and                                      |                                                  |
| `A`           | This card's <u>A</u>ttack                                           |                                                  |
| `D`           | This card's <u>D</u>efense                                          |                                                  |

### Conditional Functions
Conditional functions are case-sensitive. If the conditional function is met, returns `<n2>` instead of `<n1>`. If `<n2>` is not provided and the condition is met, `<n2>` defaults to a value of 1.

| Syntax | Function          | Example                                         |
| ------ | ----------------- | ----------------------------------------------- |
| `c(X)` | <u>C</u>ombo (X)    | `1c(3)4`<br>1, or if Combo (3), then 4.         |
| `s(X)` | <u>S</u>pellchain (X)  | `1s(3)4`<br>1, or if Spellchain (3), then 4.    |
| `o`    | <u>O</u>verflow          | `1o4`<br>1, or if Overflow is active, then 4.   |
| `n(X)` | <u>N</u>ecrocharge (X) | `1n(10)4`<br>1, or if Necrocharge (10), then 4. |

### Arithmetic Functions
Arithmetic functions apply math to both sides of the value formula to calculate the return value. Boolean functions, such as greater than or less than, return 1 if the comparison is true, and return 0 otherwise.

| Syntax | Function     | Example                                                                                 |
| ------ | ------------ | --------------------------------------------------------------------------------------- |
| `+`    | Addition     | `1+H`<br>1 + Number of cards in hand                                                    |
| `-`    | Subtraction  | `C-1`<br>Combo - 1                                                                      |
| `>`    | Greater Than | `A>3`<br>If attack is greater than 3, returns `1`, otherwise `0`                        |
| `<`    | Less Than    | `D<3`<br>If Defense is less than 2, returns `1`, otherwise `0`                          |
| `\|`   | Boolean OR   | `[A>3] \| [D>3]`<br>If Attack or Defense is greater than 3, returns `1`, otherwise `0`  |
| `&`    | Boolean AND  | `[A>3] & [D>3]`<br>If Attack and Defense are greater than 3, returns `1`, otherwise `0` |

---
## Filter Functions
Filter functions are strings used to filter effect targets. A blank filter function matches all cards. Filter functions are case-sensitive

| Syntax         | Filter                 | Notes                                                                                                |
| -------------- | ---------------------- | ---------------------------------------------------------------------------------------------------- |
| `F`            | <u>F</u>ollower        |                                                                                                      |
| `S`            | <u>S</u>pell           |                                                                                                      |
| `A`            | <u>A</u>mulet          |                                                                                                      |
| `E`            | <u>E</u>volved         |                                                                                                      |
| `K`            | To<u>k</u>en           |                                                                                                      |
| `R`            | <u>R</u>eserved        | Untapped. All cards not on the field are considered reserved                                         |
| `N`            | E<u>n</u>gaged         | Tapped. All cards not on the field are considered not engaged                                        |
| `a(<amount>)`  | <u>A</u>ttack          | Supports min/max amount `a(m(<min>,<max>))`                                                          |
| `d(<amount>)`  | <u>D</u>efense         | Supports min/max amount `d(m(<min>,<max>))`                                                          |
| `e(<amount>)`  | <u>E</u>volve Cost     | All cards that can't evolve have an evolve cost of -1<br>Supports min/max amount `e(m(<min>,<max>))` |
| `p(<amount>)`  | <u>P</u>lay Point Cost | Supports min/max amount `p(m(<min>,<max>))`                                                          |
| `X`            | E<u>x</u>clude Self    | Excludes the card that the effect is originating from                                                |
| `t(<trait>)`   | <u>T</u>rait           | Cards with given trait (case-sensitive)<br>Example: `t(Pixie)` checks for "Pixie" trait              |
| `k(<keyword>)` | <u>K</u>eyword         | Cards with given keyword (case-sensitive)<br>Example: `k(Ward)` checks for cards with Ward           |
| `c(<class>)`   | Class                  | "-craft" suffix is optional                                                                          |
| `n(<name>)`    | Card <u>N</u>ame       | Cards with given name (case-sensitive)                                                               |
| `i(<int>[])`   | Instance IDs           | ⚠️ FOR INTERNAL/BACKEND USE ONLY<br>Filters for cards with given instance IDs                        |

Special Parameters:

| Syntax           | Filter  | Notes                                                                                                           |
| ---------------- | ------- | --------------------------------------------------------------------------------------------------------------- |
| `m(<min>,<max>)` | Min/Max | Specifies a specific minimum and maximum amount of cards that can be targeted.<br>Default is a min and max of 1 |
| `!`              | Inverse | Applies an inverse/NOT check to the next filter entry<br>Example: `F!E` = Non-evolved Followers                 |

### Shortcut Filters
Shortcuts can be used as shorthand for commonly used filters. All shortcuts start with a `#` symbol.

Internally, these are replaced with it's corresponding value, as if the full filter was typed out, and *does not* have unique logic for handling. Because of this, shortcuts do not support inverse filters `!`.

| Shortcut | Value        | Notes                  |
| -------- | ------------ | ---------------------- |
| `#F`     | `F!k(Aura)`  | Followers without Aura |
| `#a`     | `!k(Aura)`   | Card without Aura      |
| `#e`     | `e(m(0,99))` | Card with Evolve       |

### Filter Notes
All effects that select a target automatically ignore cards with Aura on the opponent's field, so Aura does not need to be specified for in lines such as `TargetOpponentCard F`. However, Aura does need to be checked for in other items such as `condition` lines or other scenarios where a filter is used outside of a target modifier.

---
## Keywords
The following game keywords are supported:
- Ward
- Storm
- Rush
- Assail
- Intimidate (not implemented)
- Drain
- Bane
- Aura
- Quick

The following are uSVE internal "keywords" used to handle certain passive effects:

| Keyword             | Description                                                                  |
| ------------------- | ---------------------------------------------------------------------------- |
| `IgnoreWard`        | This card ignores ward                                                       |
| `PutOnFieldEngaged` | This card is put onto the field engaged                                      |
| `CannotDealDamage`  | This card cannot deal damage                                                 |
| `Plus1Damage`       | This (follower) deals 1 more damage<br>(not the same as an attack stat buff) |
| `Plus2Damage`       | This (follower) deals 2 more damage<br>(not the same as an attack stat buff) |

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
