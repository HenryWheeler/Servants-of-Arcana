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
        public override void SetDelegates() { }
        public Consumable() { }
    }
    public class Usable : Component
    {
        public Action<Entity, Vector> onUse;
        public int range { get; set; }
        public override void SetDelegates() { }
        public void Use(Entity user, Vector target)
        {
            onUse?.Invoke(user, target);

            if (entity.GetComponent<Consumable>() != null)
            {
                //Remove from inventory and such later
                onUse = null;
            }
        }
        public Usable(int range) 
        {
            this.range = range;
        }
        public Usable() { }
    }
    [Serializable]
    public class Equipable : Component
    {
        public bool equipped = false;
        public bool unequipable { get; set; }
        public string slot { get; set; }
        public override void SetDelegates() { }
        public Equipable(bool unequipable, string slot)
        {
            this.unequipable = unequipable;
            this.slot = slot;
        }
        public Equipable() { }
    }
}
