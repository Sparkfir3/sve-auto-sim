---
title: Keywords & Counters
parent: SVE Script
nav_order: 2
---
# Keywords
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
- Stack
Currently, a card cannot have more than 1 instance of any given keyword, with the exception of `Stack`, as that is handled internally as a counter, which has different logic than normal keywords. This may be updated later, if needed.

## Effects as Keywords
The following are uSVE internal "keywords" used to handle certain passive effects:

| Keyword             | Description                                                                  |
| ------------------- | ---------------------------------------------------------------------------- |
| `IgnoreWard`        | This card ignores ward                                                       |
| `PutOnFieldEngaged` | This card is put onto the field engaged                                      |
| `CannotDealDamage`  | This card cannot deal damage                                                 |
| `Plus1Damage`       | This (follower) deals 1 more damage<br>(not the same as an attack stat buff) |
| `Plus2Damage`       | This (follower) deals 2 more damage<br>(not the same as an attack stat buff) |

# Counters
List of supported counter names:
- Stack
- Spell
