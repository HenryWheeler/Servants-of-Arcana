using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Trap : Component
    {
        public Action<Entity> onStep;
        public void TriggerTrap(Entity entity)
        {
            onStep?.Invoke(entity);
        }
        public override void SetDelegates() { }
        public Trap() { }
    }
}
