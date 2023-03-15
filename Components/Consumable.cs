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
        public string action { get; set; }
        public override void SetDelegates() { }
        public void Use(Entity user, Vector target)
        {
            onUse?.Invoke(user, target);

            if (entity.GetComponent<Consumable>() != null)
            {
                onUse = null;
            }
        }
        public Usable(int range, string action) 
        {
            this.range = range;
            this.action = action;
        }
        public Usable() { }
    }
    [Serializable]
    public class Equipable : Component
    {
        public bool equipped = false;
        public bool removable { get; set; }
        public string slot { get; set; }
        public override void SetDelegates() { }
        public Equipable(bool removable, string slot)
        {
            this.removable = removable;
            this.slot = slot;
        }
        public Equipable() { }
    }
}
