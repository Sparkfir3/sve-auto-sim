---
title: Creating New Effects
parent: Creating New Cards
nav_order: 1
---
# Creating New Card Effects
To create a new effect, start by creating a new C# script in the approriate subfolder in the [`Scripts/Gameplay/CardAbilities/Effects`][Effects Folder] folder. An effect script template is available under `Right Click > Create/Script/SVE Effect`

The new script's class should be in the `SVESimulator` namespace and inherit from `SveEffect` (the template does this for you), and implement the `Resolve()` function, which is where the execution logic occurs.

Reference existing effect scripts to get a closer idea on what they should look like and how to implement them.

### Effect Arguments
Effect arguments used in the SVE script parser, such as `amount` or `target`, are read as field variables in the effect script.

These field variables used *must* match the name of any existing effect variables in use by other effects, since the script parser reads and converts them as string literals.

The appropriate `IntField`, `StringField`, or `EnumField` property is required, but the `Order` property isn't and only a "nice to have."

### Implementing Resolve
Some core points on implementing the `Resolve` function logic:
- The `ResolveOnTarget()` function (inherited from `SveEffect`) lets you use the `target` argument enum to obtain the appropriate targeted card(s) to perform effects on
- Effects will primarily use the `PlayerEventControllerLocal` class, fetched from the `player.LocalEvents` variable, to perform actions on cards or players

---

# Updating the Script Parser
Whenever a new effect is created, the SVE script parser must also be updated to support being able to read it.

If the new effect does not have any new argument types, it can be added to the parser in the `SveScriptParseEffect.cs` script. Near the bottom of the file is a dictionary containing info about all effects, and a new line can be added (in the appropriate labeled section).

Examples:

```
{ "DealDamage", new EffectParams("SVESimulator.SveDealDamageEffect",  EffectParameterType.Amount) }
{ "GiveStat", new EffectParams("SVESimulator.SveGiveStatBoostEffect", EffectParameterType.StatType, EffectParameterType.Amount) }
```

[Effects Folder]: https://github.com/Sparkfir3/sve-auto-sim/tree/main/Scripts/Gameplay/CardAbilities/Effects
