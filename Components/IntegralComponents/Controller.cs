using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public abstract class Controller : Component
    {
        public override void SetDelegates() { }
        public abstract void Execute();
    }
}
