### File Format

The `Dialogues.cfg` file is written in a simple and human-readable format. Each dialogue entry consists of a unique profile name followed by the NPC dialogue text and player options. The player options can have various attributes such as text, transition, command, icon, condition, and always visible.

In order to create Dialogues go to `Valheim\BepInEx\config\Marketplace\Configs\Dialogues` folder and create `AnyName.cfg` file. Then you can fill it with your custom entries

Here's the structure of a dialogue entry:

```
[UniqueProfileName]
Dialogue Text
Player Option 1
Player Option 2
...
```

The player options can have the following attributes:

- `Text`: Represents the text of the player option.

- `Transition`: Specifies a transition to another dialogue.

- `Command`: Specifies the command associated with the player option.

- `Icon`: Represents an icon associated with the player option ( Can be any item prefab or icons from client folder ).

- `Condition`: Defines the condition under which the player option is available.

- `AlwaysVisible`: Indicates that the player option is always visible, regardless of conditions.

- `Color`: Represents the color of the player option text.

Dialogue may have multiple attributes split by | (pipe) character. For example:

```
[UniqueProfileName]
NPC text
Text: Option1 | Transition: UniqueProfileName2 | Command: Damage, 20 | Command: GiveItem, Coins, 50 | Icon: Hammer | Condition: NotFinished, QuestId | AlwaysVisible: false | Color: 255, 128, 0
Text: Option2 | Transition: UniqueProfileName3 | Command: Heal, 20 | Icon: SwordIron | Condition: NotFinished, QuestId | AlwaysVisible: true
```

---

### Conditions

The following conditions can be used in the `NpcDialogues.cfg` file:


- `HasItem`
    - **Usage**: `HasItem, ItemPrefab, Amount`
    - **Description**: Checks if the player has the specified amount of a particular item.

- `NotHasItem`
    - **Usage**: `NotHasItem, ItemPrefab, Amount`
    - **Description**: Checks if the player does not have the specified amount of a particular item.

- `HasBuff`
    - **Usage**: `HasBuff, BuffName`
    - **Description**: Checks if the player has the specified buff.

- `NotHasBuff`
    - **Usage**: `NotHasBuff, BuffName`
    - **Description**: Checks if the player does not have the specified buff.

- `SkillMore`
    - **Usage**: `SkillMore, SkillName, MinLevel`
    - **Description**: Checks if the player's skill level in the specified skill is greater than or equal to the minimum level.

- `SkillLess`
    - **Usage**: `SkillLess, SkillName, MaxLevel`
    - **Description**: Checks if the player's skill level in the specified skill is less than maximum level.

- `GlobalKey`
    - **Usage**: `GlobalKey, KeyName`
    - **Description**: Checks if the specified global key is active.

- `NotGlobalKey`
    - **Usage**: `NotGlobalKey, KeyName`
    - **Description**: Checks if the specified global key is not active.

- `IsVIP`
    - **Usage**: `IsVIP`
    - **Description**: Checks if the player is a VIP.

- `NotIsVIP`
    - **Usage**: `NotIsVIP`
    - **Description**: Checks if the player is not a VIP.

- `HasQuest`
    - **Usage**: `HasQuest, QuestID`
    - **Description**: Checks if the player has the specified quest.

- `NotHasQuest`
    - **Usage**: `NotHasQuest, QuestID`
    - **Description**: Checks if the player does not have the specified quest.

- `QuestProgressDone`
    - **Usage**: `QuestProgressDone, QuestID`
    - **Description**: Checks if quest progress is done (max score).

- `QuestProgressNotDone`
    - **Usage**: `QuestProgressNotDone, QuestID`
    - **Description**:  Checks if quest progress is not done (max score).

- `QuestFinished`
    - **Usage**: `QuestFinished, QuestID`
    - **Description**: Checks if the specified quest is already finished.

- `QuestNotFinished`
    - **Usage**: `QuestNotFinished, QuestID`
    - **Description**: Checks if the specified quest is not finished yet.

- `EpicMMOLevelMore`
    - **Usage**: `EpicMMOLevelMore, MinLevel`
    - **Description**: Checks if the player's EpicMMO level is greater than or equal to the minimum level.

- `EpicMMOLevelLess`
     - **Usage**: `EpicMMOLevelLess, MaxLevel`
     - **Description**: Checks if the player's EpicMMO level is less than maximum level.

- `CozyheimLevelMore`
     - **Usage**: `CozyheimLevelMore, MinLevel`
     - **Description**: Checks if the player's Cozyheim level is greater than or equal to the minimum level.

- `CozyheimLevelLess`
     - **Usage**: `CozyheimLevelLess, MaxLevel`
     - **Description**: Checks if the player's Cozyheim level is less than maximum level.

- `HasAchievement`
     - **Usage**: `HasAchievement, AchievementID`
     - **Description**: Checks if the player has the specified achievement.

- `NotHasAchievement`
     - **Usage**: `NotHasAchievement, AchievementID`
     - **Description**: Checks if the player does not have the specified achievement.

- `HasAchievementScore`
     - **Usage**: `HasAchievementScore, MinScore`
     - **Description**: Checks if the player's achievements score is greater than or equal to the minimum score.

- `NotHasAchievementScore`
     - **Usage**: `NotHasAchievementScore, MaxScore`
     - **Description**: Checks if the player's achievements score is less than maximum score.

- `CustomValueMore`
     - **Usage**: `CustomValueMore, KeyName, MinValue`
     - **Description**: Checks if the player's custom value is greater than or equal to the minimum value.

- `CustomValueLess`
     - **Usage**: `CustomValueLess, KeyName, MaxValue`
     - **Description**: Checks if the player's custom value is less than maximum value.

- `ModInstalled`
     - **Usage**: `ModInstalled, ModName`
     - **Description**: Checks if the specified mod is installed.

- `NotModInstalled`
     - **Usage**: `NotModInstalled, ModName`
     - **Description**: Checks if the specified mod is not installed.


Please note that you can use these conditions within the player options of your dialogue entries to control the availability and visibility of options based on specific game conditions or player states.

Feel free to refer to this documentation for further clarification or provide more examples if needed.

Please note that you should replace the placeholder values (`UniqueProfileName`, `Dialogue Text`, `Player options`, `Text`, `Transition`, `Command`, `Icon`, `Condition`, `AlwaysVisible`, `QuestId`, `ItemPrefab`, `amount`, `BuffName`, `SkillName`, `MinLevel`, `somekey`) with actual values relevant to your game and dialogues.

---

### Commands

The following commands can be used in the `NpcDialogues.cfg` file:

- `OpenUI`: Opens a specific NPC type profile UI.
    - **Usage**: `OpenUI, NPC Type, Profile Name`
    - **Description**: Opens the UI associated with a particular NPC type profile.
    - **Possible NPC Types**: Marketplace, Trader, Info, Teleporter, Feedback, Banker, Gambler, Quests, Buffer, Transmog

- `PlaySound`: Plays a sound.
  - **Usage**: `PlaySound, SoundName`
  - **Description**: Plays the specified sound.

- `GiveQuest`: Gives a quest to the player.
  - **Usage**: `GiveQuest, QuestID`
  - **Description**: Gives the player the specified quest.

- `RemoveQuest`: Removes a quest from the player.
  - **Usage**: `RemoveQuest, QuestID`
  - **Description**: Removes the specified quest from the player's quest log.

- `FinishQuest`: Finishes a quest.
    - **Usage**: `FinishQuest, QuestID`
    - **Description**: Finishes the specified quest.

- `GiveItem`: Gives an item to the player.
    - **Usage**: `GiveItem, ItemPrefab, Amount, Level`
    - **Description**: Gives the player a specified number of items of a certain level.

- `RemoveItem`: Removes items from the player's inventory.
    - **Usage**: `RemoveItem, ItemPrefab, Amount`
    - **Description**: Removes a specified number of items from the player's inventory.

- `Spawn`: Spawns creatures nearby.
    - **Usage**: `Spawn, CreaturePrefab, Amount, Level`
    - **Description**: Spawns a specified number of creatures of a certain level near the player.

- `SpawnXYZ`: Spawns creatures at a specific location.
    - **Usage**: `SpawnXYZ, CreaturePrefab, Amount, Level, X, Y, Z, Radius`
    - **Description**: Spawns a specified number of creatures of a certain level at the specified coordinates.

- `Teleport`: Teleports the player to a specific location.
    - **Usage**: `Teleport, X, Y, Z`
    - **Description**: Teleports the player to the specified coordinates.

- `Damage`: Inflicts damage on the player.
    - **Usage**: `Damage, Value`
    - **Description**: Damages the player by the specified value.

- `Heal`: Restores health to the player.
    - **Usage**: `Heal, Value`
    - **Description**: Restores the player's health by the specified value.

- `GiveBuff`: Gives a buff to the player.
    - **Usage**: `GiveBuff, BuffID`
    - **Description**: Gives the player the specified buff.

- `AddPin`: Adds a pin to the player's map.
    - **Usage**: `AddPin, Name, X, Y, Z`
    - **Description**: Adds the specified pin to the player's map.

- `PingMap`: Pings a location on the player's map.
    - **Usage**: `PingMap, Name, X, Y, Z`
    - **Description**: Pings the specified location on the player's map.

- `AddEpicMMOExp`: Adds EpicMMO experience to the player.
    - **Usage**: `AddEpicMMOExp, Value`
    - **Description**: Adds the specified amount of EpicMMO experience to the player.

- `AddCozyheimExp`: Adds Cozyheim experience to the player.
    - **Usage**: `AddCozyheimExp, Value`
    - **Description**: Adds the specified amount of Cozyheim experience to the player.

- `PlayAnimation`: Plays an animation on the NPC.
    - **Usage**: `PlayAnimation, AnimationName`
    - **Description**: Plays the specified animation on the player.

- `AddCustomValue`: Adds a custom value to the player.
    - **Usage**: `AddCustomValue, KeyName, Value`
    - **Description**: Adds the specified custom value to the player.

- `SetCustomValue`: Sets a custom value for the player.
    - **Usage**: `SetCustomValue, KeyName, Value`
    - **Description**: Sets the specified custom value for the player.

- `EnterPassword`: Opens a password input dialog.
    - **Usage**: `EnterPassword, Title, Password, DialogueOpen On Success, DialogueOpen On Failure`
    - **Description**: Opens a password input dialog with the specified title and password. If the player enters the correct password, the specified dialogue will be opened. Otherwise, the specified dialogue will be opened.

Please note that you can use these commands within the player options of your dialogue entries to trigger specific actions or behaviors based on the player's choices.

You can use **multiple** commands and conditions in a single player option by separating them with | (pipe) character.

---

# Dialogue exampes:

```
[default]
Welcome to the village!
Text: Hello there! What brings you to our peaceful village?
Text: How can I assist you today?
Text: Tell me more about this village | Command: OpenUI, Info, VillageInfoProfile | Icon: village_icon
Text: I'm looking for work | Transition: JobOptions | Icon: job_icon

[JobOptions]
Available job options:
Text: We have various job opportunities available. What type of work are you interested in?
Text: Farming | Command: OpenUI, Quests, Job | Icon: Hoe | Condition: HasItem, Hoe, 1 
Text: Fishing | Command: OpenUI, Quests, FishingJob | Icon: Fish1 | Condition: SkillMore, Fishing, 10
```

Then just attach initial (in our case default) dialogue to NPC UI
