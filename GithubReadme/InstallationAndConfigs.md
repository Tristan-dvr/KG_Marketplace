<p>

1) Ship plugin to all clients and on your dedicated server
2) After server restart, new folder in BepInEx/config will be created: Marketplace
</p>

### Since v9.0.0 folders names / structure and config deployment changed



# Client Main Config

Main BepInEx Client Config `MarketplaceAndServerNPCs.cfg`:

<details><summary>Options</summary>

```cfg
## Settings file was created by plugin MarketplaceAndServerNPCs v9.0.0
## Plugin GUID: MarketplaceAndServerNPCs

[General]

## Enable Market Local Usage
# Setting type: Boolean
# Default value: false
Use Marketplace Locally = false

# Setting type: KeyCode
# Default value: J
# Acceptable values: None, Backspace, Tab, Clear, Return, Pause, Escape, Space, Exclaim, DoubleQuote, Hash, Dollar, Percent, Ampersand, Quote, LeftParen, RightParen, Asterisk, Plus, Comma, Minus, Period, Slash, Alpha0, Alpha1, Alpha2, Alpha3, Alpha4, Alpha5, Alpha6, Alpha7, Alpha8, Alpha9, Colon, Semicolon, Less, Equals, Greater, Question, At, LeftBracket, Backslash, RightBracket, Caret, Underscore, BackQuote, A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z, LeftCurlyBracket, Pipe, RightCurlyBracket, Tilde, Delete, Keypad0, Keypad1, Keypad2, Keypad3, Keypad4, Keypad5, Keypad6, Keypad7, Keypad8, Keypad9, KeypadPeriod, KeypadDivide, KeypadMultiply, KeypadMinus, KeypadPlus, KeypadEnter, KeypadEquals, UpArrow, DownArrow, RightArrow, LeftArrow, Insert, Home, End, PageUp, PageDown, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Numlock, CapsLock, ScrollLock, RightShift, LeftShift, RightControl, LeftControl, RightAlt, LeftAlt, RightCommand, RightApple, LeftCommand, LeftApple, LeftWindows, RightWindows, AltGr, Help, Print, SysReq, Break, Menu, Mouse0, Mouse1, Mouse2, Mouse3, Mouse4, Mouse5, Mouse6, JoystickButton0, JoystickButton1, JoystickButton2, JoystickButton3, JoystickButton4, JoystickButton5, JoystickButton6, JoystickButton7, JoystickButton8, JoystickButton9, JoystickButton10, JoystickButton11, JoystickButton12, JoystickButton13, JoystickButton14, JoystickButton15, JoystickButton16, JoystickButton17, JoystickButton18, JoystickButton19, Joystick1Button0, Joystick1Button1, Joystick1Button2, Joystick1Button3, Joystick1Button4, Joystick1Button5, Joystick1Button6, Joystick1Button7, Joystick1Button8, Joystick1Button9, Joystick1Button10, Joystick1Button11, Joystick1Button12, Joystick1Button13, Joystick1Button14, Joystick1Button15, Joystick1Button16, Joystick1Button17, Joystick1Button18, Joystick1Button19, Joystick2Button0, Joystick2Button1, Joystick2Button2, Joystick2Button3, Joystick2Button4, Joystick2Button5, Joystick2Button6, Joystick2Button7, Joystick2Button8, Joystick2Button9, Joystick2Button10, Joystick2Button11, Joystick2Button12, Joystick2Button13, Joystick2Button14, Joystick2Button15, Joystick2Button16, Joystick2Button17, Joystick2Button18, Joystick2Button19, Joystick3Button0, Joystick3Button1, Joystick3Button2, Joystick3Button3, Joystick3Button4, Joystick3Button5, Joystick3Button6, Joystick3Button7, Joystick3Button8, Joystick3Button9, Joystick3Button10, Joystick3Button11, Joystick3Button12, Joystick3Button13, Joystick3Button14, Joystick3Button15, Joystick3Button16, Joystick3Button17, Joystick3Button18, Joystick3Button19, Joystick4Button0, Joystick4Button1, Joystick4Button2, Joystick4Button3, Joystick4Button4, Joystick4Button5, Joystick4Button6, Joystick4Button7, Joystick4Button8, Joystick4Button9, Joystick4Button10, Joystick4Button11, Joystick4Button12, Joystick4Button13, Joystick4Button14, Joystick4Button15, Joystick4Button16, Joystick4Button17, Joystick4Button18, Joystick4Button19, Joystick5Button0, Joystick5Button1, Joystick5Button2, Joystick5Button3, Joystick5Button4, Joystick5Button5, Joystick5Button6, Joystick5Button7, Joystick5Button8, Joystick5Button9, Joystick5Button10, Joystick5Button11, Joystick5Button12, Joystick5Button13, Joystick5Button14, Joystick5Button15, Joystick5Button16, Joystick5Button17, Joystick5Button18, Joystick5Button19, Joystick6Button0, Joystick6Button1, Joystick6Button2, Joystick6Button3, Joystick6Button4, Joystick6Button5, Joystick6Button6, Joystick6Button7, Joystick6Button8, Joystick6Button9, Joystick6Button10, Joystick6Button11, Joystick6Button12, Joystick6Button13, Joystick6Button14, Joystick6Button15, Joystick6Button16, Joystick6Button17, Joystick6Button18, Joystick6Button19, Joystick7Button0, Joystick7Button1, Joystick7Button2, Joystick7Button3, Joystick7Button4, Joystick7Button5, Joystick7Button6, Joystick7Button7, Joystick7Button8, Joystick7Button9, Joystick7Button10, Joystick7Button11, Joystick7Button12, Joystick7Button13, Joystick7Button14, Joystick7Button15, Joystick7Button16, Joystick7Button17, Joystick7Button18, Joystick7Button19, Joystick8Button0, Joystick8Button1, Joystick8Button2, Joystick8Button3, Joystick8Button4, Joystick8Button5, Joystick8Button6, Joystick8Button7, Joystick8Button8, Joystick8Button9, Joystick8Button10, Joystick8Button11, Joystick8Button12, Joystick8Button13, Joystick8Button14, Joystick8Button15, Joystick8Button16, Joystick8Button17, Joystick8Button18, Joystick8Button19
Quest Journal Keycode = J

# Setting type: Boolean
# Default value: false
Mute Gambler Sounds = false

[KG Chat]

## KG Chat Font Size
# Setting type: Int32
# Default value: 18
Font Size = 18

## Use KG Chat Type Sound
# Setting type: Boolean
# Default value: false
Use Type Sound = false

## KG Chat Transparency
# Setting type: Transparency
# Default value: Two
# Acceptable values: None, One, Two, Three, Four, Five
Transparency = Two

[Marketplace]

## Market size
# Setting type: MarketSize
# Default value: Large
# Acceptable values: Large, Medium, Small
Market Size = Large
```
</details>

If you want to use mod in singleplayer set `Use Marketplace Locally` to `true`. It will generate server-related configs on client-sided and you will be able to use most of marketplace modules in singleplayer.

Currently avaliable modules to use in singleplayer:
1) Buffer
2) DistancedUI
3) Gambler
4) Dialogues
5) Trader
6) Quests
7) Territories
8) Teleporters
9) Transmog
10) ServerInfo
11) PlayerTags
12) KG_Chat

What is not avaliable in singleplayer:
1) Banker (data saved on server)
2) Marketplaces (data saved on server)
3) Leaderboards (data saved on server)


# Marketplace data folders / configs

Server:

![](https://i.imgur.com/KNm9w56.png)

1) Marketplace folder - contains all main configs and settings (Folder only exist on server or if client works in server mode)

Client-only folders / files:

![](https://i.imgur.com/NdYsvEV.png)

1) Marketplace folder - contains all main configs and settings (Folder only exist on server or if client works in server mode)
2) Marketplace_CachedImages - put .png images here to use them in your custom quests / dialogues / other systems by name
3) Marketplace_Sounds - put .mp3 sounds here to use them in your custom quests / dialogues / other systems by name
4) Marketplace_SavedNPCs - contains .yml with saved NPCs you can build with hammer. Can be shared
5) Marketplace_KGChat_Emojis - contains emoji spritesheets for KG_Chat so you can add your own emojis


# Marketplace folder structure

![](https://i.imgur.com/e1GENpv.png)

1) Configs - folder that contains subfolders for all possible marketplace synced modules / mechanics:

![](https://i.imgur.com/lrLPgZS.png)

You can create subfolder in modules and create your own FILENAME.cfg to fill it with your own data / entries

2) DiscordWebhooks - folder contains DiscordSettings.cfg that has discord webhook settings for hooking events such as Marketplace item post, quest complete and so on
3) DistancedUI - folder contains DistancedUI.cfg that has settings for DistancedUI module that allows you to use mechanics in separated UI in left side of screen without NPC
4) PlayerTags - folder contains PlayerTags.cfg that has entries for PlayerTags module that allows you to add tags to players
5) SavedData_JSON - folder contains all saved data in JSON format, such as: marketplace saved slots, banker entries and so on. You can only change it when server is offline. Keep in mind that if you change it, something can break, so be careful
6) MarketPlace.cfg - main config that contains all settings for marketplace itself

<details><summary>Options</summary>

```cfg
[Main]

## Enable/Disable Transmog Log
# Setting type: Boolean
# Default value: false
EnableTransmogLog = false

## Enable/Disable Trader Log
# Setting type: Boolean
# Default value: false
EnableTraderLog = false

## Banker Income Time (hours)
# Setting type: Int32
# Default value: 1
BankerIncomeTime = 1

## Banker Income Multiplier (per time)
# Setting type: Single
# Default value: 0
BankerIncomeMultiplier = 0

## VIP Banker Income Multiplier
# Setting type: Single
# Default value: 0
BankerVIPIncomeMultiplier = 0

## Feedback Webhook Link
# Setting type: String
# Default value: webhook link
FeedbackWebhookLink = webhook link

## Banker Interest Items
# Setting type: String
# Default value: All
BankerInterestItems = All

## Limit amount of slots player can sell in marketpalce
# Setting type: Int32
# Default value: 15
ItemMarketLimit = 15

## Marketplace Blocked Players
# Setting type: String
# Default value: User IDs
BlockedPlayers = User IDs

## Marketplace VIP Players List 
# Setting type: String
# Default value: User IDs
VIPplayersList = User IDs

## Market Taxes From Each Sell
# Setting type: Int32
# Default value: 0
MarketTaxes = 0

## VIP Player Market Taxes
# Setting type: Int32
# Default value: 0
VIPplayersTaxes = 0

## Enable/Disable players teleporter with ore
# Setting type: Boolean
# Default value: true
CanTeleportWithOre = true

## Marketplace Blocked Prefabs For Selling
# Setting type: String
# Default value: Coins, SwordCheat
MarketSellBlockedPrefabs = Coins, SwordCheat

## Prefab Of Server Currency (marketplace)
# Setting type: String
# Default value: Coins
ServerCurrency = Coins

## Enable Gambler Win Notification
# Setting type: Boolean
# Default value: false
GamblerEnableWinNotifications = false

## Enable Kill / Harvest Craft Same Target Quests Get + Score In Same Time
# Setting type: Boolean
# Default value: false
AllowMultipleQuestsScore = false

## Max Amount Of Accpeted Quests
# Setting type: Int32
# Default value: 7
MaxAcceptedQuests = 7

## Hide Quest in UI if they have OtherQuest as requirement
# Setting type: Boolean
# Default value: false
HideOtherQuestRequirementQuests = false

## Allow Kill Quests In Party
# Setting type: Boolean
# Default value: true
AllowKillQuestsInParty = true

## Enable KGChat
# Setting type: Boolean
# Default value: true
EnableKGChat = true

## Recipe for Mailpost creation
# Setting type: String
# Default value: SwordCheat,1
MailPostRecipe = SwordCheat,1

## Mailpost wait time (minutes)
# Setting type: Int32
# Default value: 5
MailPostWaitTime = 5

## Mailpost exclude items (with coma)
# Setting type: String
# Default value: Items Here
MailPostExcludeItems = Items Here

## Recipe for Piece Saver Crystal creation
# Setting type: String
# Default value: SwordCheat,1
PieceSaverRecipe = SwordCheat,1

## Use Leaderboard
# Setting type: Boolean
# Default value: false
UseLeaderboard = false

## Rebuild Heightmap On Territory Change
# Setting type: Boolean
# Default value: false
RebuildHeightmap = false
```
</details>


That's all for folders / configs structures. Now you can create your own config files and fill it with custom data

