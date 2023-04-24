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
                    Program.dungeonGenerator.towerTiles.Add(tile);
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
}
