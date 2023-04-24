using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Dig : Component
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += SpecialEffects.Dig;
            entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnLine;
        }
        public Dig() { }
    }
}
