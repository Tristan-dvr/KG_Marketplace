namespace Marketplace.Modules.Leaderboard;

public static class Leaderboard_DataTypes
{
    internal static readonly CustomSyncedValue<Dictionary<string, Client_Leaderboard>> SyncedClientLeaderboard =
        new(Marketplace.configSync, "clientLeaderboard", new());
    
    internal static readonly CustomSyncedValue<List<Client_Title>> SyncedClientTitles =
        new(Marketplace.configSync, "clientTitles", new());

    public static List<Title> AllTitles = new();
    internal static Dictionary<string, Player_Leaderboard> ServersidePlayersLeaderboard = new();

    internal class Player_Leaderboard
    {
        public Dictionary<string, int> KilledCreatures = new();
        public Dictionary<string, int> BuiltStructures = new();
        public Dictionary<string, int> ItemsCrafted = new();
        public Dictionary<string, int> KilledBy = new();
        public float MapExplored;
        public int DeathAmount;
        public string PlayerName;

        public int KilledPlayers => KilledCreatures.TryGetValue("Player", out var amount) ? amount : 0;
    }

    public enum TriggerType
    {
        MonstersKilled,
        ItemsCrafted,
        StructuresBuilt,
        Died,
        KilledBy,
        Explored,
        PlayersKilled = 100
    }

    public class Title
    {
        public int ID;
        public string Name;
        public string Description;
        public string Prefab;
        public int MinAmount;
        public Color32 Color;
        public int Score;
        public TriggerType Type;

        public bool Check(string id)
        {
            if (!ServersidePlayersLeaderboard.ContainsKey(id)) return false;
            Player_Leaderboard player = ServersidePlayersLeaderboard[id];
            return Type switch
            {
                TriggerType.MonstersKilled => player.KilledCreatures.TryGetValue(Prefab, out var amount) &&
                                              amount >= MinAmount,
                TriggerType.ItemsCrafted => player.ItemsCrafted.TryGetValue(Prefab, out var amount) &&
                                            amount >= MinAmount,
                TriggerType.StructuresBuilt => player.BuiltStructures.TryGetValue(Prefab, out var amount) &&
                                               amount >= MinAmount,
                TriggerType.KilledBy => player.KilledBy.TryGetValue(Prefab, out var amount) && amount >= MinAmount,
                TriggerType.Explored => player.MapExplored >= MinAmount,
                TriggerType.Died => player.DeathAmount >= MinAmount,
                TriggerType.PlayersKilled => player.KilledPlayers >= MinAmount,
                _ => false
            };
        }
    }

    internal class Client_Leaderboard : ISerializableParameter
    {
        public string PlayerName;
        public int KilledCreatures;
        public int KilledPlayers;
        public int BuiltStructures;
        public int ItemsCrafted;
        public int Died;
        public float MapExplored;
        public List<int> Titles;
        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(PlayerName ?? "");
            pkg.Write(KilledCreatures);
            pkg.Write(KilledPlayers);
            pkg.Write(BuiltStructures);
            pkg.Write(ItemsCrafted);
            pkg.Write(Died);
            pkg.Write(MapExplored);
            pkg.Write(Titles.Count);
            foreach (var title in Titles)
            {
                pkg.Write(title);
            }
        }

        public void Deserialize(ref ZPackage pkg)
        {
            PlayerName = pkg.ReadString();
            KilledCreatures = pkg.ReadInt();
            KilledPlayers = pkg.ReadInt();
            BuiltStructures = pkg.ReadInt();
            ItemsCrafted = pkg.ReadInt();
            Died = pkg.ReadInt();
            MapExplored = pkg.ReadSingle();
            int count = pkg.ReadInt();
            Titles = new();
            for (int i = 0; i < count; i++)
            {
                Titles.Add(pkg.ReadInt());
            }
        }
    }


    internal class Client_Title : ISerializableParameter
    {
        public int ID;
        public string Name;
        public int Score;
        public string Description;
        public Color32 Color;

        public void Serialize(ref ZPackage pkg)
        {
            pkg.Write(Name ?? "");
            pkg.Write(Score);
            pkg.Write(Description ?? "");
            pkg.Write(global::Utils.ColorToVec3(Color));
            pkg.Write(ID);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            Name = pkg.ReadString();
            Score = pkg.ReadInt();
            Description = pkg.ReadString();
            Color = global::Utils.Vec3ToColor(pkg.ReadVector3());
            ID = pkg.ReadInt();
        }
    }
}