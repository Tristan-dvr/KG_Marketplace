The gambler NPC requires items to activate, typically coins. The Gambler offers a list of items and a set amount of which the player can win. So for example a gambler can have ten items in the list, allow two of them to be won, and set a price to roll a chance at winning.

Gambler configs located in  `Valheim\BepInEx\config\Marketplace\Configs\Gamblers` 

Simply create ANYFILENAME.cfg and fill it with data.

<b>To add a new profile</b> you need to write [ProfileName=ItemsPerRollCount] , and then on a new line add an item list for it (<u>max 10 items</u>, first item is ITEM NEEDED TO ROLL): RollItemPrefab, RollItemCount, Item1, Item1count, Item2, Item2Count, Item3, Item3Count.....     

Item counts can be variable as seen below.

Example:

```cfg
[test=2]   
Coins, 10, SwordIron, 1, Tar, 30-50, Wood, 1-100
```

^ This will add a profile to gambler with 2 items per roll count (he can take 2 items out of 3 in the list)   
Player will need 10 coins per roll, Items are: Sword iron (one), Tar (from 30 to 50 randomly), Wood (from 1 to 100) randomly

<br> 
More Examples:  

```cfg
[gmeadows=3]<br>
Coins, 250, SpearBronze, 1, Tar, 3-5, Wood, 25, ArrowFire, 20-30, FineWood, 20, Stone, 25, ArrowWood, 20-30, Feathers, 15, MeadTasty, 3-5, TurnipStew, 2-3, ArmorTrollLeatherChest, 1, QueensJam, 3-5, FishRaw, 10, ArrowFlint, 20-30, ArmorTrollLeatherLegs, 1, Coal, 25

[gswamp=3]<br>
Coins, 500, AtgeirBronze, 1, ArrowFire, 30-50, ArrowBronze, 20-30, FineWood, 40, Stone, 50, ArrowIron, 10-20, Feathers, 20, MeadTasty, 3-5, TurnipStew, 3-5, ArmorRootChest, 1, OdinsDelight, 2-3, TeriyakiSalmon, 3-5, BoneArrow, 20-30, ArmorRootLegs, 1, Coal, 35

[gmountain=3]<br>
Coins, 1000, AtgeirIron, 1, Tar, 30-50, ArrowPoison, 50, FineWood, 60, Stone, 75, ArrowObsidian, 50, Feathers, 25, MeadTasty, 3-5, TurnipStew, 5-10, ArmorFenringChest, 1, OdinsDelight, 3-5, HoneyTeriyakiSalmonWrap, 3-5, BoneArrow, 30-50, ArmorFenringLegs, 1, Coal, 50
```