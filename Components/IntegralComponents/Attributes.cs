using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class Attributes : Component
    {
        public int maxHealth { get; set; }
        public int health { get; set; }
        public float maxEnergy { get; set; }
        public int strength { get; set; }
        public int intelligence { get; set; }
        public int armorValue { get; set; }
        public int sight { get; set; }
        public Func<string> status;
        public override void SetDelegates() { }
        public Attributes(int health, float maxEnergy, int strength, int intelligence, int armorValue, int sight) 
        {
            maxHealth = health;
            this.health = health;

            this.maxEnergy = maxEnergy;
            this.strength = strength;
            this.intelligence = intelligence;

            this.armorValue = armorValue;

            this.sight = sight;
        }
    }
}
