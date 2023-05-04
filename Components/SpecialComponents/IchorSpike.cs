using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class IchorSpike : Component
    {
        public int strength { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += Strike;
            entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnLine;
            strength = entity.GetComponent<Usable>().strength;
        }
        public void Strike(Entity user, Vector origin)
        {
            SpecialEffects.PhysicalProjectile(user, origin, strength, entity.GetComponent<Usable>().range, "Ichor Spike", Color.Purple);
        }
        public IchorSpike(int strength)
        {
            this.strength = strength;
        }
    }
}
