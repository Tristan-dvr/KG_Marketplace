This is the template for creating lootboxes. Navigate to `config\Marketplace\Configs\Lootboxes` and create a cfg file (name it whatever you want) for example: `Loot.cfg`

This is data layout for lootboxes if you're not using WithChance type:

<details><summary>.cfg file templates</summary>


```
[UID (no spaces, use _ if you want to use space)]
TYPE (One, All, AllWithChance, AllWithChanceShowTooltip)
Prefab1, QuantityMin1-QuantityMax2 (can be only quantitymin1 if you don't want to add more gamble), ItemLevel1, Prefab2, QuantityMin2....so on
Description
Icon (can be piece, item or image from CachedImages, default)
Open VFX (https://valheim-modding.github.io/Jotunn/data/prefabs/prefab-list.html)
```

If you're using WithChance type you would also need to specify each item chance after its level:

```
[UID (no spaces, use _ if you want to use space)]
TYPE (One, All, AllWithChance, AllWithChanceShowTooltip)
Prefab1, QuantityMin1-QuantityMax2 (can be only quantitymin1 if you don't want to add more gamble), ItemLevel1, ItemChance1, Prefab2,QuantityMin2....so on
Description
Icon (can be piece, item or image from CachedImages, default)
Open VFX (https://valheim-modding.github.io/Jotunn/data/prefabs/prefab-list.html)
```

Please note that you need to write item level even if max item level is 1 (materials and e.t.c)

```yaml
[lootbox1]
One
Wood,1-100,1,SwordIron,1,4
Test your skills
Default
fx_Potion_frostresist

[lootbox2]
All
Wood,1-100,1,SwordIron,1,4
Test your skills
chest_hildir1
none

[lootbox3]
AllWithChance
Wood,1-100,1,50,SwordIron,1,4,20
Test your skills
chest_hildir2
vfx_spawn

[lootbox4]
AllWithChanceShowToolTip
Wood,1-100,1,50,SwordIron,1,4,20
Test your skills
chest_hildir3
sfx_spawn
```

</details>

Also you can use .yml or .yaml files for lootboxes. This is layout:

<details><summary>.yml file template</summary>

```yaml
- Name: lootbox1
  Icon: Default
  OpenVFX: fx_Potion_frostresist
  Description: Test your skills
  Lootbox Type: One
  Send Webhook: false
  Item List:
  - Prefab: Wood
    Min: 1
    Max: 100
    Level: 1
    Chance: 100
  - Prefab: SwordIron
    Min: 1
    Max: 1
    Level: 4
    Chance: 100

- Name: lootbox2
  Icon: chest_hildir1
  OpenVFX: none
  Description: Test your skills
  Lootbox Type: All
  Send Webhook: false
  Item List:
  - Prefab: Wood
    Min: 1
    Max: 100
    Level: 1
    Chance: 100
  - Prefab: SwordIron
    Min: 1
    Max: 1
    Level: 4
    Chance: 100

- Name: lootbox3
  Icon: chest_hildir2
  OpenVFX: vfx_spawn
  Description: Test your skills
  Lootbox Type: AllWithChance
  Send Webhook: false
  Item List:
  - Prefab: Wood
    Min: 1
    Max: 100
    Level: 1
    Chance: 50
  - Prefab: SwordIron
    Min: 1
    Max: 1
    Level: 4
    Chance: 20
    
- Name: lootbox4
  Icon: chest_hildir3
  OpenVFX: sfx_spawn
  Description: Test your skills
  Lootbox Type: AllWithChanceShowTooltip
  Send Webhook: false
  Item List:
  - Prefab: Wood
    Min: 1
    Max: 100
    Level: 1
    Chance: 50
  - Prefab: SwordIron
    Min: 1
    Max: 1
    Level: 4
    Chance: 20
```

</details>

<p align="center"><img src="https://imgur.com/bMLxbbj.png" width="350" height="300"></a><img src="https://imgur.com/13t1jox.png" width="350" height="300"></a></p>
<p align="center"><img src="https://imgur.com/mvuxsHB.png" width="350" height="300"></a><img src="https://imgur.com/dOExsJH.png" width="350" height="300"></a></p>



### Also there is a Discord Webhook for lootboxes. If you want your lootbox to send webhook to discord add @DISCORD anywhere in lootbox UID. Example:

```
[AllItem_Lootbox_Test@DISCORD]
All
Wood,20-50,1,Stone,100-500,1,SwordIron,1,5
AllItems Test
Default
None
```

If you're using .yml file you need to add `Send Webhook: true` to your lootbox.

Result:

![](https://i.imgur.com/5IlAG8m.png)
