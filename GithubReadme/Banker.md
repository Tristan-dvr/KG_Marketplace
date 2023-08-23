
Banker is an NPC that allows you to deposit / withdraw your items in bank. Also if you set Banker Income and Banker Income Time in Marketplace.cfg then each N hours (Banker Income Time) every player will get % Income to their bank resources.

To create a Banker profile go to `Valheim\BepInEx\config\Marketplace\Configs\Bankers` and simply create ANYFILENAME.cfg and fill it with data.

![](https://i.imgur.com/n7TZqfI.png)

I want to make a Banker profile that will accept Coins + Rubies. For that i would need to add profile [profileName] and add acceptable items on each new line

![](https://i.imgur.com/Zt1lTbw.png)

Let's assign Banker profile to our Banker NPC in-game:

![](https://i.imgur.com/dQriWbn.png)

On Interact with NPC you should see this:

![](https://i.imgur.com/KlarEFR.png)

Green number = resource amount in bank account. Bottom text = inventory amount

So if i want to deposit (put) 250 coins into bank i would need to write "250" and press "+" :

![](https://i.imgur.com/f22k5fQ.png)

![](https://i.imgur.com/SFOAvma.png)

As you can see now i have 250 coins in bank that will be kept there forever and getting income if server admin wants to be so

You may have multiple banker NPCs with different slots (resources) to keep your items in. For example you can have 1 banker that will keep your coins and another one that will keep your rubies

Think about banker as a "global" big chest with infinite space :D

