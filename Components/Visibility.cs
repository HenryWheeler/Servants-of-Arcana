using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Visibility : Component
    {
        public bool opaque { get; set; }
        public bool visible { get; set; } = false;
        public bool explored { get; set; } = false;
        public void SetVisible(bool visibility) 
        {
            if (visibility) 
            {
                visible = true; 
                explored = true;
            }
            else 
            {
                visible = false;
            }
        }
        public Visibility(bool opaque) 
        {
            this.opaque = opaque; 
        }
        public Visibility() { }
    }
}
