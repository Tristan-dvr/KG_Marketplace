NPC acts as teleport-hub but all in one. Its profile/data controlled by  `Valheim\BepInEx\config\Marketplace\Configs\Teleporters`

Simply create ANYFILENAME.cfg and fill it with data.

![](https://i.imgur.com/pTjanHG.png)

![](https://i.imgur.com/MpIGCz8.png)

To Add new teleport spots you need to add them new line each with structure: Spot Name, X coord, Y coord, Z coord, Icon name

Icon can be used from client-side only folder Marketplace_CachedImages.

I recommend you to use 32x32 icons.
Also you can write ItemPrefab name instead of icon in order to use its icon as map pin
When you click Interact on Teleporter NPC with profile you will open map and it will show pins to you. After Left Mouse click on icon you will teleport to XYZ coords of spot.

![https://i.imgur.com/Hoy6Gg1.png](https://i.imgur.com/Hoy6Gg1.png)

XYZ COORDS SHOULD BE INTEGERS VALUE ONLY (5.6 <= WRONG, 5 <= good)

If you want to make teleport not instant but be more like "magic" teleport, then you can add <speed=value> parameter to teleport spot name

Example:

Spawn <speed=10>, 0,30,0

That will make teleport to spawn not instant but more magic-alike with speed of 10 meters / second
