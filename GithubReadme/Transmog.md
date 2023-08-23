Transmogrification is a system that allows your players to give their equipment any other item appearance in game.

As server admin you can configure which npc / profile will give which skins to use.

Transmog configs should be created in `Valheim\BepInEx\config\Marketplace\Configs\Transmogrifications` folder. Simply create ANYFILENAME.cfg and fill it with data.

### Format:
```
[profilename]
SkinPrefab, Price Prefab, Price Amount, Skip TypeCheck true/false, Special VFX ID (optional)
```
To add more items to profile add them on new line.
Example:

```
[testprofile]
SwordIron, Coins, 10, false
SwordIron, Coins, 20, false, 2
SwordIron, Coins, 50, false, 20
SwordIronFire, Ruby, 10, false
SwordIronFire, Ruby, 20, false, 2
SwordIronFire, Ruby, 50, false, 20
```

^ This profile will give NPC 6 items to use as skins, usual IronSword, IronSword with VFX ID 2, IronSword with VFX ID 20, FireSword, FireSword with VFX ID 2, FireSword with VFX ID 20.

Note that if VFX id is 21 then players will be able to chooce vfx manually on item.

1) Assigning profile to NPC:
   ![](https://imgur.com/JwHAUpQ.png)
2) Open UI by interacting with NPC to see result:
   ![](https://imgur.com/Fq4kjch.png)
3) On Left side choose the item from your inventory you want to transmogrify, you can use HEX color to change the color of the item and also preview what it looks like by clicking the button in the top right, in this example will transmogrify the Iron Chest to look like Bronze Chest with color changes and vfx.
   ![](https://imgur.com/RZoeGwD.png)
4) We made the Iron Chest look like the Bronze Chest but with Red Color and using vfx "1" [mpasn_transmog_eff1: Azure Ashes], click the button in the bottom right to apply the transmog
   ![](https://imgur.com/uE9erX1.png)

IF YOU SET SKIP TYPECHECK TO TRUE, YOU WILL BE ABLE TO USE ANY ITEM AS SKIN, EVEN IF IT IS NOT EQUIPMENT. THIS WILL CAUSE SOME ISSUES WITH SOME ITEMS, SO USE IT ONLY IF YOU KNOW WHAT YOU ARE DOING.

Also skip typecheck will allow you to set 2-handed weapon as skin for 1-handed weapon and vice versa. Or it will allow you to use Trophy as skin:

![](https://i.imgur.com/T8QmpJm.png)

![](https://i.imgur.com/Sd4Xsdo.png)

As you noticed there are 20 VFX's marketplace can give you. To use them after typecheck skip true/false write VFX ID you want to use.

Effect names by default:
```
mpasn_transmog_eff1: Azure Ashes
mpasn_transmog_eff2: Burning Low
mpasn_transmog_eff3: Cyan Wrap
mpasn_transmog_eff4: Ice Age
mpasn_transmog_eff5: Angel Touch
mpasn_transmog_eff6: Purple Flame
mpasn_transmog_eff7: Burning High
mpasn_transmog_eff8: Turbulence
mpasn_transmog_eff9: Radiation
mpasn_transmog_eff10: Loki's Anger
mpasn_transmog_eff11: Phantom
mpasn_transmog_eff12: Golden Age
mpasn_transmog_eff13: Ice Menace
mpasn_transmog_eff14: Cyan Breathe
mpasn_transmog_eff15: Lightning Strike
mpasn_transmog_eff16: Tranquility
mpasn_transmog_eff17: Magic Arise
mpasn_transmog_eff18: Water
mpasn_transmog_eff19: Energy Flow
mpasn_transmog_eff20: Lightning Menace
```

Lets try to affect out Cheat Sword with transmog:
1) Choose item
   ![](https://i.imgur.com/SDJsDOh.png)
2) Choose skin
   ![](https://i.imgur.com/DSkdimb.png)

(press little square icon in right bottom)

4) Done:
   ![](https://i.imgur.com/STsZbGs.png)
5) Out item looks like that now:
   ![](https://i.imgur.com/T4Ss9IB.png)
6) When you equip item you will see that its appearance changed, as well now it has VFX. All weapon stats are same, as well as animation of attacks and so on:
   ![](https://i.imgur.com/apOXM30.png)
7) If you want to remove transmog from item - choose an item in UI and press "Clear" button
