using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Lightning : Component
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
            SpecialEffects.Lightning(user, origin, strength, entity.GetComponent<Usable>().range, "Lightning");
        }
        public Lightning(int strength)
        {
            this.strength = strength;
        }
    }
}
