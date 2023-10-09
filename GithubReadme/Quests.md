
In order to create your own Quests you would need to focus on two folders: 

1) Quests Database folder: `Valheim\BepInEx\config\Marketplace\Configs\Quests`
2) Quest Profiles folder: `Valheim\BepInEx\config\Marketplace\Configs\QuestProfiles`

Simply create ANYFILENAME.cfg and corresponding folder and fill it with data.

Quest Database config file - a file that contains ALL your created quests. Think about it as a place where all your quests getting their ID there, so later you can add that ID to QuestProfiles NPC

Quest Profiles config file - a file that allows you to distribute quests into NPC profiles. You may have 5 NPCs giving SAME quest, as well as 10 NPCs giving different quests

So... Let's create our own first quest! First think you should do is to create a new Quest in QuestDatabase.cfg.

Here's the quest structure:
```
[QuestID]
QuestType
Name
Description
Quest Target Prefab , Amount, Min Level (min level works only on Kill or Collect quest in order to set minimum level or target you need to kill)
QuestRewardType: prefab, Amount, Level
In-Game Days Cooldown
QuestRequirementType: Prefab, MinLevel (only use with Skill requirement)
```

<span style="color:aqua;"> NOTE: If you want quest to be able to autocomplete (no need to speak again with npc and press "Complete" button after score is 100%), then you can write [QuestID=autocomplete]
</span>

There are 6 types of quests: Kill, Collect, Harvest, Craft, Talk and Build:
```
1) Kill - a quest where the Target is a Character (any creature) prefab. You can set minimum level of target creature to kill
2) Collect - a quest where the Target is an Item prefab. Please note that COLLECT is the only quest type that actually TAKES item from player inventory in order to be finished
3) Harvest - a quest where the Target is a Pickable prefab. Example: Pickable_Carrot, Pickable_Stone and so on. For adding score to this quest you would need to Harvest any of those "farm" prefabs
4) Craft - a quest where the Target is a Item prefab. You can set an item level that should be crafted or leave it 1
5) Talk - a quest where the Target is a full NPC name. After interacting with NPC target quest will autocomplete and rewards will be given
6) Build - a quest where the Target is a Piece prefab. Please note that prefabs that you build for quest target won't return any resources on destroy
```
Quest rewards type:
```
1) Item - a reward where the Target is an ItemDrop prefab. You can set amount and level of given item
2) Skill - a reward where the Target is Skill name. Example Skill: Run, 10. Will give +10 levels of run skill to player who finished a quest
3) Pet - a reward where the Target is a Tameable Creature prefab that will spawn already tamed. You can set amount and level of given pet
4) Skill_EXP - a reward where the Target is Skill name. Example Skill_EXP: Run, 100. Will give +100 exp of run skill to player who finished a quest
5) EpicMMO_EXP  - a reward where the Target is amount of exp. Example EpicMMO_EXP: 100. Will give +100 exp of EpicMMO skill to player who finished a quest
6) Battlepass_EXP - a reward where the Target is amount of exp. Example Battlepass_EXP: 100. Will give +100 exp of Battlepass skill to player who finished a quest
7) MH_EXP - a reward where the Target is amount of exp. Example MH_EXP: 100. Will give +100 exp of MagicHeim experience to player who finished a quest
8) Cozyheim_EXP - a reward where the Target is amount of exp. Example Cozyheim_EXP: 100. Will give +100 exp of Cozyheim experience to player who finished a quest
9) SetCustomValue - a reward where the Target is a CustomValue name. Example SetCustomValue: MyCustomValue, 100. Will set CustomValue with name MyCustomValue to 100
10) AddCustomValue - a reward where the Target is a CustomValue name. Example AddCustomValue: MyCustomValue, 100. Will add 100 to CustomValue with name MyCustomValue
11) GuildAddLevel - a reward where the Target is amount of guild level. Example GuildAddLevel: 10. Will add 10 guild levels to player's guild
```

Quest Requirements Types:
```
Quests using absolutely same conditions / requirements as Dialogues. Dialogues guides is more complete so head towards there: https://kg-dev.xyz/#Dialogues
```

Please note that Quest Targets, Quest Rewards and Quest Requirements may be multiple in one quest. You can add them as much as you want with | symbol. Example:


```
Item: SwordIron, 1, 5 | Pet: Wolf, 2, 10 | Skill: Run, 2 | Item: Coins, 100
```
^ quest will give 1 Iron Sword level 5, 2 Wolves level 10, +2 levels of Run skill and 100 coins

Same for requirements:
```
OtherQuest: MyQuest123 | HasItem: PickaxeIron | Skill: Run, 10
```
^ quest will be only acceptable if player has completed quest with ID MyQuest123, has at least 1 PickaxeIron in inventory and has at least 10 levels of Run skill

So... Now that we know all of these things lets create our first quest! I will create a quest where player will need to kill 10 wolves and get 100 Coins + Iron Sword level 3 as a reward with no quest requirements (i will leave it to None). I will set quest cooldown to be 10 in-game days (5 hours real time)

My quest looks like that:
```
[MyTestQuest1]
Kill
This is my first quest!
And this is my first quest description!
Wolf, 10 | Skeleton, 5
Item: SwordIron, 1, 3 | Item: Coins, 100
10
None
```
Now we can add this data to out QuestDATABASE.cfg file:

![](https://i.imgur.com/ejk2NIl.png)

After that we are able to give this quest to any NPC profile we create in QuestProfiles.cfg (in quest profiles folder)

I will create NPC profile named TestQuests and add my quest to it:

![](https://i.imgur.com/rhuUwUh.png)

Now let's assign this profile to our NPC:

![](https://i.imgur.com/ba3gJUh.png)

On iteract with NPC you should get your result!

![](https://i.imgur.com/lleU3rp.png)

![](https://i.imgur.com/c4FHGqG.png)

As you can see I didn't specify the Wolf target level (Wolf, 10). So it will by default be level 0 (0 stars). So killing any Wolf will be acceptable for this quest.

Let's take quest and try it out!

![](https://i.imgur.com/nVKKAud.png)

Note that Kill, Collect, Harvest quests will have a markers about target. You can disable marker in local Marketplace config on client

![](https://i.imgur.com/GQKiXZG.png)

On killing wolf i get score 1/10

![](https://i.imgur.com/RIOapFp.png)

Now let's change our quest a little. I will change Wolf, 10 to Wolf, 10, 2. This will make so that only wolves level 2 or higher (2 stars) will be acceptable for this quest

![](https://i.imgur.com/hgInMiO.png)

As you can see our quest target in-game changed:

![](https://i.imgur.com/ZjP5S3z.png)

![](https://i.imgur.com/r47i7qA.png)

Only wolf with 2 stars and higher now acceptable as quest target. You can see that by quest marker above wolf's head

After finishing quest you can come to same NPC that gave it to you and click "Complete" button to receive rewards.

![](https://i.imgur.com/5qZiacv.png)

![](https://i.imgur.com/tlMY7jW.png)

If quest cooldown is lower than 5000 days then it will be still visible in Quest UI. Use quest cooldown 10000+ for one-time quests

Some Quick Screenshots with few other quest types:

Database:
![](https://i.imgur.com/IzGyHHV.png)

Profiles:
![](https://i.imgur.com/nJTMq4r.png)

Results:

Markers on Build quest targets
![](https://i.imgur.com/AGJ4bGI.png)

Markers on harvest + collect targets
![](https://i.imgur.com/Rr3SMac.png)


Markers on Talk Targets

![](https://i.imgur.com/Ejrhf5u.png)