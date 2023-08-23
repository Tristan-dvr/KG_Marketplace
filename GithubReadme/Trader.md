
Trader NPC allows you to set items to be exchanged. Item A x number will be exchanged for Item B x number.

To start with let's make our trader profile in `Valheim\BepInEx\config\Marketplace\Configs\Traders`

Simply create ANYFILENAME.cfg and fill it with data.

![](https://i.imgur.com/cYxd3gH.png)

The data format is:

ItemA, ItemA quantity, ItemB, ItemB quantity, ItemB level (If needed)

For example i want to make a trader that will trader 100 coins for 1 swordiron level 2, and trade 1 wood for 10 Rubies:

My profile will look like that:

```
[TestTrader]
Coins, 100, SwordIron, 1, 2
Wood, 1, Ruby, 10
```

Adding that to TraderProfiles.cfg

![](https://i.imgur.com/PSpqNPL.png)

(As in any other NPC you are able to add as many profiles as you want so you can have 100 different NPCs trading different items)

Now let's assign profile to our trader NPC:

![](https://i.imgur.com/BjPrHIS.png)

On interact trader UI will open:

![](https://i.imgur.com/WMFaYl4.png)

Because i have wood and coins in my inventory i can actually exchange that. On clicking big green > (arrow) button in middle i will exchange item A on item B.

Also you can add Pets as trader items. Example: Stone, 100, Wolf, 1, 5. Will exchange 100 stone on one pet wolf level 5

Let's add another profile with pets only!

```
[PetsTrader]
Stone, 100, Wolf, 1, 5
Ruby, 25, Boar, 10, 2
```

![](https://i.imgur.com/10OELul.png)

Assigning PetsTrader profile to our NPC will give us this result:

![](https://i.imgur.com/W4YHMKr.png)

Note that wolf level 5 is 4 stars because stars starts from 0 and level starts from 1. Same for Boar

On top right you have x1, x5, x10 , x100 modifier buttons so player can change exchange rate for faster trading. Note that it applies original rate so Coins, 5, Wood, 1 on exchange rate x100 will be 500 coins to 100 wood

# Since 8.2.7 Marketplace trader got one more data format you can use

New format allows you to use up to 5 Needed Items and 5 Given Items. Also with new format left-side items may also have level (quality) requirement. Format:

```
Item, Quality, Level(IF NEEDED), Item2, Quality2, Level2(IF NEEDED),.... = Item, Quality, Level (IF NEEDED), Item2, Quality2, Level2 (IF NEEDED),....
```

Example:

```
BlackMetal, 1, AxeBlackMetal, 1, 9, Coins, 25 = AxeBlackMetal, 1, 10, Wood, 123
```

^ will give you this result:

![](https://i.imgur.com/tkb8MM5.png)

Keep in mind that you can still use old format in same profile. Example:

```
[test]
SwordIron, 1, 9, Ruby, 666 = SwordIron, 1, 10
BlackMetal, 1, AxeBlackMetal,1,9, Coins, 25 = AxeBlackMetal, 1,10, Wood, 123
Coins, 0 = AxeBlackMetal, 1, 9
Coins, 0, BlackMetal, 5
```

Result will be:

![](https://i.imgur.com/eTT5SbT.png)


