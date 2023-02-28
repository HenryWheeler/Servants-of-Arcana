using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Consumable : Component
    {
        public Consumable() { }
    }
    [Serializable]
    public class Equipable : Component
    {
        public bool equipped = false;
        public bool unequipable { get; set; }
        public string slot { get; set; }
        public Equipable(bool unequipable, string slot)
        {
            this.unequipable = unequipable;
            this.slot = slot;
        }

        public Equipable() { }
    }
}
