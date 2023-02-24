using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Component
    {
        public Entity entity { get; set; }
        public Component() { }
    }
}
