_________________________________
![https://i.imgur.com/iWZO1dp.png](https://i.imgur.com/iWZO1dp.png)



How to spawn/change NPC:</summary>
<p> 

1) Start the game and join your world/server, if you're playing singleplayer you need to enable marketplace to work locally then restart game for Marketplace folder to be created.
2) Use any admin mod to enable DEBUG MODE
3) After enabling debug mode you can open your hammer and "build" NPC you want

---
There are two types of NPC's: Visible on map and Not Visible on map.
- The difference is only that visible on map NPC will have its Pin on map from any distance. If you're using Debug Mode you can also modify NPC from map window by using left click / right click on icon <img src="https://imgur.com/LjRLERw.png" width="50" height="50">
<div class="align-center">
  <img src="https://imgur.com/ukyJHtN.png" width="500" height="500">
  <img src="https://imgur.com/fbwWHuC.png" width="500" height="500">
</div>

---
After placing NPC in Debug Mode you can start applying few changes to it. You can open 2 menus: Main NPC UI and Fashion Menu.
<div class="align-center">
  <img src="https://imgur.com/jPPhADl.png" width="500" height="500">
  <img src="https://imgur.com/gyJCzsG.png" width="500" height="500">
</div>

## Main NPC UI:

![](https://imgur.com/FYmh7jk.png)

1) Top buttons - Change NPC type (Marketplace, Trader, Info, Teleporter and so on)
2) Change NPC Profile - NPC profile that will hook data from your any of the cfg files you create for that specific NPC Type which are created in the subfolders `config\Marketplace\Configs`, it always revert to lowercase 
3) Override NPC Name - Change NPC name to whatever you want
4) Override NPC Model - Change NPC model to any in-game (even other mod) creature you want, look below for example.
5) Set Patrol Data - You can make npc walk from one spot to another, or even make a full path for it. Example: 300, 200, 305, 200. It will make your NPC walk from 300 x spot to x 305 spot (5 meters), while Z coord is always 200
6) Attach NPC Dialogue - Dialogue profile name to use data from your cfg file you create in the `Dialogue` folder located in `config\Marketplace\Configs\Dialogues`
7) Snap To Ground And Rotate - snaps NPC to ground and rotates it to where you look at
8) Apply - apply changes

P.S: Override NPC Model accepts ANY Character (monster) prefab (Troll, Greydwarf, Hatchling, and so on). But monsters will have their own animator.
If you want to use Overriden NPC with Player animation from fashion menu you can add @humanoid to your prefab name.
Example:
Troll@humanoid, Greydwarf@humanoid, Neck@humanoid.
That will give these creature Player animator so they will be able to use emote_wave animations and so on (crafting animations also)

Let's try it out:

### Adding data:

![](https://imgur.com/NaQXjlo.png)

### Result:

![](https://imgur.com/g68UYHB.png)

Now let's see Fasion Menu:
(Keep in mind that most fashion prefabs / equipment will only work on Player or Player_Female models override. Armors and such won't work on monster override models)

![](https://imgur.com/9QHD0cX.png)

1) Left Item - left hand prefab
2) Right Item- right hand prefab
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
21) Interact Sound
22) Text Size
23) Text Height
24) Periodic Animation
25) Periodic Animation Time
26) Periodic Sound
27) Periodic Sound Time

Now let's write some random data:

![](https://imgur.com/VgaH8AF.png)

### Result:

<div class="align-center">
  <img src="https://imgur.com/PPnPqka.png" width="500" height="500">
  <img src="https://imgur.com/flcIa1c.png" width="500" height="500">
</div>

# Saving NPCs

Now that we have created our NPC, we will want to save them so they show up in the hammer menu to place the same exact NPC anywhere else if you wish to do so.

1) Press C+E on the NPC to "Save NPC", These will be saved client side
2) In console type in `reloadnpcs` to reload the NPCs, open up your hammer and go to `Marketplace` tab and your newly created NPC will now appear and can place the NPC anywhere by keeping the same exact modifications you made for it.
3) Go to `config\Marketplace_SavedNPCs` to find the NPC.yml you created, you change the name of the file to whatever you want, inside the yml file has all the information for that NPC and you can modify it if you wish to change the NPC to something else or its fashion.

![](https://imgur.com/zFUDvsS.png)

<h1>

Lagoshi made an amazing mod that has pretty much everything setup to help understand how the structure of some of the features work
- https://valheim.thunderstore.io/package/JewelHeim/Marketplace_Configs/

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