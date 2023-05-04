using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class SpawnDetails : Component
    {
        public Action<Room> onSpawn;
        public int minWidth { get; set; }
        public int minHeight { get; set; }
        public int maxWidth { get; set; }
        public int maxHeight { get; set; }
        public string type { get; set; } = "None";
        public Room usedRoom { get; set; }
        public override void SetDelegates() { }
        public void Invoke(Room origin)
        {
            usedRoom = origin;

            if (type != "None")
            {
                usedRoom = SpecialEffects.SpawnPrefab(origin, minWidth, minHeight, maxWidth, maxHeight, type);

                Program.dungeonGenerator.rooms.Remove(origin);
                Program.dungeonGenerator.rooms.Add(usedRoom);

                foreach (Tile tile in usedRoom.tiles)
                {
                    Program.dungeonGenerator.towerTiles.Add(tile.GetComponent<Vector>());
                }
            }

            onSpawn?.Invoke(usedRoom);
        }
        public SpawnDetails(int minWidth, int minHeight, int maxWidth, int maxHeight, string type = "None")
        {
            this.minWidth = minWidth;
            this.minHeight = minHeight;
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            this.type = type;
        }
        public SpawnDetails() { }
    }
    [Serializable]
    public class SpawnTiles : Component
    {
        public List<string> tileNames { get; set; }
        public int amountToSpawn { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<SpawnDetails>().onSpawn += Spawn;
        }
        public void Spawn(Room room)
        { 
            SpecialEffects.SpawnTiles(room, tileNames, amountToSpawn);
        }
        public SpawnTiles(List<string> tileNames, int amountToSpawn)
        {
            this.tileNames = tileNames;
            this.amountToSpawn = amountToSpawn;
        }
        public SpawnTiles() { }
    }
    [Serializable]
    public class SpawnItems : Component
    {
        public List<string> itemNames { get; set; }
        public bool randomize { get; set; } = false;
        public bool useRandomTable { get; set; } = false;
        public int amountToSpawn { get; set; }
        public string type { get; set; }
        public override void SetDelegates()
        {
            if (type == "Spawn")
            {
                entity.GetComponent<SpawnDetails>().onSpawn += Spawn;
            }
            else if (type == "Equip")
            {
                entity.GetComponent<SpawnDetails>().onSpawn += Equip;
            }
            else if (type == "Death")
            {
                entity.GetComponent<Harmable>().onDeath += Spawn;
            }
        }
        public void Spawn(Room room)
        {
            SpecialEffects.SpawnItems(room, itemNames, amountToSpawn);
        }
        public void Equip(Room room)
        {
            if (useRandomTable)
            {
                int random;
                if (randomize)
                {
                    random = Program.dungeonGenerator.seed.Next(1, amountToSpawn);
                }
                else
                {
                    random = amountToSpawn;
                }

                for (int i = 0; i < random; i++)
                {
                    InventoryManager.EquipItem(entity, JsonDataManager.ReturnEntity(RandomTableManager.RetrieveRandomSpecified(itemNames.First(), 0, true)), false);
                }
            }
            foreach (string item in itemNames)
            {
                InventoryManager.EquipItem(entity, JsonDataManager.ReturnEntity(item), false);
            }
        }
        public void Spawn(Entity attacker, Vector deathSite)
        {
            string name = RandomTableManager.RetrieveRandomSpecified($"{entity.GetComponent<Description>().name}", 0, false);
            if (name != "None")
            {
                SpecialEffects.SpawnItems(deathSite, 1, new List<string>() { name }, amountToSpawn);
            }
        }
        public SpawnItems(List<string> itemNames, int amountToSpawn)
        {
            this.itemNames = itemNames;
            this.amountToSpawn = amountToSpawn;
            type = "Spawn";
        }
        public SpawnItems(List<string> itemNames)
        {
            this.itemNames = itemNames;
            type = "Equip";
        }
        public SpawnItems(string table, int maxRandom)
        {
            this.itemNames = itemNames;
            this.amountToSpawn = maxRandom;
            randomize = true;
            useRandomTable = true;
            type = "Equip";
        }
        public SpawnItems(int amountToSpawn)
        {
            this.amountToSpawn = amountToSpawn;
            type = "Death";
        }
        public SpawnItems() { }
    }
    [Serializable]
    public class SpawnMinions : Component
    {
        public string message { get; set; }
        public List<string> minionNames { get; set; }
        public int amountToSummon { get; set; }
        public List<Entity> minions = new List<Entity>();
        public string delegateType { get; set; }
        public override void SetDelegates()
        {
            switch (delegateType)
            {
                case "Use":
                    {
                        entity.GetComponent<Usable>().onUse += Summon;
                        entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnSphere;
                        break;
                    }
                case "Death":
                    {
                        entity.GetComponent<Harmable>().onDeath += Summon;
                        break;
                    }
                case "Spawn":
                    {
                        entity.GetComponent<SpawnDetails>().onSpawn += Summon;
                        break;
                    }
            }
        }
        public void Summon(Entity user, Vector origin)
        {
            Log.Add(message);
            minions = SpecialEffects.SummonMinions(user, origin, minionNames, amountToSummon);
        }
        public void Summon(Room room)
        {
            minions = SpecialEffects.SummonMinions(entity, room, minionNames, amountToSummon);
        }
        public SpawnMinions(List<string> minionNames, int amountToSummon, string delegateType)
        {
            this.minionNames = minionNames;
            this.amountToSummon = amountToSummon;
            this.delegateType = delegateType;
        }
    }
    /// <summary>
    /// For monsters whom are composed of several different entities that compose different monsters.
    /// </summary>
    [Serializable]
    public class SpawnBodyParts : Component
    {
        public List<string> minionNames { get; set; }
        public int amountToSummon { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<SpawnDetails>().onSpawn += Summon;
        }
        public void Summon(Room room)
        {
            List<Entity> parts = SpecialEffects.SummonMinions(entity, room, minionNames, amountToSummon);
            HostAI host = entity.GetComponent<HostAI>();

            List<Entity> body = new List<Entity>();
            List<Entity> limbs = new List<Entity>();

            foreach (Entity entity in parts)
            {

            }
        }
        public SpawnBodyParts(List<string> partNames, int amountToSummon)
        {
            this.minionNames = minionNames;
            this.amountToSummon = amountToSummon;
        }
    }
    /// <summary>
    /// A component for items that summon minions when the item is equipped, and despawns the minions when the item is unequipped.
    /// </summary>
    [Serializable]
    public class BoundMinion : SpawnMinions
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Equipable>().onEquip += Summon;
        }
        public void Summon(Entity user, bool equipped)
        {
            if (equipped)
            {
                if (minionNames.Count == 0)
                {
                    Log.Add("Nothing happens.");
                }
                else
                {
                    Log.Add(message);

                    List<Vector> chosenTiles = new List<Vector>();
                    Vector startingLocation = user.GetComponent<Vector>();

                    for (int x = startingLocation.x - 3; x <= startingLocation.x + 3; x++)
                    {
                        for (int y = startingLocation.y - 3; y <= startingLocation.y + 3; y++)
                        {
                            if (Math.CheckBounds(x, y) && Program.tiles[x, y].terrainType != 0 && new Vector(x, y) != startingLocation && Program.tiles[x, y].actor == null && !chosenTiles.Contains(new Vector(x, y)))
                            {
                                chosenTiles.Add(new Vector(x, y));
                            }
                        }
                    }

                    Vector chosen = null;

                    if (chosenTiles.Count > 0)
                    {
                        chosen = chosenTiles[Program.random.Next(chosenTiles.Count)];
                    }

                    if (chosen != null)
                    {
                        minions = SpecialEffects.SummonMinions(user, chosen, minionNames, amountToSummon);
                        minionNames.Clear();
                    }
                    else
                    {
                        Log.Add("There is not enough room to summon minions.");
                    }
                }
            }
            else
            {
                foreach (Entity minion in minions)
                {
                    TurnComponent component = minion.GetComponent<TurnComponent>();
                    if (component.isAlive)
                    {
                        Vector vector = minion.GetComponent<Vector>();

                        component.isAlive = false;
                        component.isTurnActive = false;
                        TurnManager.RemoveActor(component);
                        Program.tiles[vector.x, vector.y].actor = null;

                        minionNames.Add(minion.GetComponent<Description>().name);
                    }
                }
            }
        }
        public BoundMinion(List<string> minionNames, int amountToSummon) : base(minionNames, amountToSummon, "Use") { }
    }
}
