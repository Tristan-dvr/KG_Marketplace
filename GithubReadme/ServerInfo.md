NPC will read info from  `Valheim\BepInEx\config\Marketplace\Configs\ServerInfos` config files and display that on GUI.
Rich text markers can be applied to text you write
ServerInfo npc uses "default" profile by default. But you can add as many info profiles you want (same as Trader NPC profiles). Example below:

![](https://i.imgur.com/JSZ90if.png)

![](https://i.imgur.com/cwOiOsO.png)

![](https://i.imgur.com/MfZXnVH.png)

To add data you need to create profile with [ProfileName], and then uder it you can write info you need. Later just assign this profile to Info NPC and it will show it.
Non-profiled text will be applied to every new Info NPC with "default" profile.