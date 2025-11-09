---
title: Effect List
parent: SVE Script
nav_order: 1
---
# Triggered Effects
## Card Movement Effects
---
Effects that handle moving a card from one zone to another:

| Name                                 | Args                                     | Target | Filter | Description                                                                           |
| ------------------------------------ | ---------------------------------------- | ------ | ------ | ------------------------------------------------------------------------------------- |
| `DrawCard`                           | `amount`                                 | ✅      | ❌      | Target player draws X cards                                                           |
| `DrawThenDamage`                     | `amount` (to draw), `amount` (to damage) | ✅      | ✅      | Target a card, draw X cards, then deal damage Y to targeted card                      |
| `ReturnToHand`                       |                                          | ✅      | ✅      | Returns target card to owner's hand                                                   |
| `BottomDeck`                         |                                          | ✅      | ✅      | Returns target card to bottom of owner's deck                                         |
| `TopDeck`                            |                                          | ✅      | ✅      | Returns target card to top of owner's deck                                            |
| `TopOrBottomDeck`                    |                                          | ✅      | ✅      | Player chooses to put target card to the top or bottom of it's owner's deck           |
| `DestroyCard`                        |                                          | ✅      | ✅      | Destroys target card (send to cemetery)                                               |
| `Banish`                             |                                          | ✅      | ✅      | Banishes target card                                                                  |
| `Search`                             | `amount`, `filter`, `searchAction`¹      | ❌      | ❌²     | Search X cards from deck, and send them to specified zone                             |
| `Salvage`                            | `amount`, `filter`¹                      | ❌      | ❌²     | Add X cards from cemetery to hand                                                     |
| `CemeteryToField`                    | `amount`, `filter`¹                      | ❌      | ❌²     | Place X cards from cemetery onto the field                                            |
| `PlaySpellFrom`<br>`Cemetery`        | `filter`                                 | ❌      | ❌      | Play target spell from cemetery, using its cost                                       |
| `PlaySpellFrom`<br>`CemeterySetCost` | `filter`, `amount`                       | ❌      | ❌      | Play target spell from cemetery, ignoring its cost and using the given amount instead |
| `Mill`                               | `amount`                                 | ✅      | ❌      | Target player sends X cards from top deck to cemetery                                 |

¹ The min/max target amount should be included in the `amount` and not the `filter`
² `filter` is still used, but as an effect arg instead of a target modifier, meaning it is included in the effect parentheses and not after the `to` keyword

See also: [Check Top Deck](#check-top-deck) effect

## Stat Effects
---
Effects that handle card stats, such as attack and defense. This includes a card's engaged/reserved (tapped/untapped) status.

| Name                | Args                                     | Target | Filter | Description                                                       |
| ------------------- | ---------------------------------------- | ------ | ------ | ----------------------------------------------------------------- |
| `DealDamage`        | `amount`                                 | ✅     | ✅     | Deals X damage to card or leader                                  |
| `DealDamageDivided` | `amount`                                 | ❌     | ✅¹    | Deals X damage between specified number of target cards           |
| `DrawThenDamage`    | `amount` (to draw), `amount` (to damage) | ✅     | ✅     | Target a card, draw a card, and then deal damage to targeted card |
| `GiveStat`          | `stat`, `amount`                         | ✅     | ✅     | Gives X stat increase to target card or leader                    |
| `SetStat`           | `stat`, `amount`                         | ✅     | ✅     | Directly sets X stat to Y                                         |
| `EngageCard`        |                                          | ✅     | ✅     | Engages (taps) target card                                        |
| `ReserveCard`       |                                          | ✅     | ✅     | Reserves (untaps) target card                                     |

¹ Explicit number of targets as a min/max in the `filter` is required

## Keyword Effects
---
Effects that handle card keywords

| Name                   | Args      | Target | Filter | Description                               |
| ---------------------- | --------- | ------ | ------ | ----------------------------------------- |
| `GiveKeyword`          | `keyword` | ✅     | ✅     | Gives X keyword to card                   |
| `GiveKeywordEndOfTurn` | `keyword` | ✅     | ✅     | Gives X keyword to card until end of turn |

## Counter Effects
---
Effects that handle counters

| Name            | Args                | Target | Filter | Description                                                                                   |
| --------------- | ------------------- | ------ | ------ | --------------------------------------------------------------------------------------------- |
| `GiveCounter`   | `counter`, `amount` | ✅      | ✅      | Give X number of Y counters to card                                                           |
| `RemoveCounter` | `counter`, `amount` | ✅      | ✅      | Remove X number of Y counters to card<br>If `amount` is not given, removes all counters       |
| `MoveCounters`  | `counter`, `amount` | ✅¹     | ✅      | Move X counters from this card to target card<br>If `amount` is not given, moves all counters |

¹ Only supports target mode `TargetPlayerCard`, and any min/max target count in the filter will be ignored

## Token Effects
---
Effects that handle creating tokens

| Name                   | Args                                                 | Target | Filter | Description                                                                                           |
| ---------------------- | ---------------------------------------------------- | ------ | ------ | ----------------------------------------------------------------------------------------------------- |
| `SummonToken`          | `tokenName`, `tokenOption`, `amount`                 | ❌      | ❌      | Summon token(s) with given arguments                                                                  |
| `SummonTokenAndTarget` | `tokenName`, `tokenOption`, `amount`, `effectName[]` | ❌      | ❌      | Summon token(s) with given arguments, then perform the given effects on the summoned tokens in order¹ |
| `Transform`            | `tokenName`                                          | ✅      | ✅      | Transform target card into specified token                                                            |

¹ The effects are performed as normal, with the `filter` of the effect updated to only accept the summoned tokens. For example:
- `AllPlayerCards` with target all tokens (on the field) that were summoned
- `TargetPlayerCard` has the player choose a target(s) only from the summoned tokens (on the field)

## Effect Execution
---
Effects that handle unique effect execution, such as performing multiple effects in order or choosing from a list

| Name                    | Args           | Target | Filter | Description                                                                                                 |
| ----------------------- | -------------- | ------ | ------ | ----------------------------------------------------------------------------------------------------------- |
| `Sequence`              | `effectName[]` | ❌      | ❌      | Performs all given effects in order, max 5                                                                  |
| `ChooseFromList`        | `effectName[]` | ❌      | ❌      | Player chooses 1 effect from the given list, max 5                                                          |
| `OpponentPerformEffect` | `effectName`   | ✅      | ✅      | Opponent performs the given effect¹                                                                         |
| `PerformAsEachTarget`   | `effectName`   | ✅      | ✅      | Perform the given effect once for each target card, as if each card performed the effect with target `Self` |

¹ Detailed breakdown coming soon (tm)

## Other Effects
---

| Name        | Args                           | Target | Filter | Description                                   |
| ----------- | ------------------------------ | ------ | ------ | --------------------------------------------- |
| `CheckTop`  | `amount`, `cardCheckActions[]` | ❌      | ❌      | See [Check Top Deck](#check-top-deck) section |
| `ExtraTurn` |                                | ❌      | ❌      | Take an extra turn after this one             |

### Check Top Deck
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


# Passive Effects
---
Passive abilities (using the `Passive` trigger) have a separate list of effects from regular abilities:

| Name             | Args                                | Has Target | Has Filter | Description                                                                          |
| ---------------- | ----------------------------------- | ---------- | ---------- | ------------------------------------------------------------------------------------ |
| `GiveKeyword`    | `keyword`, `passiveDuration`        | ✅          | ✅          | Gives X [keyword](#keywords) to valid cards on field                                 |
| `GiveStat`       | `stat`, `amount`, `passiveDuration` | ✅          | ✅          | Gives X stat boost to valid cards on field<br>`amount` must be a constant integer    |
| `MinusCostOther` | `amount`, `passiveDuration`         | ✅          | ✅          | Reduces play point cost of all valid targets in hand and EX area by specified amount |

Passive effects only support the following targets modes:
- `Self`
- `AllPlayerCards`

## Modified Cost Effects
---
Modify cost abilities (using the `ModifiedCost` trigger) also have a separate list of effects, which handle non-standard card costs:

| Name            | Args     | Has Target | Has Filter | Description                                                                                                                                                                                                          |
| --------------- | -------- | ---------- | ---------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ReducedCost`   | `amount` | ❌          | ❌          | Reduces the play point cost by the given amount<br>Negative reduce amount is not supported<br>Final cost will be clamped to 0 or higher.                                                                             |
| `AlternateCost` |          | ❌          | ❌          | Defines that the card has an alternate cost that can be used instead of their normal play point cost<br>The specific alt cost must be defined as a cost in the ability definition (outside of the effect definition) |

Modified costs only apply to cards played from hand or the EX area

# List of Effect Arguments
---

| Name                 | Type                                 | Notes                                                                                                                                                    |
| -------------------- | ------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `amount`             | [Value Function](#value-functions)   |                                                                                                                                                          |
| `keyword`            | [Keyword](#keywords)                 |                                                                                                                                                          |
| `counter`            | Counter                              |                                                                                                                                                          |
| `stat`               | Enum                                 | - `Attack` or `Atk`<br>- `Defense` or `Def`<br>- `AttackDefense` or `AtkDef`<br>- `EvolveCost`<br>- `MaxPlayPoints` or `MaxPP`<br>- `PlayPoints` or `PP` |
| `filter`             | [Filter Function](#filter-functions) | Only applicable when used as an arg and *not* a target modifier (when inside parentheses, and not after the `to` keyword)                                |
| `tokenName`          | String                               | Must surround in quotes if the name contains a comma                                                                                                     |
| `tokenOption`        | Enum                                 | - `Field`<br>- `ExArea`<br>- `FieldOverflowToEx`<br>- `ChooseForEachFieldOrEx`                                                                           |
| `searchAction`       | Enum                                 | - `Hand`<br>- `Cemetery`                                                                                                                                 |
| `cardCheckActions[]` | List of Actions                      | See [Check Top Deck](#check-top-deck) section<br>Minimum of 1, maximum of 3                                                                              |
| `effectName`         | String                               | The defined `name` of an ability on this card                                                                                                            |
| `effectName[]`       | List of Strings                      | List of strings, each one being the defined `name` of an ability, separated by commas. Quotation marks optional.<br>Minimum of 1, maximum of 5           |
| `passiveDuration`    | Enum                                 | - `WhileOnField`<br>- `OpponentTurn`                                                                                                                     |
