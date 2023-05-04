using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class PatrolComponent : Component
    {
        public int currentRoute { get; set; } = 1;
        public Vector lastPosition { get; set; }
        public override void SetDelegates() { }
        public PatrolComponent() { }
    }
}
