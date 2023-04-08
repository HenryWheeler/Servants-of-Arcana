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
}
