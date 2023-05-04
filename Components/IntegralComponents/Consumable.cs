using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Charges : Component
    {
        public int chargesRemaining { get; set; }
        public override void SetDelegates() { }
        public Charges(int chargesRemaining) 
        {
            this.chargesRemaining = chargesRemaining;
        }
        public Charges() { }
    }
    public class Usable : Component
    {
        public Action<Entity, Vector> onUse;
        public Func<Vector, Vector, int, List<Vector>> areaOfEffect;
        public int range { get; set; }
        public int strength { get; set; }
        public int requiredTargets { get; set; }
        public List<int> tileTypes = new List<int>();
        public string action { get; set; }
        public string message { get; set; }
        public override void SetDelegates() { }
        public void Use(Entity user, Vector target)
        {
            Log.Add($"{user.GetComponent<Description>().name} {message}");
            onUse?.Invoke(user, target);
        }
        public Usable(int range, int strength, int requiredTargets, string action, string message, List<int> tileTypes = null)
        {
            this.range = range;
            this.strength = strength;
            this.action = action;
            this.message = message;
            this.requiredTargets = requiredTargets;
            if (tileTypes != null)
            {
                this.tileTypes = tileTypes;
            }
        }
        public Usable() { }
    }
    [Serializable]
    public class Equipable : Component
    {
        public bool equipped = false;
        public bool removable { get; set; }
        public string slot { get; set; }
        /// <summary>
        /// If set to true the action will be when the item is equipped, when set to false it will be when the item is unequipped
        /// </summary>
        public Action<Entity, bool> onEquip;
        public override void SetDelegates() { }
        public Equipable(bool removable, string slot)
        {
            this.removable = removable;
            this.slot = slot;
        }
        public Equipable() { }
    }
}
