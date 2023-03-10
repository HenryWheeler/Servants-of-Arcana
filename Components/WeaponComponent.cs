using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class WeaponComponent : Component
    {
        public int toHitBonus { get; set; }
        public int damageBonus { get; set; }
        public int damageDie1 { get; set; }
        public int damageDie2 { get; set; }
        public override void SetDelegates() { }
        public WeaponComponent(int toHitBonus, int damageBonus, int damageDie1, int damageDie2) 
        {
            this.toHitBonus = toHitBonus;
            this.damageBonus = damageBonus;
            this.damageDie1 = damageDie1;
            this.damageDie2 = damageDie2;
        }
        public WeaponComponent() { }
    }
}
