---
title: Examples
parent: SVE Script
nav_order: 6
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

## [Titania's Sanctuary](https://en.shadowverse-evolve.com/cards/?cardno=SD01-002EN)
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

## [Lightning Shooter](https://en.shadowverse-evolve.com/cards/?cardno=SD03-007EN)
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
