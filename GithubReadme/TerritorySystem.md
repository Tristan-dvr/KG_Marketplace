
The `TerritoryDatabase.cfg` file is used to define territories or zones within your game world. Each zone can have specific attributes such as shape, position, size, color, flags, and owners. This guide will help you understand the format and options available in the configuration file.

Territory configs should be created in `Valheim\BepInEx\config\Marketplace\Configs\Territories` folder. Simply create ANYFILENAME.cfg and fill it with data.

## Format

The configuration file follows the following format:

```plaintext
[ZoneName]
Shape type (Circle, Square, Custom)
X pos, Z pos, Radius (for circle/square) or X pos, Z pos, X length, Z length (for custom zone)
Red Color, Green Color, Blue Color, Show Territory on water (True/False)
Zone Flags (separated by comma if multiple)
Owners SteamID (separated by comma if multiple)
```

## Zone Attributes

### Zone Name

Each zone entry begins with a unique `ZoneName`. This identifier is used to differentiate between different zones in the configuration file.

### Shape Type

The shape of the zone can be specified as one of the following:

- Circle: The zone is defined as a circle with a center point and a radius.
- Square: The zone is defined as a square with a center point and side length.
- Custom: The zone is defined with custom dimensions using the X and Z position coordinates, along with the X and Z lengths.

### Position and Size

Depending on the shape type, you need to specify the position and size of the zone:

- For a circle or square, provide the X and Z position coordinates and the radius (for a circle) or side length (for a square).
- For a custom zone, provide the X and Z position coordinates, as well as the X and Z lengths.

### Color and Show Territory on Water

Specify the color of the zone using RGB values (Red, Green, Blue). Additionally, indicate whether the territory should be visible on water by specifying `True` or `False` after the RGB color values.

### Zone Flags

You can assign specific flags to a zone to define its behavior and characteristics. Multiple flags can be assigned to a zone, separated by commas. Here are the available flags:

- `PushAway`: Players are pushed away from the zone boundaries.
- `NoBuild`: Building structures is not allowed within the zone.
- `NoPickaxe`: Players cannot use pickaxes within the zone.
- `NoInteract`: Interactions with objects or NPCs within the zone are disabled.
- `NoAttack`: Players cannot initiate attacks or engage in combat within the zone.
- `PvpOnly`: Forces PvP mode within the zone.
- `PveOnly`:  Forces PvE mode within the zone.
- `PeriodicHeal`: Players are periodically healed while inside the zone (only zone owners).
- `PeriodicDamage`: Players receive periodic damage while inside the zone.
- `IncreasedPlayerDamage`: Player attacks deal increased damage within the zone.
- `IncreasedMonsterDamage`: Monsters deal increased damage to players within the zone.
- `NoMonsters`: Monsters do not spawn or exist within the zone.
- `CustomEnvironment`: The zone has a custom environment specified by the environment name.
- `MoveSpeedMultiplier`: Players' movement speed is multiplied by a certain factor within the zone.
- `NoDeathPenalty`: Players do not suffer penalties upon death within the zone.
- `NoPortals`: Teleportation portals cannot be used within the zone.
- `PeriodicHealALL`: All players are periodically healed within the zone.
- `ForceGroundHeight`: The ground height is forcefully set within the zone.
- `ForceBiome`: The biome within the zone is forcefully set.
- `AddGroundHeight`: Additional ground height is added within the zone.
- `NoBuildDamage`: Structures within the zone do not take damage.
- `MonstersAddStars`: Monsters within the zone have additional stars, indicating higher difficulty.
- `InfiniteFuel`: Fuel consumption is disabled within the zone.
- `NoInteractItems`: Interactions with items within the zone are disabled.
- `NoInteractCraftingStation`: Interactions with crafting stations within the zone are disabled.
- `NoInteractItemStands`: Interactions with item stands within the zone are disabled.
- `NoInteractChests`: Interactions with chests within the zone are disabled.
- `NoInteractDoors`: Interactions with doors within the zone are disabled.
- `NoStructureSupport`: Structures within the zone do not get damaged if they are not supported.
- `NoInteractPortals`: Interactions with portals within the zone are disabled.
- `CustomPaint`: The zone has custom paint applied to it.
- `LimitZoneHeight`: The minimum height of the zone is limited.
- `NoItemLoss`: Players do not lose items upon death within the zone.
- `SnowMask`: A snow mask effect is applied within the zone.
- `NoMist`: Mist weather effects are disabled within the zone.
- `InfiniteEitr`: Eitr consumption is disabled within the zone.
- `InfiniteStamina`: Stamina consumption is disabled within the zone.
- `DropMultiplier`: The drop rate of items is multiplied by a certain factor within the zone.
- `ForceWind`: Force wind strength within the zone.

**Note:** For the `CustomEnvironment`, `PeriodicDamage`, `PeriodicHealALL`, `PeriodicHeal`, `IncreasedMonsterDamage`, `IncreasedPlayerDamage`, `MoveSpeedMultiplier`, `ForceGroundHeight`, `AddGroundHeight`, `LimitZoneHeight`, `ForceBiome`, `MonstersAddStars`, `ForceWind`, `DropMultiplier` and `CustomPaint` flags, the flag should be followed by = and the value of the flag. For example, `CustomEnvironment = Clear` or `PeriodicDamage = 10`.

`ForceBiome` accepts values:
```
Meadows = 1,
Swamp = 2,
Mountain = 4,
BlackForest = 8,
Plains = 16,
AshLands = 32,
DeepNorth = 64,
Ocean = 256,
Mistlands = 512
```

(`ForceBiome = 2` will force the biome to be swamp)

`CustomPaint` accepts values:
```
Paved = 0,
Grass = 1,
Cultivated = 2,
Dirt = 3
```

(`CustomPaint = 2` will paint the zone with the Cultivated texture)

### Owners

Specify the SteamIDs of the owners of the zone. If there are multiple owners, separate their SteamIDs with commas.

## Example

Here's an example entry in the `TerritoryDatabase.cfg` file:

```plaintext
[ExampleZone]
Square
150, 100, 800
0, 128, 255, false
NoBuild, NoInteract, PeriodicHealALL = 50
None


[ZoneWithHigherPriority@2]
Square
150, 100, 400
255, 0, 0, false
CustomEnvironment = Clear, NoAttack, NoPickaxe, PeriodicDamage = 10
None

```

All zones by default having priority 1. If you want to change priority of zone, you need to add `@` and priority number after zone name. For example, `ZoneWithHigherPriority@2` will have priority 2.
That will allow you to create zones inside zones. For example, you can create a zone with priority 1 and then create a zone with priority 2 inside it.
