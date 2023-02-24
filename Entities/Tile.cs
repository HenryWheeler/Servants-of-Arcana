using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Tile : Entity
    {
        public Entity actor { get; set; }
        public Entity item { get; set; }
        public int terrainType { get; set; }
        public Tile(int x, int y, Color fColor, Color bColor, char character, string name, string description, int terrainType, bool opacity) 
        {
            this.terrainType = terrainType;
            AddComponent(new Visibility(opacity));
            AddComponent(new Vector(x, y));
            AddComponent(new Draw(fColor, bColor, character));
            AddComponent(new Description(name, description));
        }
        public Tile() { }
    }
}
