using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class Attributes : Component
    {
        public float maxEnergy { get; set; }
        public Attributes(float maxEnergy) 
        {
            this.maxEnergy = maxEnergy;
        }
    }
}
