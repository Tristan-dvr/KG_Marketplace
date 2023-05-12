<p><img src="https://i.imgur.com/CkSehPu.png" alt="https://i.imgur.com/CkSehPu.png"></p>
<h1>
<p><a href="https://github.com/war3i4i/Marketplace">GITHUB</a></p>
</h1>
<p><img src="https://i.imgur.com/dBf99Od.png" alt="https://i.imgur.com/dBf99Od.png"></p>
<p>Like my mods? Support me:
Paypal: <a href="mailto:war3spells@gmail.com">war3spells@gmail.com</a></p>
<h2>MOD ONLY WORKS IF YOU USE IT ON DEDICATED SERVER. DON’T TRY TO USE IT IN SINGLEPLAYER / CLIENT HOST</h2>
<h2>Mod adds different NPCs and Unique mechanics to server so admins can configure them from serverside with no need to restart server for applying settings:</h2>
<details>
  <summary><b><span style="color:aqua;font-weight:200;font-size:20px">
    Patchnotes
</span></b></summary>
<table>
<thead>
<tr>
<th>Version</th>
<th>Changes</th>
</tr>
</thead>
<tbody>
<tr>
<td>7.7.1</td>
<td>1) Now skill level as quest REWARD will not give skill levels if skill level is 0 (professions)<br/>2) Now all configs (including discord config, territory config and MAIN config (that also got changed) ) updating in server runtime without restart<br/>3) Changed discord connector config so you can write your own messages using {0] {1} {2} string formatting<br/>4) Fixed some patrol errors<br/>5) NPC that visible on map will be displayed as quest complete icon if its Talk quest target<br/>6) Fixed bug where every player would be an owner of any admin zone<br/>New territory flags<br/>7) NPC’s now can move if you set their patrol data (example: X0, Y0, X1, Y1, X2, Y2 and so on)<br/>8) Added new NPC name <icon> tag that allows you to add icon to NPC (exampe: <icon>Hammer</icon>), icon may be in-game monster, item or teleporter icon<br/>9) Added caching of teleporter icons<br/>10) Added /zones command to show zones in world<br/>11) Added F8 client GUI to create/remove/edit zones<br/>12) Added new NPC that’s visible on map<br/>13) Added caching of quest descriptions<br/>14) Quests now may have multiple restrictions</td>
</tr>
<tr>
<td>7.7.2-7.7.6</td>
<td>1) Small bugfixes<br/>2) Fixed npc patrol dropping underground because of no collision check<br/>3) Added isModed = true flag for valheim<br/>4) New territory flags were added: CustomPaint, LimitZoneHeight</td>
</tr>
<tr>
<td>7.7.7</td>
<td>1) Max accepted quest count now controlled by option in serverside<br/>2) Updated accepted quests UI. Added scrollview so you can see a lot of quests now. Also accepted quests UI now expandable in height if you drag its bottom border<br/>3) Fixed visible on map npc icon giving error<br/>4) Fixed patrol npc skyrocket in sky</td>
</tr>
<tr>
<td>7.7.8</td>
<td>1) Fixed Jewelcrafting incompatibility<br/>2) Added new API methods for my server control bot</td>
</tr>
<tr>
<td>7.7.9-7.8.2</td>
<td>1) Added new mechanic: Battlepass. (Still it test so no guides atm)<br/>2) Fixed marketplace default NPC models being able to go through (model collider issues)<br/>3) Added marketplace comptibility with ANY EIDF (Extended Item Data Framework) mod, such as my Transmogrification, Jewelcrafting, EpicLoot and so on<br/>4) Items in marketplace now have tooltip in right side with item stats and additional mod effects<br/>5) Added new quest Requirement: HasItem. Example: HasItem: Coins, 500<br/>6) Added new territory flags: LimitZoneHeight, CustomPaint<br/>7) Some additional optimizations<br/>8) Quest system improvements in terms of serverside crashes</td>
</tr>
<tr>
<td>7.8.3</td>
<td>1) Changed marketplace fonts and optimized UI<br/>2) Battlepass fixes<br/>3) Webhooks now having <color> richtext removed</td>
</tr>
<tr>
<td>7.8.4</td>
<td>1) Added german + portugese languages support<br/>2) HOTFIX for bug that doesn’t allow mod to work</td>
</tr>
<tr>
<td>7.8.5</td>
<td>1) Fixed cooking skill bug<br/>2) Fixed marketplace UI sorting by itemname/price/amount/seller text disappear on click</td>
</tr>
<tr>
<td>7.8.6</td>
<td>1) Added Korean language support<br/>2) Fixed possible EIDF item dupe</td>
</tr>
<tr>
<td>7.8.7</td>
<td>1) Now collect and craft quests may also have target level<br/>2) Fixed JC api<br/>3) Added new trader UI buttons: x1, x5, x10, x100</td>
</tr>
<tr>
<td>7.8.8</td>
<td>Fixed Previous Version</td>
</tr>
<tr>
<td>7.8.9</td>
<td>Fixed kill quest sometimes giving double reward</td>
</tr>
<tr>
<td>7.9.0</td>
<td>Fixed problem where item with 5 sockets were shown as 4 sockets max</td>
</tr>
<tr>
<td>7.9.1-7.9.2</td>
<td>1) Bugfixes<br/>Increase max marketplace pric to 10 mil</td>
</tr>
<tr>
<td>7.9.3</td>
<td>Fixed new Jewelcrafting mod version problem with marketplace display</td>
</tr>
<tr>
<td>7.9.4</td>
<td>NPC’s now may have interact sound<br/>New Territory Flag: SnowMask (makes ground with snow only)<br/>New Territory Flag: NoItemLoss. On death inventory kept with player<br/>Bugfixes<br/>Added <speed> tag to Teleporter spot name (read Teleporter guides)<br/>Moved all system Guides to separated github page because of char limit</td>
</tr>
<tr>
<td>7.9.5-7.9.6</td>
<td>Fixed an issue with disconnecting players after few hours</td>
</tr>
<tr>
<td>7.9.7</td>
<td>Added 3 new options in NPC Fasion Menu: Text Font, Text Size, Test Height Offset</td>
</tr>
<tr>
<td>7.9.8</td>
<td>Now Admins using Debug Mod can remove slots (even Expired one’s) from marketplace by clicking “X” button in end of each slot</td>
</tr>
<tr>
<td>7.9.9</td>
<td>Fixed small marketplace bug on trying to sell items<br/>Now “NPC Model Override” can be literally ANYTHING in game: Piece objects (structures), Itemdrops, trees and so on<br/>Please use new model override feature on your own risk since its not being tested yet and may cause a lot of bugs. DO NOT USE VFX’s as model override or model will be gone. If you somehow failed NPC model override then write it chat /npc remove . That will cause all near NPC’s (5 meter range) be removed</td>
</tr>
<tr>
<td>7.9.10</td>
<td>Fixed KeyManager problem for server using same IP</td>
</tr>
<tr>
<td>8.0.0</td>
<td>1) Bugfixes<br/>2) Added new Premium System: Distanced UI that can use NPC profiles without interacting with NPCs. To use go to MarketplaceKG/PremiumSystem/ folder to  edit .cfg file. Hotkey to open UI is L. Alt + ~<br/>3) Added new NPC UI : Save/Load. Opens with C + Interact. Allows you to save NPC appearance and then load it back on another NPC. To save ALL NPCs in your location write /npc save in chat<br/>4) Replaced old localization on LocalizationManager. Now you can add your own localization. For that download file: <a href="https://pastebin.com/7z08xMQq">https://pastebin.com/7z08xMQq</a> . Place it into Valheim/BepInEx/config/ folder and name it MarketplaceAndServerNPCs.YOURLANGUAGE.yml . Then you can translate lines to make your own language localization</td>
</tr>
<tr>
<td>8.0.2</td>
<td>Added few log lines for PremiumSystem</td>
</tr>
<tr>
<td>8.1.0</td>
<td><span style="color:red;">BEFORE INSTALLING 8.1.0 VERSION MAKE SURE TO REMOVE ALL ITEMS FROM MARKETPLACE SINCE AFTER UPDATE IT WILL REMOVE ITEMS OWNERSHIP FROM ALL USERS. ALSO DO THE SAME FOR ALL PLAYER CREATED TERRITORIES<br/></span>New NPC (System) Added: Transmogrification (Paid feature only)<br/>New System added: Quest Events<br/>New quest reward added: Skill_EXP<br/>New quest restriction added: NotFinished<br/>Bugfixes<br/>Now NPC Sounds are mp3 files instead of wav<br/>Now Territories with at least one color less than 0 wont be displayed on map<br/>Added tooltips on hover on any quest reward or trader item<br/>If you will write [questID=autocomplete] then quest will be considered finished without completing it in NPC UI, it will be completed immediately when your quest target is done<br/></td>
</tr>
<tr>
<td>8.1.1</td>
<td>Returned Quest Journal (a little changed)<br/>Fixed NPC sound reverb problem<br/>Fixed player getting skill experience while attacking NPC</td>
</tr>
<tr>
<td>8.1.2</td>
<td>Fixed critical bug that didn’t allow players to join server</td>
</tr>
<tr>
<td>8.2.0</td>
<td>Now mod compatible with mistlands update<br/>Updated NPC + NPC Fashion UI’s<br/>Now Marketplace also saves Crafter Name + Crafter ID<br/>Updated transmog to use ItemDataManager. After update all transmogrified items will be nullified. But because of using ItemDataManager now transmog wont disappear when you upgrade an item + will have much less bugs (armor stand ad so on)<br/>New Territory flags added: NoMist, InfiniteEitr, InfiniteStamina<br/>Small Localization update</td>
</tr>
<tr>
<td>8.2.1</td>
<td>Fixed quest autocomplete tag problem on most quest types. Now it properly works on all Kill, Collect, Craft, Build type quests</td>
</tr>
<tr>
<td>8.2.3</td>
<td>Fixed Jewelcrafting compatibility. <br/>Added new VFX id: 21 to Transmogrification that allows people to chooce any effect manually. <br/>Fixed player territories map showup issue</td>
</tr>
<tr>
<td>8.2.4</td>
<td>Added MagicHeim API (Quest Reward Add MagicHeim EXP, Quest Restriction MagicHeim Level)<br/>Fixed compatibility issue with Marketplace Territories and Jere’s ExpandWorld</td>
</tr>
<tr>
<td>8.2.6</td>
<td>Updated to latest Valheim live version<br/>Added new &lt;image=link&gt; tag for quest name to show preview image<br/>Added PutAll button to Banker<br/>Added Periodic animation to NPC Fashion UI<br/>Fixed Premium UI syncing<br/>Added new territory flag: NoCreatureDrops</td>
</tr>
<tr>
<td>8.2.7</td>
<td>Added new trader format, now Trader may have up to 5 items to exchange in left and right side, also left side items may now also have level required<br/>Quests now may have multiple targets per one quest as rewards and requirements (same format with adding)<br/>Reworked Marketplace UI visuals<br/>Fixed a bug where marketplace prevented items from being able to change rotation / roll<br/>Some code optimizations<br/>Now if you press RIGHT mouse button on “Receive Income” button in Marketplace then income will be added directly to your banker</td>
</tr>
<tr>
<td>8.2.8</td>
<td>All data in DO NOT TOUCH folder now decrypted. Keep in mind that you can’t change that in runtime and if you edit .json file then do it on your own risk<br/>Changed NPC Save / Load UI, changed Marketplace UI, changed Premium UI<br/>Added IsVIP restriction for quests (quest will be shown only for VIP’s)<br/>Fixed trader NeedToKnowMaterial items appear if player doesn’t know materials<br/>Now you can buy particular amount of items from stack in Marketplace<br/>Updated KeyManager<br/>Items in Marketplace cannot be Expired anymore</td>
</tr>
<tr>
<td>8.3.0</td>
<td>Updated for new Valheim version<br/>Bugfixes<br/>Added  Marketplace_GOBLIN, Marketplace_SKELETON, Marketplace_QUESTBOARD, Marketplace_TELEPORTER, Marketplace_DEFAULTNPC as separated models that you can use to override NPC model</td>
</tr>
<tr>
<td>8.3.2</td>
<td>Quest descriptions now may have \n as new line<br/>Territory minimap text fix<br/>Fixed NPC save/load UI problems<br/>Fixed Teleporter map names showup</td>
</tr>
<tr>
<td>8.3.3</td>
<td>Added Groups API for Kill type quests</td>
</tr>
<tr>
<td>8.4.0</td>
<td>Player Territories removed. Please do not install this version until you replace Player Territories module on something else (Azumatt wards / e.t.c) (TerritoryDatabase is same and working, just not the players one)<br/>Added KGchat as part of marketplace. Its enabled by default but you can turn it off in Main config on serverside. You can replace KGchat emojis in BepInEx/Config/MarketplaceEmojis. You will find spritesheet_original.png there, change pics on what you need and rename it to spritesheet.png<br/>Added 2 new fields to fashion UI: Periodic Sound + Periodic Sound Time<br/>Added new quest event: NpcText<br/>Optimized mod by rewriting it almost from scratch. Now mod is open-source, check: <a href="https://github.com/war3i4i/Marketplace">https://github.com/war3i4i/Marketplace</a> for code<br/>Added API for territories so other mods may use it (check github)<br/>NPC’s now won’t show up in hammer menu if Debug Mode is turned off<br/><br/>Transmogrification system access has changed (now transmogrification is a separated DLL). If you bought Transmog access before this patch please contact me in discord KG#7777 so i can send you mod to enable Transmog</td>
</tr>
<tr>
<td>8.5.0</td>
<td>New system added: NPC Dialogue (guide soon)<br/>New system added: Item Mocking (guide soon)<br/>Fixed banker multiplier bug<br/>Fixed KGchat text overflow</td>
</tr>
<tr>
<td>8.6.0</td>
<td>New system added: Mailbox<br/>Finished NPC Dialogues system<br/>Bugfixes<br/>Fixed Banker interest not working<br/>Now Marketplace can use SOME of its features locally on client (to enable set config option to true on clientside)<br/>New Quest Restriction - Time: value, allows quest to be time limited<br/>Added NPC font support for chinese symbols and other languages special symbols<br/></td>
</tr>
</tbody>
</table>
</details>
<span style="color: bisque;">
Now you can add your own localization. For that download file: <a href="MarketplaceAndServerNPCs.English.yml" download>Translation</a>.<br>Place it into Valheim/BepInEx/config/ folder and name it MarketplaceAndServerNPCs.YOURLANGUAGE.yml . Then you can translate lines to make your own language localization
</span>
<h1>
<p><a href="https://www.youtube.com/@therealstonedprophet">Stonedprophet</a> made an amazing video-guide on this mod. You can check them out here:</p>
<details><summary><span style="color:yellow;font-weight:300;font-size:35px">Stonedprophet tutorials:</span></summary>
<p> 
<ol>
<li><a href="https://youtu.be/5fR_9Qygkro">https://youtu.be/5fR_9Qygkro</a> (part one)</li>
<li><a href="https://youtu.be/BthPUGOeaeA">https://youtu.be/BthPUGOeaeA</a> (part two)</li>
<li><a href="https://youtu.be/hUU_bPCwFeE">https://youtu.be/hUU_bPCwFeE</a> (part three)</li>
<li><a href="https://youtu.be/ZgoeYVpEcI4">https://youtu.be/ZgoeYVpEcI4</a> (part four)</li>
<li><a href="https://youtu.be/xdj2CccUYhk">https://youtu.be/xdj2CccUYhk</a> (part five)</li>
</ol>
</p>
</details>
</h1>
<hr>
<p><img src="https://i.imgur.com/iWZO1dp.png" alt="https://i.imgur.com/iWZO1dp.png"></p>
<details><summary>Installation and main configs:</summary>
<p> 
<ol>
<li>Ship plugin to all clients and on your dedicated server</li>
<li>After server restart, new folder in BepInEx/config will be created: MarketplaceKG</li>
</ol>
<p><img src="https://i.imgur.com/EnHUG1T.png" alt=""></p>
<p>Each file / folder description:</p>
<ol>
<li>Battlepass folder - contains battlepass configs for Free / Premium rewards and main battlepass config (battlepass name, exp step)</li>
<li>Discord Webhook folder - allows you to configure webhooks for Marketplace notifications (Quest completed, Marketplace item placed, Gambler won)</li>
<li>DO NOT TOUCH - this folder only contains encrypted marketplace related data (players messages, players income, marketplace slots and so on). DO NOT TOUCH this folder since you will lose all your marketplace data if you do so. There are none files you can / need to edit</li>
<li>MapPinsIcons - folder where you can place small-weight icons for Teleporter NPC. But there is also MarketplaceCachedTeleporterIcons folder in clientside which i recommend you to use, instead of adding icons on serverside</li>
<li>PlayerTerritories - folder with json files and .cfg for Player-made territories (Admin territories are inside TerritoryDatabase.cfg)</li>
<li>BankerProfiles.cfg - file for configuring banker NPC’s</li>
<li>BufferDATABASE.cfg - file that contains all your created buffs for Buffer NPC</li>
<li>BufferProfiles.cfg - file for configuring Buffer NPC (you can choose which NPC profile has WHICH buffs from database)</li>
<li>GamblerProfiles.cfg - file for configuring Gambler NPC</li>
<li>LOGS.log - few logs for some marketplace actions (item deposit / withdraw to banker, marketplace item placed, etc)</li>
<li>MarketPlace.cfg - main config that contains small config values for various mechanics</li>
<li>QuestDATABASE.cfg - file where you have all your written quests</li>
<li>QuestProfiles.cfg - file for configuring Quest NPC (you can choose which NPC profile has WHICH quests from database)</li>
<li>ServerInfoProfiles.cfg - file for configuring ServerInfo NPC</li>
<li>TeleportHubProfiles.cfg - file for configuring Teleporter NPC</li>
<li>TerritoryDatabase.cfg - file for configuring territories</li>
<li>TraderProfiles.cfg - file for configuring Trader NPC</li>
<li>TransmogrificationProfiles.cfg - file for configuring Transmogrification NPC</li>
</ol>
</p>
</details>
<details><summary>How to spawn/change NPC:</summary>
<p> 
<ol>
<li>Start the game and join your server</li>
<li>Use any admin mod to enable DEBUG MODE</li>
<li>After enabling debug mode you can open your hammer and “build” NPC you want</li>
</ol>
<p>There are two types of NPC’s: Visible on map and Not Visible on map.</p>
<p><img src="https://i.imgur.com/i4hwElW.png" alt=""></p>
<p><img src="https://i.imgur.com/7A8rr8u.png" alt=""></p>
<p><img src="https://i.imgur.com/IMQ7hpV.png" alt=""></p>
<p>The difference is only that visible on map NPC will have its Pin on map from any distance.</p>
<p><img src="https://i.imgur.com/zlm4GR6.png" alt=""></p>
<p>After placing NPC in Debug Mode you can start applying few changes to it. You can open 2 menus: Main NPC UI and Fashion Menu.</p>
<p><img src="https://i.imgur.com/K6zbBEQ.png" alt=""></p>
<p>Main NPC UI:</p>
<p><img src="https://i.imgur.com/eSOXkyZ.png" alt=""></p>
<ol>
<li>Top buttons - change NPC type (Marketplace, Trader, Info, Teleporter and so on)</li>
<li>Change NPC Profile - NPC profile that will hook data from your <em>NpcType</em>Profiles.cfg</li>
<li>Override NPC Name - Change NPC name to whatever you want</li>
<li>Override NPC Model - Change NPC model to any in-game (even other mod) creature you want</li>
<li>Set Patrol Data - You can make npc walk from one spot to another, or even make a full path for it. Example: 300, 200, 305, 200. It will make your NPC walk from 300 x spot to x 305 spot (5 meters), while Z coord is always 200</li>
<li>Snap To Ground And Rotate - snaps NPC to ground and rotates it to where you look at</li>
<li>Apply - apply changes</li>
</ol>
<p>P.S: Override NPC Model accepts ANY Character (monster) prefab (Troll, Greydwarf, Hatchling, and so on). But monsters will have their own animator.
If you want to use Overriden NPC with Player animation from fashion menu you can add @humanoid to your prefab name.
Example:
Troll@humanoid, Greydwarf@humanoid, Neck@humanoid.
That will give these creature Player animator so they will be able to use emote_wave animations and so on (crafting animations also)</p>
<p>Let’s try it out:</p>
<p>Adding data:</p>
<p><img src="https://i.imgur.com/u5L80rk.png" alt=""></p>
<p>Result:</p>
<p><img src="https://i.imgur.com/kxIKSm6.png" alt=""></p>
<p>Now let’s see Fasion Menu:
(Keep in mind that most fashion prefabs / equipment will only work on Player or Player_Female models override. Armors and such won’t work on monster override models)</p>
<p><img src="https://i.imgur.com/rqGj581.png" alt=""></p>
<ol>
<li>Left Hand - left hand prefab</li>
<li>Right Hand - right hand prefab</li>
<li>Helmet Item - helmet prefab</li>
<li>Chest Item - chest prefab</li>
<li>Legs Item - legs prefab</li>
<li>Cape Item</li>
<li>Left Back Item - left back prefab</li>
<li>Right Back Item - right back prefab</li>
<li>Hair Index - hair index (1 2 3 4 5 and so on)</li>
<li>Hair Color (#hex) - hex color for hair color, example: #ffffff</li>
<li>Skin Color (#hex) - hex color for skin color, example: #ffffff</li>
<li>Model scale - model size (works on any override model)</li>
<li>Interact animation - animation when someone interacts with NPC, example: emote_nonono</li>
<li>Greeting animation - animation when someone comes close to NPC, example: emote_thumbsup</li>
<li>Bye Animation - animation when someone leaves NPC, example: emote_wave</li>
<li>Greeting Text - text when someone comes close to NPC, example: Hello!</li>
<li>Bye Text - text when someone leaves NPC, example: Bye!</li>
<li>Crafting animation index - animation for Player and Player_Female models that turning on crafting state, there are 0 1 2 3 crafting animation states</li>
<li>Beard index - same as hair index, but for beard</li>
<li>Beard color (#hex) - hex color for beard color, example: #ffffff</li>
</ol>
<p>Now let’s write some random data:</p>
<p><img src="https://i.imgur.com/xK0Kywc.png" alt=""></p>
<p>Result:</p>
<p><img src="https://i.imgur.com/ULo443R.png" alt=""></p>
<p><img src="https://i.imgur.com/lFzK72V.png" alt=""></p>
<p>Now that we learned how to spawn / edit NPC’s lets try to configure some of those from serverside</p>
</p>
</details>
<details><summary>MarketPlace.cfg (misc configs):</summary>
<p> 
<p><img src="https://i.imgur.com/48FkIqM.png" alt=""></p>
<ol>
<li>ItemMarketLimit - limit of slots a player can post in Marketplace</li>
<li>BlockedPlayers - SteamID list of players that can’t post items in Marketplace</li>
<li>VIPplayersList - SteamID list of players that are VIPs (less taxes)</li>
<li>MarketTaxes - taxes for Marketplace items (non-VIP users)</li>
<li>VIPplayerTaxes - taxes for Marketplace items (VIP users)</li>
<li>CanTeleportWithOre - define if players can teleport with ore in Teleporter NPC</li>
<li>MarketSellBlockedPrefabs - prefabs that players cannot sell on marketplace</li>
<li>FeedbackWebhookLink - Feedback NPC webhook link</li>
<li>ServerCurrency - currency to use in Marketplace. If you have your own prefab - analogue of Coins you can write it here</li>
<li>BankerIncomeTime - how often (HOURS) banker will give players income</li>
<li>BankerIncomeMultiplier - each #BankerIncomeTime (hours) will add income with multiplier. Example: if player has 100 coins in bank and multiplier is 0.1, then each BankerIncomeTime he will have 100 + 100 * 0.1 (110). Then 110 + 110 * 0.1 = 221. And so on</li>
<li>BankerVIPIncomeMultiplier - same as upper, but for VIP players</li>
<li>MarketSlotExpirationTime - how many hours should pass, so that player marketplace slot will expire (won’t be shown in marketplace list anymore)</li>
<li>GamblerEnableWinNotifications - enable global chat win notifications when someone wins something in gambler NPC</li>
<li>AllowMultipleQuestsScore - if set to true, then if player has 2 quests with same target, upon adding quest score it will be added to BOTH quests instead of just one</li>
<li>MaxAcceptedQuests - maximum number of quests that player can have accepted at once</li>
<li>BattlepassVIPlist - SteamID list of players that are VIPs in Battlepass</li>
<li>Enable KG Chat - enable / disable KG chat</li>
</ol>
</p>
</details>
<details><summary>Trader</summary>
<p> 
Trader NPC allows you to set items to be exchanged. Item A x number will be exchanged for Item B x number.
<p>To start with let’s make our trader profile in TraderProfiles.cfg:</p>
<p><img src="https://i.imgur.com/cYxd3gH.png" alt=""></p>
<p>The data format is:</p>
<p>ItemA, ItemA quantity, ItemB, ItemB quantity, ItemB level (If needed)</p>
<p>For example i want to make a trader that will trader 100 coins for 1 swordiron level 2, and trade 1 wood for 10 Rubies:</p>
<p>My profile will look like that:</p>
<pre><code>[TestTrader]
Coins, 100, SwordIron, 1, 2
Wood, 1, Ruby, 10
</code></pre>
<p>Adding that to TraderProfiles.cfg</p>
<p><img src="https://i.imgur.com/PSpqNPL.png" alt=""></p>
<p>(As in any other NPC you are able to add as many profiles as you want so you can have 100 different NPCs trading different items)</p>
<p>Now let’s assign profile to our trader NPC:</p>
<p><img src="https://i.imgur.com/BjPrHIS.png" alt=""></p>
<p>On interact trader UI will open:</p>
<p><img src="https://i.imgur.com/WMFaYl4.png" alt=""></p>
<p>Because i have wood and coins in my inventory i can actually exchange that. On clicking big green &gt; (arrow) button in middle i will exchange item A on item B.</p>
<p>Also you can add Pets as trader items. Example: Stone, 100, Wolf, 1, 5. Will exchange 100 stone on one pet wolf level 5</p>
<p>Let’s add another profile with pets only!</p>
<pre><code>[PetsTrader]
Stone, 100, Wolf, 1, 5
Ruby, 25, Boar, 10, 2
</code></pre>
<p><img src="https://i.imgur.com/10OELul.png" alt=""></p>
<p>Assigning PetsTrader profile to our NPC will give us this result:</p>
<p><img src="https://i.imgur.com/W4YHMKr.png" alt=""></p>
<p>Note that wolf level 5 is 4 stars because stars starts from 0 and level starts from 1. Same for Boar</p>
<p>On top right you have x1, x5, x10 , x100 modifier buttons so player can change exchange rate for faster trading. Note that it applies original rate so Coins, 5, Wood, 1 on exchange rate x100 will be 500 coins to 100 wood</p>
<h1>Since 8.2.7 Marketplace trader got one more data format you can use</h1>
<p>New format allows you to use up to 5 Needed Items and 5 Given Items. Also with new format left-side items may also have level (quality) requirement. Format:</p>
<pre><code>Item, Quality, Level(IF NEEDED), Item2, Quality2, Level2(IF NEEDED),.... = Item, Quality, Level (IF NEEDED), Item2, Quality2, Level2 (IF NEEDED),....
</code></pre>
<p>Example:</p>
<pre><code>BlackMetal, 1, AxeBlackMetal, 1, 9, Coins, 25 = AxeBlackMetal, 1, 10, Wood, 123
</code></pre>
<p>^ will give you this result:</p>
<p><img src="https://i.imgur.com/tkb8MM5.png" alt=""></p>
<p>Keep in mind that you can still use old format in same profile. Example:</p>
<pre><code>[test]
SwordIron, 1, 9, Ruby, 666 = SwordIron, 1, 10
BlackMetal, 1, AxeBlackMetal,1,9, Coins, 25 = AxeBlackMetal, 1,10, Wood, 123
Coins, 0 = AxeBlackMetal, 1, 9
Coins, 0, BlackMetal, 5
</code></pre>
<p>Result will be:</p>
<p><img src="https://i.imgur.com/eTT5SbT.png" alt=""></p>
</p>
</details>
<details><summary>Quest System</summary>
<p> 
<p>In order to create your own Quests you would need to focus on two file: QuestDATABASE.cfg and QuestProfiles.cfg</p>
<p><img src="https://i.imgur.com/4l2Kshv.png" alt=""></p>
<p>QuestDATABASE.cfg - a file that contains ALL your created quests. Think about it as a place where all your quests getting their ID there, so later you can add that ID to QuestProfiles NPC</p>
<p>QuestProfiles.cfg - a file that allows you to distribute quests into NPC profiles. You may have 5 NPCs giving SAME quest, as well as 10 NPCs giving different quests</p>
<p>So… Let’s create our own first quest! First think you should do is to create a new Quest in QuestDATABASE.cfg.</p>
<p>Here’s the quest structure:</p>
<pre><code>[QuestID]
QuestType
Name
Description
Quest Target Prefab , Amount, Min Level (min level works only on Kill or Collect quest in order to set minimum level or target you need to kill)
QuestRewardType: prefab, Amount, Level
In-Game Days Cooldown
QuestRequirementType: Prefab, MinLevel (only use with Skill requirement)
</code></pre>
<p><span style="color:aqua;"> NOTE: If you want quest to be able to autocomplete (no need to speak again with npc and press “Complete” button after score is 100%), then you can write [QuestID=autocomplete]
</span></p>
<p>There are 6 types of quests: Kill, Collect, Harvest, Craft, Talk and Build:</p>
<pre><code>1) Kill - a quest where the Target is a Character (any creature) prefab. You can set minimum level of target creature to kill
2) Collect - a quest where the Target is an Item prefab. Please note that COLLECT is the only quest type that actually TAKES item from player inventory in order to be finished
3) Harvest - a quest where the Target is a Pickable prefab. Example: Pickable_Carrot, Pickable_Stone and so on. For adding score to this quest you would need to Harvest any of those &quot;farm&quot; prefabs
4) Craft - a quest where the Target is a Item prefab. You can set an item level that should be crafted or leave it 1
5) Talk - a quest where the Target is a full NPC name. After interacting with NPC target quest will autocomplete and rewards will be given
6) Build - a quest where the Target is a Piece prefab. Please note that prefabs that you build for quest target won't return any resources on destroy
</code></pre>
<p>Quest rewards type:</p>
<pre><code>1) Item - a reward where the Target is an ItemDrop prefab. You can set amount and level of given item
2) Skill - a reward where the Target is Skill name. Example Skill: Run, 10. Will give +10 levels of run skill to player who finished a quest
3) Pet - a reward where the Target is a Tameable Creature prefab that will spawn already tamed. You can set amount and level of given pet
4) Skill_EXP - a reward where the Target is Skill name. Example Skill_EXP: Run, 100. Will give +100 exp of run skill to player who finished a quest
5) EpicMMO_EXP  - a reward where the Target is amount of exp. Example EpicMMO_EXP: 100. Will give +100 exp of EpicMMO skill to player who finished a quest
6) Battlepass_EXP - a reward where the Target is amount of exp. Example Battlepass_EXP: 100. Will give +100 exp of Battlepass skill to player who finished a quest
7) MH_EXP - a reward where the Target is amount of exp. Example MH_EXP: 100. Will give +100 exp of MagicHeim experience to player who finished a quest
</code></pre>
<p>Quest Requirements Types:</p>
<pre><code>1) Skill - example: Skill: Run, 10. Will make so that only if you have skill Run at least 10 levels you can accept this quest
2) OtherQuest - example: OtherQuest: MyQuestID123. Will make so that only if you have completed quest with ID MyQuestID123 you can accept this quest
3) GlobalKey - example: GlobalKey: defeated_gdking. Will make so that quest is only acceptable if yagluth was killed on server
4) EpicMMO_Level - example: EpicMMO_Level: 20. Will make so that quest is only acceptable if player has at least 20 EpicMMO levels (other mod API)
5) HasItem - example: HasItem: SwordIron. Will make so that quest is only acceptable if player has at least 1 SwordIron in inventory
6) NotFinished - example: NotFinished: MyQuestID123. Will make so that quest is only acceptable if player has NOT finished quest with ID MyQuestID123
7) IsVIP - example: IsVIP . Will make so that quest is only acceptable if player is VIP
8) MH_Level - example: MH_Level: 20. Will make so that quest is only acceptable if player has at least 20 MagicHeim levels (other mod API)
9) Time - example: Time: 60. Will time limit quest completion to 60 seconds. If player won't complete quest in 60 seconds it will fail
</code></pre>
<p>Please note that Quest Targets, Quest Rewards and Quest Requirements may be multiple in one quest. You can add them as much as you want with | symbol. Example:</p>
<pre><code>Item: SwordIron, 1, 5 | Pet: Wolf, 2, 10 | Skill: Run, 2 | Item: Coins, 100
</code></pre>
<p>^ quest will give 1 Iron Sword level 5, 2 Wolves level 10, +2 levels of Run skill and 100 coins</p>
<p>Same for requirements:</p>
<pre><code>OtherQuest: MyQuest123 | HasItem: PickaxeIron | Skill: Run, 10
</code></pre>
<p>^ quest will be only acceptable if player has completed quest with ID MyQuest123, has at least 1 PickaxeIron in inventory and has at least 10 levels of Run skill</p>
<p>So… Now that we know all of these things lets create our first quest! I will create a quest where player will need to kill 10 wolves and get 100 Coins + Iron Sword level 3 as a reward with no quest requirements (i will leave it to None). I will set quest cooldown to be 10 in-game days (5 hours real time)</p>
<p>My quest looks like that:</p>
<pre><code>[MyTestQuest1]
Kill
This is my first quest!
And this is my first quest description!
Wolf, 10 | Skeleton, 5
Item: SwordIron, 1, 3 | Item: Coins, 100
10
None
</code></pre>
<p>Now we can add this data to out QuestDATABASE.cfg file:</p>
<p><img src="https://i.imgur.com/ejk2NIl.png" alt=""></p>
<p>After that we are able to give this quest to any NPC profile we create in QuestProfiles.cfg</p>
<p>I will create NPC profile named TestQuests and add my quest to it:</p>
<p><img src="https://i.imgur.com/rhuUwUh.png" alt=""></p>
<p>Now let’s assign this profile to our NPC:</p>
<p><img src="https://i.imgur.com/ba3gJUh.png" alt=""></p>
<p>On iteract with NPC you should get your result!</p>
<p><img src="https://i.imgur.com/lleU3rp.png" alt=""></p>
<p><img src="https://i.imgur.com/c4FHGqG.png" alt=""></p>
<p>As you can see I didn’t specify the Wolf target level (Wolf, 10). So it will by default be level 0 (0 stars). So killing any Wolf will be acceptable for this quest.</p>
<p>Let’s take quest and try it out!</p>
<p><img src="https://i.imgur.com/nVKKAud.png" alt=""></p>
<p>Note that Kill, Collect, Harvest quests will have a markers about target. You can disable marker in local Marketplace config on client</p>
<p><img src="https://i.imgur.com/GQKiXZG.png" alt=""></p>
<p>On killing wolf i get score 1/10</p>
<p><img src="https://i.imgur.com/RIOapFp.png" alt=""></p>
<p>Now let’s change our quest a little. I will change Wolf, 10 to Wolf, 10, 2. This will make so that only wolves level 2 or higher (2 stars) will be acceptable for this quest</p>
<p><img src="https://i.imgur.com/hgInMiO.png" alt=""></p>
<p>As you can see our quest target in-game changed:</p>
<p><img src="https://i.imgur.com/ZjP5S3z.png" alt=""></p>
<p><img src="https://i.imgur.com/r47i7qA.png" alt=""></p>
<p>Only wolf with 2 stars and higher now acceptable as quest target. You can see that by quest marker above wolf’s head</p>
<p>After finishing quest you can come to same NPC that gave it to you and click “Complete” button to receive rewards.</p>
<p><img src="https://i.imgur.com/5qZiacv.png" alt=""></p>
<p><img src="https://i.imgur.com/tlMY7jW.png" alt=""></p>
<p>If quest cooldown is lower than 5000 days then it will be still visible in Quest UI. Use quest cooldown 10000+ for one-time quests</p>
<p>Some Quick Screenshots with few other quest types:</p>
<p>Database:
<img src="https://i.imgur.com/IzGyHHV.png" alt=""></p>
<p>Profiles:
<img src="https://i.imgur.com/nJTMq4r.png" alt=""></p>
<p>Results:</p>
<p>Markers on Build quest targets
<img src="https://i.imgur.com/AGJ4bGI.png" alt=""></p>
<p>Markers on harvest + collect targets
<img src="https://i.imgur.com/Rr3SMac.png" alt=""></p>
<p>Markers on Talk Targets</p>
<p><img src="https://i.imgur.com/Ejrhf5u.png" alt=""></p>
<p>Good luck with creating your own quests!</p>
</p>
</details>
<details><summary>Quest Events</summary>
<p> 
Quest Events allows you to "attach" events and actions to particular quests created in QuestDatabase.cfg
<p>Possible events:</p>
<pre><code>OnAcceptQuest - when player accepts quest
OnCancelQuest - when player cancels quest
OnCompleteQuest - when player completes quest (successfully)
</code></pre>
<p>Possible actions:</p>
<pre><code>GiveItem - example: GiveItem, SwordIron, 1, 5. Will give player 1 Iron Sword level 5
GiveQuest - example: GiveQuest, MyQuestID123. Will give player quest with ID MyQuestID123
RemoveQuest - example: RemoveQuest, MyQuestID123. Will remove quest with ID MyQuestID123
Spawn - example: Spawn, Wolf, 5, 2. Will spawn 5 wolves level 2 (near)
Teleport - example: Teleport, 100, 100, 100. Will teleport player to x100, y100, z100
Damage - example: Damage, 100. Will deal 100 damage to player
Heal - example: Heal, 100. Will heal player for 100 health
PlaySound - example: PlaySound, MySound. Will play sound MySound
NpcText - example: NpcText, MyText. Will show text MyText above closest NPC head
</code></pre>
<p>Data Format:</p>
<pre><code>[questID]
Event: Action, arguments
</code></pre>
<p>Example:
<img src="https://i.imgur.com/Qcp98Rx.png" alt=""></p>
<p>You are not limited in using one event and action once, you can add as many same events as you want to with different actions, example:</p>
<pre><code>[TestQuest]
OnAcceptQuest: GiveItem, SwordIron, 1, 5
OnAcceptQuest: GiveItem, Coins, 100, 1
OnAcceptQuest: Heal, 5000
</code></pre>
</p>
</details>
<details><summary>Marketplace</summary>
<p> 
<p>The only NPC that doesn’t really need anything to be configured. Its working out of box.</p>
<p><img src="https://i.imgur.com/Av5NuBe.png" alt=""></p>
<p>To sell items click “Sell” tab =&gt; choose item you want to sell =&gt; choose quantity / price and click “Sell”</p>
<p><img src="https://i.imgur.com/Js9QC2r.png" alt=""></p>
<p>After clicking “Sell” item should appear in “BUY” tab with all other items. If you’re slot owner you can click on it and “Cancel” your sell.</p>
<p><img src="https://i.imgur.com/QKmf1Gl.png" alt=""></p>
<p>When someone will buy your item you will get currency in “Income: 0 (it will be bigger when you sell)”. To redeem your gold just click + button (Income). Currency will be added to your inventory</p>
<p>Marketplace supports all Custom Item data mods, such as EpicLoot, Jewelcrafting, Professions and such</p>
</p>
</details>
<details><summary>Banker</summary>
<p> 
Banker is an NPC that allows you to deposit / withdraw your items in bank. Also if you set Banker Income and Banker Income Time in Marketplace.cfg then each N hours (Banker Income Time) every player will get % Income to their bank resources.
<p>To create a Banker profile go to BankerProfiles.cfg and add a new profile:</p>
<p><img src="https://i.imgur.com/n7TZqfI.png" alt=""></p>
<p>I want to make a Banker profile that will accept Coins + Rubies. For that i would need to add profile [profileName] and add acceptable items on each new line</p>
<p><img src="https://i.imgur.com/Zt1lTbw.png" alt=""></p>
<p>Let’s assign Banker profile to our Banker NPC in-game:</p>
<p><img src="https://i.imgur.com/dQriWbn.png" alt=""></p>
<p>On Interact with NPC you should see this:</p>
<p><img src="https://i.imgur.com/KlarEFR.png" alt=""></p>
<p>Green number = resource amount in bank account. Bottom text = inventory amount</p>
<p>So if i want to deposit (put) 250 coins into bank i would need to write “250” and press “+” :</p>
<p><img src="https://i.imgur.com/f22k5fQ.png" alt=""></p>
<p><img src="https://i.imgur.com/SFOAvma.png" alt=""></p>
<p>As you can see now i have 250 coins in bank that will be kept there forever and getting income if server admin wants to be so</p>
<p>You may have multiple banker NPCs with different slots (resources) to keep your items in. For example you can have 1 banker that will keep your coins and another one that will keep your rubies</p>
<p>Think about banker as a “global” big chest with infinite space :D</p>
</p>
</details>
<details><summary>Info </summary>
<p> 
<p>NPC will read info from ServerInfo.cfg and display that on GUI.
Rich text markers can be applied to text you write
ServerInfo npc uses “default” profile by default. But you can add as many info profiles you want (same as Trader NPC profiles). Example below:</p>
<p><img src="https://i.imgur.com/JSZ90if.png" alt=""></p>
<p><img src="https://i.imgur.com/cwOiOsO.png" alt=""></p>
<p><img src="https://i.imgur.com/MfZXnVH.png" alt=""></p>
<p>To add data you need to create profile with [ProfileName], and then uder it you can write info you need. Later just assign this profile to Info NPC and it will show it.
Non-profiled text will be applied to every new Info NPC with “default” profile.</p>
</p>
</details>
<details><summary>Teleporter</summary>
<p> 
<p>NPC acts as teleport-hub but all in one. Its profile/data controlled by BepInEx/MarketplaceKG/TeleportHubProfiles.cfg</p>
<p><img src="https://i.imgur.com/pTjanHG.png" alt=""></p>
<p><img src="https://i.imgur.com/MpIGCz8.png" alt=""></p>
<p>To Add new teleport spots you need to add them new line each with structure: Spot Name, X coord, Y coord, Z coord, Icon name</p>
<p>You can add Icons in BepInEx/config/MarketplaceKG/MapPinsIcons folder</p>
<p><img src="https://i.imgur.com/yZVRMLF.png" alt="https://i.imgur.com/yZVRMLF.png"></p>
<p>I recommend you to use 32x32 icons.
Also you can write ItemPrefab name instead of icon in order to use its icon as map pin
When you click Interact on Teleporter NPC with profile you will open map and it will show pins to you. After Left Mouse click on icon you will teleport to XYZ coords of spot.</p>
<p><img src="https://i.imgur.com/Hoy6Gg1.png" alt="https://i.imgur.com/Hoy6Gg1.png"></p>
<p>XYZ COORDS SHOULD BE INTEGERS VALUE ONLY (5.6 &lt;= WRONG, 5 &lt;= good)</p>
<p>If you want to make teleport not instant but be more like “magic” teleport, then you can add &lt;speed=value&gt; parameter to teleport spot name</p>
<p>Example:</p>
<p>Spawn &lt;speed=10&gt;, 0,30,0</p>
<p>That will make teleport to spawn not instant but more magic-alike with speed of 10 meters / second</p>
</p>
</details>
<details><summary>Gambler</summary>
<p>
<p>An NPC that can be placed by admin. The gambler NPC requires items to activate, typically coins. The Gambler offers a list of items and a set amount of which the player can win. So for example a gambler can have ten items in the list, allow two of them to be won, and set a price to roll a chance at winning.</p>
<p>It is possible to combine an admin placed territory at NPC locations if you feel that is right for your server environment. This can provide a safe haven for players while interacting with NPC’s. The territory area will also announce itself when entering which can add ambience to the zone. Refer to the Territories reamde for more info on setting up a territory zone.</p>
<p>All NPC placed characters can be altered to include looks, clothing, interactions, patrol options, greetings, animations, salutations, etc. Refer to the “how to spawn/change section” readme for more info on setting up and altering NPC’s.<br>
<br>
<br>
<b>To add a new profile</b> you need to write [ProfileName=ItemsPerRollCount] , and then on a new line add an item list for it (<u>max 10 items</u>, first item is ITEM NEEDED TO ROLL): RollItemPrefab, RollItemCount, Item1, Item1count, Item2, Item2Count, Item3, Item3Count…<br>
Item counts can be variable as seen below.
<br></p>
<p>Example:</p>
<p>[test=2]<br>
Coins, 10, SwordIron, 1, Tar, 30-50, Wood, 1-100</p>
<p>^ This will add a profile to gambler with 2 items per roll count (he can take 2 items out of 3 in the list)<br>
Player will need 10 coins per roll, Items are: Sword iron (one), Tar (from 30 to 50 randomly), Wood (from 1 to 100) randomly</p>
<br> 
More Examples:  
<p>[gmeadows=3]<br>
Coins, 250, SpearBronze, 1, Tar, 3-5, Wood, 25, ArrowFire, 20-30, FineWood, 20, Stone, 25, ArrowWood, 20-30, Feathers, 15, MeadTasty, 3-5, TurnipStew, 2-3, ArmorTrollLeatherChest, 1, QueensJam, 3-5, FishRaw, 10, ArrowFlint, 20-30, ArmorTrollLeatherLegs, 1, Coal, 25</p>
<p>[gswamp=3]<br>
Coins, 500, AtgeirBronze, 1, ArrowFire, 30-50, ArrowBronze, 20-30, FineWood, 40, Stone, 50, ArrowIron, 10-20, Feathers, 20, MeadTasty, 3-5, TurnipStew, 3-5, ArmorRootChest, 1, OdinsDelight, 2-3, TeriyakiSalmon, 3-5, BoneArrow, 20-30, ArmorRootLegs, 1, Coal, 35</p>
<p>[gmountain=3]<br>
Coins, 1000, AtgeirIron, 1, Tar, 30-50, ArrowPoison, 50, FineWood, 60, Stone, 75, ArrowObsidian, 50, Feathers, 25, MeadTasty, 3-5, TurnipStew, 5-10, ArmorFenringChest, 1, OdinsDelight, 3-5, HoneyTeriyakiSalmonWrap, 3-5, BoneArrow, 30-50, ArmorFenringLegs, 1, Coal, 50</p>
</p>
</details>
<details><summary>Buffer</summary>
<p> 
<p>Buffer<br>
is a placeable npc that can be set in the world with pre-configured “buffs” that can be temporarily enabled on the players items. When a player interacts with the npc they can choose from what type of buff they want and on what inventory item it gets placed.</p>
<p>It is possible to combine an admin placed territory at NPC locations if you feel that is right for your server environment. This can provide a safe haven for players while interacting with NPC’s. The territory area will also announce itself when entering which can add ambience to the zone. Refer to the Territories reamde for more info on setting up a territory zone.</p>
<p>All NPC placed characters can be altered to include looks, clothing, interactions, patrol options, greetings, animations, salutations, etc. Refer to the “how to spawn/change section” readme for more info on setting up and altering NPC’s.<br>
<br></p>
<p>Buffs
The Database config is a file with ALL Your buffs. Here you will need to add all buffs so later you can use them in NPC profiles that you setup.</p>
<p>Each buff should have a UNIQUE name (it will be its own Unique ID). Buff should have a layout like this:</p>
<p>[UniqueName]<br>
Name<br>
Duration (seconds)<br>
Buff Icon (Can be taken from monster prefab name or item prefab name)<br>
Price prefab name, Price count<br>
Buff modifiers<br>
Buff visual effect<br>
Buff group</p>
<p>Example:</p>
<p>[TestBuff]<br>
First buff i created<br>
180<br>
SwordIron<br>
Coins, 1<br>
ModifyAttack = 1.5<br>
vfx_Burning<br>
Combat</p>
<p>^ Creates buff with duration 180 sec, icon = SwordIron icon,  price = 1 coin, Modifiers are ModifyAttack x1.5,
visual effect is burning and group is Combat.<br>
<br></p>
<br>
Modifiers   
All possible modfifiers: ModifyAttack, ModifyHealthRegen, ModifyStaminaRegen, ModifyRaiseSkills, ModifySpeed, ModifyNoise,
ModifyMaxCarryWeight, ModifyStealth, RunStaminaDrain, DamageReduction   
<p>Note: Multiple buffs can be applied at once by putting a “,” between them such as;<br>
ModifySpeed = 1.2, ModifyNoise = 1.4</p>
<p>One “buff” can have nine different modifiers, and the Buff Group combines Buff modifiers into one group. This is done only for balancing, so you can make cheap buffs, normal buffs, and high-priced buffs.<br>
Note: If buffs are in the same group then player would be able to buy only ONE BUFF OUT OF GROUP at a time. See below there are two examples in the “exploration” group, so only one could be purchases/applied at a time.<br>
<br></p>
<p>Profiles<br>
Buffs need to be applied to an NPC profile in order to work. To add a new profile you need to write [ProfileName] , and on a new line add buffer list for it (buff unique IDs from BufferDATABASE.cfg)</p>
<p>[MeadowsBuffs]<br>
TestBuff1, TestBuff2</p>
<p>^adds MeadowsBuffs profile to the buffer NPC with 2 buffs taken from buff database config file.</p>
<br>
More Examples:
<p>[Stealth]
Stealth Increase<br>
2400<br>
HelmetTrollLeather<br>
Coins, 300<br>
ModifyStealth = 5<br>
None<br>
Exploration</p>
<p>[Speed]<br>
Swiftness<br>
1600<br>
TankardOdin<br>
Coins, 150<br>
ModifySpeed = 1.5<br>
None<br>
Speed</p>
<p>[Run]<br>
Running Increase<br>
1800<br>
GlowingMushroom<br>
Coins, 500<br>
ModifyStaminaRegen = 2, ModifySpeed = 2<br>
vfx_GodExplosion<br>
Exploration</p>
<p>[Tenacity]<br>
Toughness increase<br>
900<br>
HelmetDrake<br>
Coins, 500<br>
DamageReduction = 0.30<br>
vfx_creature_love<br>
Toughness</p>
<p>[Assault]<br>
Fighting increase<br>
600<br>
FlametalOre<br>
Coins, 500<br>
ModifyAttack = 2<br>
vfx_fir_oldlog<br>
Rage</p>
<p>Note: you can view all the in-game VFX by using easy spawner and searching for vfx.<br>
some common useful ones are vfx_HealthUpgrade, vfx_lootspawn, vfx_odin_despawn, vfx_offering, vfx_perfectblock, vfx_odin_despawn</p>
</p>
</details>
<details><summary>Territory System</summary>
<p> 
<p>The <code>TerritoryDatabase.cfg</code> file is used to define territories or zones within your game world. Each zone can have specific attributes such as shape, position, size, color, flags, and owners. This guide will help you understand the format and options available in the configuration file.</p>
<h2>Format</h2>
<p>The configuration file follows the following format:</p>
<pre><code class="language-plaintext">[ZoneName]
Shape type (Circle, Square, Custom)
X pos, Z pos, Radius (for circle/square) or X pos, Z pos, X length, Z length (for custom zone)
Red Color, Green Color, Blue Color, Show Territory on water (True/False)
Zone Flags (separated by comma if multiple)
Owners SteamID (separated by comma if multiple)
</code></pre>
<h2>Zone Attributes</h2>
<h3>Zone Name</h3>
<p>Each zone entry begins with a unique <code>ZoneName</code>. This identifier is used to differentiate between different zones in the configuration file.</p>
<h3>Shape Type</h3>
<p>The shape of the zone can be specified as one of the following:</p>
<ul>
<li>Circle: The zone is defined as a circle with a center point and a radius.</li>
<li>Square: The zone is defined as a square with a center point and side length.</li>
<li>Custom: The zone is defined with custom dimensions using the X and Z position coordinates, along with the X and Z lengths.</li>
</ul>
<h3>Position and Size</h3>
<p>Depending on the shape type, you need to specify the position and size of the zone:</p>
<ul>
<li>For a circle or square, provide the X and Z position coordinates and the radius (for a circle) or side length (for a square).</li>
<li>For a custom zone, provide the X and Z position coordinates, as well as the X and Z lengths.</li>
</ul>
<h3>Color and Show Territory on Water</h3>
<p>Specify the color of the zone using RGB values (Red, Green, Blue). Additionally, indicate whether the territory should be visible on water by specifying <code>True</code> or <code>False</code> after the RGB color values.</p>
<h3>Zone Flags</h3>
<p>You can assign specific flags to a zone to define its behavior and characteristics. Multiple flags can be assigned to a zone, separated by commas. Here are the available flags:</p>
<ul>
<li><code>PushAway</code>: Players are pushed away from the zone boundaries.</li>
<li><code>NoBuild</code>: Building structures is not allowed within the zone.</li>
<li><code>NoPickaxe</code>: Players cannot use pickaxes within the zone.</li>
<li><code>NoInteract</code>: Interactions with objects or NPCs within the zone are disabled.</li>
<li><code>NoAttack</code>: Players cannot initiate attacks or engage in combat within the zone.</li>
<li><code>PvpOnly</code>: Forces PvP mode within the zone.</li>
<li><code>PveOnly</code>:  Forces PvE mode within the zone.</li>
<li><code>PeriodicHeal</code>: Players are periodically healed while inside the zone (only zone owners).</li>
<li><code>PeriodicDamage</code>: Players receive periodic damage while inside the zone.</li>
<li><code>IncreasedPlayerDamage</code>: Player attacks deal increased damage within the zone.</li>
<li><code>IncreasedMonsterDamage</code>: Monsters deal increased damage to players within the zone.</li>
<li><code>NoMonsters</code>: Monsters do not spawn or exist within the zone.</li>
<li><code>CustomEnvironment</code>: The zone has a custom environment specified by the environment name.</li>
<li><code>MoveSpeedMultiplier</code>: Players’ movement speed is multiplied by a certain factor within the zone.</li>
<li><code>NoDeathPenalty</code>: Players do not suffer penalties upon death within the zone.</li>
<li><code>NoPortals</code>: Teleportation portals cannot be used within the zone.</li>
<li><code>PeriodicHealALL</code>: All players are periodically healed within the zone.</li>
<li><code>ForceGroundHeight</code>: The ground height is forcefully set within the zone.</li>
<li><code>ForceBiome</code>: The biome within the zone is forcefully set.</li>
<li><code>AddGroundHeight</code>: Additional ground height is added within the zone.</li>
<li><code>NoBuildDamage</code>: Structures within the zone do not take damage.</li>
<li><code>MonstersAddStars</code>: Monsters within the zone have additional stars, indicating higher difficulty.</li>
<li><code>InfiniteFuel</code>: Fuel consumption is disabled within the zone.</li>
<li><code>NoInteractItems</code>: Interactions with items within the zone are disabled.</li>
<li><code>NoInteractCraftingStation</code>: Interactions with crafting stations within the zone are disabled.</li>
<li><code>NoInteractItemStands</code>: Interactions with item stands within the zone are disabled.</li>
<li><code>NoInteractChests</code>: Interactions with chests within the zone are disabled.</li>
<li><code>NoInteractDoors</code>: Interactions with doors within the zone are disabled.</li>
<li><code>NoStructureSupport</code>: Structures within the zone do not get damaged if they are not supported.</li>
<li><code>NoInteractPortals</code>: Interactions with portals within the zone are disabled.</li>
<li><code>CustomPaint</code>: The zone has custom paint applied to it.</li>
<li><code>LimitZoneHeight</code>: The minimum height of the zone is limited.</li>
<li><code>NoItemLoss</code>: Players do not lose items upon death within the zone.</li>
<li><code>SnowMask</code>: A snow mask effect is applied within the zone.</li>
<li><code>NoMist</code>: Mist weather effects are disabled within the zone.</li>
<li><code>InfiniteEitr</code>: Eitr consumption is disabled within the zone.</li>
<li><code>InfiniteStamina</code>: Stamina consumption is disabled within the zone.</li>
<li><code>NoCreatureDrops</code>: Creatures within the zone do not drop items upon defeat.</li>
</ul>
<p><strong>Note:</strong> For the <code>CustomEnvironment</code>, <code>PeriodicDamage</code>, <code>PeriodicHealALL</code>, <code>PeriodicHeal</code>, <code>IncreasedMonsterDamage</code>, <code>IncreasedPlayerDamage</code>, <code>MoveSpeedMultiplier</code>, <code>ForceGroundHeight</code>, <code>AddGroundHeight</code>, <code>LimitZoneHeight</code>, <code>ForceBiome</code>, <code>MonstersAddStars</code>, and <code>CustomPaint</code> flags, the flag should be followed by = and the value of the flag. For example, <code>CustomEnvironment = Clear</code> or <code>PeriodicDamage = 10</code>.</p>
<p><code>ForceBiome</code> accepts values:</p>
<pre><code>Meadows = 1,
Swamp = 2,
Mountain = 4,
BlackForest = 8,
Plains = 16,
AshLands = 32,
DeepNorth = 64,
Ocean = 256,
Mistlands = 512
</code></pre>
<p>(<code>ForceBiome = 2</code> will force the biome to be swamp)</p>
<p><code>CustomPaint</code> accepts values:</p>
<pre><code>Paved = 0,
Grass = 1,
Cultivated = 2,
Dirt = 3
</code></pre>
<p>(<code>CustomPaint = 2</code> will paint the zone with the Cultivated texture)</p>
<h3>Owners</h3>
<p>Specify the SteamIDs of the owners of the zone. If there are multiple owners, separate their SteamIDs with commas.</p>
<h2>Example</h2>
<p>Here’s an example entry in the <code>TerritoryDatabase.cfg</code> file:</p>
<pre><code class="language-plaintext">[ExampleZone]
Square
150, 100, 800
0, 128, 255
False
NoBuild, NoInteract, PeriodicHealALL = 50
None


[ZoneWithHigherPriority@2]
Square
150, 100, 400
255, 0, 0
False
CustomEnvironment = Clear, NoAttack, NoPickaxe, PeriodicDamage = 10
None

</code></pre>
<p>All zones by default having priority 1. If you want to change priority of zone, you need to add <code>@</code> and priority number after zone name. For example, <code>ZoneWithHigherPriority@2</code> will have priority 2.
That will allow you to create zones inside zones. For example, you can create a zone with priority 1 and then create a zone with priority 2 inside it.</p>
</p>
</details>
<details><summary>Battlepass</summary>
<p> 
<p>Battlepass<br>
is a reward system for players on a server. It allows the admin to set items as rewards, and players can claim their reward when they have accumulated enough experience points. The admin will need to create quests or find some other way to award battlepass experience to the players.</p>
<p>The battlepass folder contains a main config, a config for free rewards, and another for premium rewards. To add players to the premium list you must enter their Steam Ids in the main marketplace.cfg file in the section “BattlepassVIPlist”. Only those players will have access to premium rewards.</p>
<p>The main config has two options. First is the battlepass name which is a unique name. Be careful choosing the name because after changing the battlepass name it will drop all experience / rewards for the previous battlepass name, meaning all players accumulated experience will be lost if you change the name mid-season.</p>
<p>The second option is the battlepass experience step. This can be whatever integer value you wish. This value should correlate with the amount of experience being awarded through quests. If the experience step is set to 50 then you may wish to give smaller experience rewards from quests like 10 or 15 per quest completed. However, if you set the steps to 200 then you will need to increase the amount given for quests to accomodate.</p>
<p>Finally, if you want to skip a level then simply do not include the “reward level”. For example, if you want to have a reward at level 2 and then the next at level 5 all you have to do is not include a reward level for the levels in between. For example, go straight from level 3 to level 7.</p>
<p>Format
The format for creating the rewards is the same for either free or premium. The format for entering rewards is [unique name = reward level] , followed by the reward on the next line. The format of the reward is item name,amount,item level</p>
<p>Example:<br>
[food is good = 1]<br>
Carrot,5,0</p>
<p>More Examples:</p>
<p>[reward = 1]<br>
ArmorTrollLeatherLegs,1,0</p>
<p>[reward = 2]<br>
ArmorTrollLeatherChest,1,0</p>
<p>[reward = 3]<br>
HelmetTrollLeather,1,0</p>
<p>[reward = 4]<br>
CapeTrollHide,1,0</p>
<p>[reward = 5]<br>
BowFineWood,1,0</p>
<p>[reward = 6]<br>
SpearChitin,1,0</p>
<p>[reward = 7]<br>
ArmorIronLegs,1,0</p>
<p>[reward = 8]<br>
ArmorIronChest,1,0</p>
<p>[reward = 9]<br>
HelmetIron,1,0</p>
</p>
</details>
<details><summary>NPC Dialogues</summary>
<h3>File Format</h3>
<p>The <code>NpcDialogues.cfg</code> file is written in a simple and human-readable format. Each dialogue entry consists of a unique profile name followed by the NPC dialogue text and player options. The player options can have various attributes such as text, transition, command, icon, condition, and always visible.</p>
<p>Here’s the structure of a dialogue entry:</p>
<pre><code>[UniqueProfileName]
Dialogue Text
Player Option 1
Player Option 2
...
</code></pre>
<p>The player options can have the following attributes:</p>
<ul>
<li>
<p><code>Text</code>: Represents the text of the player option.</p>
</li>
<li>
<p><code>Transition</code>: Specifies a transition to another dialogue.</p>
</li>
<li>
<p><code>Command</code>: Specifies the command associated with the player option.</p>
</li>
<li>
<p><code>Icon</code>: Represents an icon associated with the player option ( Can be any item prefab or icons from client folder ).</p>
</li>
<li>
<p><code>Condition</code>: Defines the condition under which the player option is available.</p>
</li>
<li>
<p><code>AlwaysVisible</code>: Indicates that the player option is always visible, regardless of conditions.</p>
</li>
</ul>
<p>Dialogue may have multiple attributes split by | (pipe) character. For example:</p>
<pre><code>[UniqueProfileName]
NPC text
Text: Option1 | Transition: UniqueProfileName2 | Command: Damage, 20 | Icon: Hammer | Condition: NotFinished, QuestId | AlwaysVisible: true
Text: Option2 | Transition: UniqueProfileName3 | Command: Heal, 20 | Icon: SwordIron | Condition: NotFinished, QuestId | AlwaysVisible: true
</code></pre>
<h3>Conditions</h3>
<p>The following conditions can be used in the <code>NpcDialogues.cfg</code> file:</p>
<ul>
<li>
<p><code>NotFinished</code></p>
<ul>
<li><strong>Usage</strong>: <code>NotFinished, QuestId</code></li>
<li><strong>Description</strong>: Checks if the specified quest is not finished yet.</li>
</ul>
</li>
<li>
<p><code>OtherQuest</code></p>
<ul>
<li><strong>Usage</strong>: <code>OtherQuest, QuestId</code></li>
<li><strong>Description</strong>: Checks if the specified quest is already finished.</li>
</ul>
</li>
<li>
<p><code>HasItem</code></p>
<ul>
<li><strong>Usage</strong>: <code>HasItem, ItemPrefab, Amount</code></li>
<li><strong>Description</strong>: Checks if the player has the specified amount of a particular item.</li>
</ul>
</li>
<li>
<p><code>HasBuff</code></p>
<ul>
<li><strong>Usage</strong>: <code>HasBuff, BuffName</code></li>
<li><strong>Description</strong>: Checks if the player currently has the specified buff.</li>
</ul>
</li>
<li>
<p><code>Skill</code></p>
<ul>
<li><strong>Usage</strong>: <code>Skill, SkillName, MinLevel</code></li>
<li><strong>Description</strong>: Checks if the player’s skill level in the specified skill is equal to or higher than the minimum level.</li>
</ul>
</li>
<li>
<p><code>GlobalKey</code></p>
<ul>
<li><strong>Usage</strong>: <code>GlobalKey, GlobalKey</code></li>
<li><strong>Description</strong>: Checks if the specified global key is active.</li>
</ul>
</li>
<li>
<p><code>IsVIP</code></p>
<ul>
<li><strong>Usage</strong>: <code>IsVIP</code></li>
<li><strong>Description</strong>: Checks if the player is a VIP.</li>
</ul>
</li>
</ul>
<p>Please note that you can use these conditions within the player options of your dialogue entries to control the availability and visibility of options based on specific game conditions or player states.</p>
<p>Feel free to refer to this documentation for further clarification or provide more examples if needed.</p>
<p>Please note that you should replace the placeholder values (<code>UniqueProfileName</code>, <code>Dialogue Text</code>, <code>Player options</code>, <code>Text</code>, <code>Transition</code>, <code>Command</code>, <code>Icon</code>, <code>Condition</code>, <code>AlwaysVisible</code>, <code>QuestId</code>, <code>ItemPrefab</code>, <code>amount</code>, <code>BuffName</code>, <code>SkillName</code>, <code>MinLevel</code>, <code>somekey</code>) with actual values relevant to your game and dialogues.</p>
<h3>Commands</h3>
<p>The following commands can be used in the <code>NpcDialogues.cfg</code> file:</p>
<ul>
<li>
<p><code>OpenUI</code>: Opens a specific NPC type profile UI.</p>
<ul>
<li><strong>Usage</strong>: <code>OpenUI, NPC Type, Profile Name</code></li>
<li><strong>Description</strong>: Opens the UI associated with a particular NPC type profile.</li>
<li><strong>Possible NPC Types</strong>: Marketplace, Trader, Info, Teleporter, Feedback, Banker, Gambler, Quests, Buffer, Transmog</li>
</ul>
</li>
<li>
<p><code>PlaySound</code>: Plays a sound.</p>
<ul>
<li><strong>Usage</strong>: <code>PlaySound, SoundName</code></li>
<li><strong>Description</strong>: Plays the specified sound.</li>
</ul>
</li>
<li>
<p><code>GiveQuest</code>: Gives a quest to the player.</p>
<ul>
<li><strong>Usage</strong>: <code>GiveQuest, QuestID</code></li>
<li><strong>Description</strong>: Gives the player the specified quest.</li>
</ul>
</li>
<li>
<p><code>GiveItem</code>: Gives an item to the player.</p>
<ul>
<li><strong>Usage</strong>: <code>GiveItem, ItemPrefab, Amount, Level</code></li>
<li><strong>Description</strong>: Gives the player a specified number of items of a certain level.</li>
</ul>
</li>
<li>
<p><code>RemoveItem</code>: Removes items from the player’s inventory.</p>
<ul>
<li><strong>Usage</strong>: <code>RemoveItem, ItemPrefab, Amount</code></li>
<li><strong>Description</strong>: Removes a specified number of items from the player’s inventory.</li>
</ul>
</li>
<li>
<p><code>Spawn</code>: Spawns creatures nearby.</p>
<ul>
<li><strong>Usage</strong>: <code>Spawn, CreaturePrefab, Amount, Level</code></li>
<li><strong>Description</strong>: Spawns a specified number of creatures of a certain level near the player.</li>
</ul>
</li>
<li>
<p><code>Teleport</code>: Teleports the player to a specific location.</p>
<ul>
<li><strong>Usage</strong>: <code>Teleport, X, Y, Z</code></li>
<li><strong>Description</strong>: Teleports the player to the specified coordinates.</li>
</ul>
</li>
<li>
<p><code>RemoveQuest</code>: Removes a quest from the player.</p>
<ul>
<li><strong>Usage</strong>: <code>RemoveQuest, QuestID</code></li>
<li><strong>Description</strong>: Removes the specified quest from the player’s quest log.</li>
</ul>
</li>
<li>
<p><code>Damage</code>: Inflicts damage on the player.</p>
<ul>
<li><strong>Usage</strong>: <code>Damage, Value</code></li>
<li><strong>Description</strong>: Damages the player by the specified value.</li>
</ul>
</li>
<li>
<p><code>Heal</code>: Restores health to the player.</p>
<ul>
<li><strong>Usage</strong>: <code>Heal, Value</code></li>
<li><strong>Description</strong>: Restores the player’s health by the specified value.</li>
</ul>
</li>
<li>
<p><code>GiveBuff</code>: Gives a buff to the player.</p>
<ul>
<li><strong>Usage</strong>: <code>GiveBuff, BuffID</code></li>
<li><strong>Description</strong>: Gives the player the specified buff.</li>
</ul>
</li>
</ul>
<p>Please note that you can use these commands within the player options of your dialogue entries to trigger specific actions or behaviors based on the player’s choices.</p>
<p>You can use <strong>multiple</strong> commands and conditions in a single player option by separating them with | (pipe) character.</p>
<h1>Dialogue exampes:</h1>
<pre><code>[default]
Welcome to the village!
Text: Hello there! What brings you to our peaceful village?
Text: How can I assist you today?
Text: Tell me more about this village | Command: OpenUI, Info, VillageInfoProfile | Icon: village_icon
Text: I'm looking for work | Transition: JobOptions | Icon: job_icon

[JobOptions]
Available job options:
Text: We have various job opportunities available. What type of work are you interested in?
Text: Farming | Command: OpenUI, Quests, Job | Icon: Hoe | Condition: HasItem, Hoe, 1 
Text: Fishing | Command: OpenUI, Quests, FishingJob | Icon: Fish1 | Condition: Skill, Fishing, 10
</code></pre>
<p>Then just attach initial (in our case default) dialogue to NPC UI</p>
</details>
<details><summary><span style="color:crimson;font-weight:200;font-size:18px">Transmogrification</span></summary>
<p> 
<p>Transmogrification is a system that allows your players to give their equipment any other item appearance in game.</p>
<p>As server admin you can configure which npc / profile will give which skins to use.</p>
<p>Transmogrification is a Paid-feature in Marketplace so in order to use it you need to buy access. If you want to use it please contact KG#7777 (discord).</p>
<p>In order to start configuring the system go to marketplace folder and open TransmogrificationProfiles.cfg.</p>
<p>Data Format:</p>
<pre><code>[ProfileName]
SkinPrefab, Price Prefab, Price Amount, Skip TypeCheck true/false, Special VFX ID (optional)
</code></pre>
<p>To add more items to profile add them on new line.
Example:</p>
<pre><code>[TestProfile]
SwordIron, Coins, 10, false
SwordIron, Coins, 20, false, 2
SwordIron, Coins, 50, false, 20
SwordIronFire, Ruby, 10, false
SwordIronFire, Ruby, 20, false, 2
SwordIronFire, Ruby, 50, false, 20
</code></pre>
<p>^ This profile will give NPC 6 items to use as skins, usual IronSword, IronSword with VFX ID 2, IronSword with VFX ID 20, FireSword, FireSword with VFX ID 2, FireSword with VFX ID 20.</p>
<p>Note that if VFX id is 21 then players will be able to chooce vfx manually on item.</p>
<ol>
<li>Assigning profile to NPC:
<img src="https://i.imgur.com/AZVMocc.png" alt=""></li>
<li>Open UI by interacting with NPC to see result:
<img src="https://i.imgur.com/tbbWD7j.png" alt="">
In Left side you can choose item from your inventory you want to transmogrify and then choose an item in right window</li>
</ol>
<p>IF YOU SET SKIP TYPECHECK TO TRUE, YOU WILL BE ABLE TO USE ANY ITEM AS SKIN, EVEN IF IT IS NOT EQUIPMENT. THIS WILL CAUSE SOME ISSUES WITH SOME ITEMS, SO USE IT ONLY IF YOU KNOW WHAT YOU ARE DOING.</p>
<p>Also skip typecheck will allow you to set 2-handed weapon as skin for 1-handed weapon and vice versa. Or it will allow you to use Trophy as skin:</p>
<p><img src="https://i.imgur.com/T8QmpJm.png" alt=""></p>
<p><img src="https://i.imgur.com/Sd4Xsdo.png" alt=""></p>
<p>As you noticed there are 20 VFX’s marketplace can give you. To use them after typecheck skip true/false write VFX ID you want to use.</p>
<p>Effect names by default:</p>
<pre><code>mpasn_transmog_eff1: Azure Ashes
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
</code></pre>
<p>Lets try to affect out Cheat Sword with transmog:</p>
<ol>
<li>Choose item
<img src="https://i.imgur.com/SDJsDOh.png" alt=""></li>
<li>Choose skin
<img src="https://i.imgur.com/DSkdimb.png" alt=""></li>
</ol>
<p>(press little square icon in right bottom)</p>
<ol start="4">
<li>Done:
<img src="https://i.imgur.com/STsZbGs.png" alt=""></li>
<li>Out item looks like that now:
<img src="https://i.imgur.com/T4Ss9IB.png" alt=""></li>
<li>When you equip item you will see that its appearance changed, as well now it has VFX. All weapon stats are same, as well as animation of attacks and so on:
<img src="https://i.imgur.com/apOXM30.png" alt=""></li>
<li>If you want to remove transmog from item - choose an item in UI and press “Clear” button</li>
</ol>
</p>
</details>
<h2>ALL OPTIONS / PROFILES / NPCs DATA ARE AUTO-RELOADED IN SERVER RUNTIME WITHOUT RESTART</h2>
<h2><img src="https://i.imgur.com/5ZHfxlo.png" alt="https://i.imgur.com/5ZHfxlo.png"></h2>
<h2>To install mod place MarketPlaceRevamped.dll into Client Plugins folder AND Server Plugins Folder</h2>
<p><img src="https://i.imgur.com/gTTJ9HJ.png" alt="https://i.imgur.com/gTTJ9HJ.png"></p>
<p>For Questions or Comments, find KG#7777 <img src="https://i.imgur.com/CPYNjXV.png" alt="https://i.imgur.com/CPYNjXV.png">﻿ in the Odin Plus Team Discord:
<a href="https://discord.gg/5gXNxNkUBt"><img src="https://i.imgur.com/XXP6HCU.png" alt="https://i.imgur.com/XXP6HCU.png"></a></p>
