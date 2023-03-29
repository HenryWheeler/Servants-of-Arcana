using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class SpawnDetails : Component
    {
        public Action onSpawn;
        public override void SetDelegates() { }
        public SpawnDetails() { }
    }
}
