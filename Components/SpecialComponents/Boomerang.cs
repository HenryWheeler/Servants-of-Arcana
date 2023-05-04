using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Boomerang : Component
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += Throw;
        }
        public void Throw(Entity user, Vector origin)
        {
            SpecialEffects.Boomerang(user, origin, entity);
        }
        public Boomerang() { }
    }
}
