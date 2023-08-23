
Quest Events allows you to "attach" events and actions to particular quests created in `Valheim\BepInEx\config\Marketplace\Configs\QuestEvents`

Simply create ANYFILENAME.cfg and fill it with data.

Possible events:
```
OnAcceptQuest - when player accepts quest
OnCancelQuest - when player cancels quest
OnCompleteQuest - when player completes quest (successfully)
```

Possible actions:
```
GiveItem - example: GiveItem, SwordIron, 1, 5. Will give player 1 Iron Sword level 5
GiveQuest - example: GiveQuest, MyQuestID123. Will give player quest with ID MyQuestID123
RemoveQuest - example: RemoveQuest, MyQuestID123. Will remove quest with ID MyQuestID123
Spawn - example: Spawn, Wolf, 5, 2. Will spawn 5 wolves level 2 (near)
Teleport - example: Teleport, 100, 100, 100. Will teleport player to x100, y100, z100
Damage - example: Damage, 100. Will deal 100 damage to player
Heal - example: Heal, 100. Will heal player for 100 health
PlaySound - example: PlaySound, MySound. Will play sound MySound
NpcText - example: NpcText, MyText. Will show text MyText above closest NPC head
```

Data Format:
```
[questID]
Event: Action, arguments
```

Example:
![](https://i.imgur.com/Qcp98Rx.png)

You are not limited in using one event and action once, you can add as many same events as you want to with different actions, example:

```
[TestQuest]
OnAcceptQuest: GiveItem, SwordIron, 1, 5
OnAcceptQuest: GiveItem, Coins, 100, 1
OnAcceptQuest: Heal, 5000
```
