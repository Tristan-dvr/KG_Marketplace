Buffer npc that can be set in the world with pre-configured "buffs" that can be temporarily enabled on the players items. When a player interacts with the npc they can choose from what type of buff they want and on what inventory item it gets placed.

# Buffs

The Database config is a file with ALL Your buffs. Here you will need to add all buffs so later you can use them in NPC profiles that you setup.

To create a new database with buffs create a .cfg with any name in `Valheim\BepInEx\config\Marketplace\Configs\Buffers` and fill it with data.

Each buff should have a UNIQUE name (it will be its own Unique ID). Buff should have a layout like this:

```cfg
[UniqueName]   
Name   
Duration (seconds)   
Buff Icon (Can be taken from monster prefab name or item prefab name)   
Price prefab name, Price count   
Buff modifiers    
Buff visual effect    
Buff group

Example:

[TestBuff]    
First buff i created    
180   
SwordIron   
Coins, 1     
ModifyAttack = 1.5   
vfx_Burning    
Combat
```

^ Creates buff with duration 180 sec, icon = SwordIron icon,  price = 1 coin, Modifiers are ModifyAttack x1.5,
visual effect is burning and group is Combat.    
<br>

<br>
Modifiers   

All possible modfifiers: `ModifyAttack`, `ModifyHealthRegen` , `ModifyStaminaRegen`, `ModifyRaiseSkills`, `ModifySpeed`, `ModifyNoise`,
`ModifyMaxCarryWeight`, `ModifyStealth`, `RunStaminaDrain`, `DamageReduction `

Note: Multiple buffs can be applied at once by putting a "," between them such as;   
ModifySpeed = 1.2, ModifyNoise = 1.4

One "buff" can have nine different modifiers, and the Buff Group combines Buff modifiers into one group. This is done only for balancing, so you can make cheap buffs, normal buffs, and high-priced buffs.   
Note: If buffs are in the same group then player would be able to buy only ONE BUFF OUT OF GROUP at a time. See below there are two examples in the "exploration" group, so only one could be purchases/applied at a time.   
<br>

# Profiles  

Profiles located in `Valheim\BepInEx\config\Marketplace\Configs\BufferProfiles`, simply create ANYFILENAME.cfg and fill it with data.

Buffs need to be applied to an NPC profile in order to work. To add a new profile you need to write [ProfileName] , and on a new line add buffer list for it (buff unique IDs from BufferDATABASE.cfg)

```cfg
[MeadowsBuffs]    
TestBuff1, TestBuff2
```

^adds MeadowsBuffs profile to the buffer NPC with 2 buffs taken from buff database config file.

<br>
More Examples:

```cfg
[Stealth]
Stealth Increase   
2400   
HelmetTrollLeather   
Coins, 300   
ModifyStealth = 5    
None   
Exploration


[Speed]   
Swiftness   
1600   
TankardOdin   
Coins, 150   
ModifySpeed = 1.5   
None   
Speed

[Run]    
Running Increase   
1800   
GlowingMushroom   
Coins, 500   
ModifyStaminaRegen = 2, ModifySpeed = 2   
vfx_GodExplosion   
Exploration

[Tenacity]    
Toughness increase     
900     
HelmetDrake     
Coins, 500     
DamageReduction = 0.30    
vfx_creature_love   
Toughness

[Assault]    
Fighting increase    
600    
FlametalOre    
Coins, 500     
ModifyAttack = 2    
vfx_fir_oldlog    
Rage
```

Note: you can view all the in-game VFX by using easy spawner and searching for vfx.  
some common useful ones are vfx_HealthUpgrade, vfx_lootspawn, vfx_odin_despawn, vfx_offering, vfx_perfectblock, vfx_odin_despawn
