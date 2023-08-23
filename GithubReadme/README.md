_________________________________
![https://i.imgur.com/iWZO1dp.png](https://i.imgur.com/iWZO1dp.png)



How to spawn/change NPC:</summary>
<p> 

1) Start the game and join your server
2) Use any admin mod to enable DEBUG MODE
3) After enabling debug mode you can open your hammer and "build" NPC you want

There are two types of NPC's: Visible on map and Not Visible on map.

![](https://i.imgur.com/i4hwElW.png)

![](https://i.imgur.com/7A8rr8u.png)

![](https://i.imgur.com/IMQ7hpV.png)

The difference is only that visible on map NPC will have its Pin on map from any distance.

![](https://i.imgur.com/zlm4GR6.png)


After placing NPC in Debug Mode you can start applying few changes to it. You can open 2 menus: Main NPC UI and Fashion Menu.

![](https://i.imgur.com/K6zbBEQ.png)

Main NPC UI:

![](https://i.imgur.com/eSOXkyZ.png)

1) Top buttons - change NPC type (Marketplace, Trader, Info, Teleporter and so on)
2) Change NPC Profile - NPC profile that will hook data from your *NpcType*Profiles.cfg 
3) Override NPC Name - Change NPC name to whatever you want
4) Override NPC Model - Change NPC model to any in-game (even other mod) creature you want
5) Set Patrol Data - You can make npc walk from one spot to another, or even make a full path for it. Example: 300, 200, 305, 200. It will make your NPC walk from 300 x spot to x 305 spot (5 meters), while Z coord is always 200
6) Snap To Ground And Rotate - snaps NPC to ground and rotates it to where you look at
7) Apply - apply changes

P.S: Override NPC Model accepts ANY Character (monster) prefab (Troll, Greydwarf, Hatchling, and so on). But monsters will have their own animator.
If you want to use Overriden NPC with Player animation from fashion menu you can add @humanoid to your prefab name.
Example:
Troll@humanoid, Greydwarf@humanoid, Neck@humanoid.
That will give these creature Player animator so they will be able to use emote_wave animations and so on (crafting animations also)

Let's try it out:

Adding data:

![](https://i.imgur.com/u5L80rk.png)

Result:

![](https://i.imgur.com/kxIKSm6.png)


Now let's see Fasion Menu:
(Keep in mind that most fashion prefabs / equipment will only work on Player or Player_Female models override. Armors and such won't work on monster override models)

![](https://i.imgur.com/rqGj581.png)

1) Left Hand - left hand prefab
2) Right Hand - right hand prefab
3) Helmet Item - helmet prefab
4) Chest Item - chest prefab
5) Legs Item - legs prefab
6) Cape Item
7) Left Back Item - left back prefab
8) Right Back Item - right back prefab
9) Hair Index - hair index (1 2 3 4 5 and so on) 
10) Hair Color (#hex) - hex color for hair color, example: #ffffff
11) Skin Color (#hex) - hex color for skin color, example: #ffffff
12) Model scale - model size (works on any override model)
13) Interact animation - animation when someone interacts with NPC, example: emote_nonono
14) Greeting animation - animation when someone comes close to NPC, example: emote_thumbsup
15) Bye Animation - animation when someone leaves NPC, example: emote_wave
16) Greeting Text - text when someone comes close to NPC, example: Hello!
17) Bye Text - text when someone leaves NPC, example: Bye!
18) Crafting animation index - animation for Player and Player_Female models that turning on crafting state, there are 0 1 2 3 crafting animation states
19) Beard index - same as hair index, but for beard
20) Beard color (#hex) - hex color for beard color, example: #ffffff

Now let's write some random data:

![](https://i.imgur.com/xK0Kywc.png)

Result:

![](https://i.imgur.com/ULo443R.png)

![](https://i.imgur.com/lFzK72V.png)

Now that we learned how to spawn / edit NPC's lets try to configure some of those from serverside





## ALL OPTIONS / PROFILES / NPCs DATA ARE AUTO-RELOADED IN SERVER RUNTIME WITHOUT RESTART

## ![https://i.imgur.com/5ZHfxlo.png](https://i.imgur.com/5ZHfxlo.png)

## To install mod place MarketPlaceRevamped.dll into Client Plugins folder AND Server Plugins Folder


<h1>

[Stonedprophet](https://www.youtube.com/@therealstonedprophet) made an amazing video-guide on this mod. You can check them out here:
<span style="color:yellow;font-weight:300;font-size:35px">Stonedprophet tutorials:</span></summary>
<p> 

1) https://youtu.be/5fR_9Qygkro (part one)
2) https://youtu.be/BthPUGOeaeA (part two)
3) https://youtu.be/hUU_bPCwFeE (part three)
4) https://youtu.be/ZgoeYVpEcI4 (part four)
5) https://youtu.be/xdj2CccUYhk (part five)
6) https://youtu.be/0COuBKO3Gpg (part six - Dialogues)

</p>

</h1>




