using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Transpose : Component
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += Teleport;
            entity.GetComponent<Usable>().areaOfEffect += AreaOfEffectModels.ReturnTwoPoints;
        }
        public void Teleport(Entity user, Vector origin)
        {
            SpecialEffects.Transpose(user, origin);
        }
        public Transpose() { }
    }
}
