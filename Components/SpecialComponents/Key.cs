using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Key : Component
    {
        public override void SetDelegates()
        {
            entity.GetComponent<Usable>().onUse += Unlock;
        }
        public void Unlock(Entity entity, Vector vector)
        {
            Program.dungeonGenerator.staircaseUnlocked = true;
        }
        public Key() { }
    }
}
