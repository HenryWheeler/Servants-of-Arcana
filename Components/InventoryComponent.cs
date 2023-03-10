using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class InventoryComponent : Component
    {
        public List<Entity> items = new List<Entity>();
        public EquipmentSlot[] equipment = new EquipmentSlot[4];
        public EquipmentSlot ReturnSlot(string slotName)
        {
            foreach (EquipmentSlot slot in equipment)
            {
                if (slot.slotName == slotName) { return slot; }
            }
            return null;
        }
        public override void SetDelegates() { }
        public InventoryComponent() 
        {
            equipment[0] = new EquipmentSlot("Armor");
            equipment[1] = new EquipmentSlot("Off Hand");
            equipment[2] = new EquipmentSlot("Magic Item");
            equipment[3] = new EquipmentSlot("Weapon");
            items = new List<Entity>();
        }
    }
    [Serializable]
    public class EquipmentSlot
    {
        public Entity item { get; set; }
        public string slotName { get; set; }
        public EquipmentSlot(string slotName)
        {
            this.slotName = slotName;
        }
        public EquipmentSlot() { } 
    }
}
