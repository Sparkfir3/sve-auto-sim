---
title: SVE Script
nav_order: 2
---
# SVE Script
The SVE simulator uses a custom language to store information about cards and their properties, and to define the functionality of each card's abilities.

### Example: [Aria, Fairy Princess](https://en.shadowverse-evolve.com/cards/?cardno=SD01-001EN)
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
