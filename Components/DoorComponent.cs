using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class DoorComponent : Component
    {
        public bool open { get; set; } = false;
        public void ChangeDoorState()
        {
            if (open) 
            {
                open = false;

                entity.GetComponent<Draw>().character = '+';
                entity.GetComponent<Visibility>().opaque = true;
            }
            else
            {
                open = true;

                entity.GetComponent<Draw>().character = '-';
                entity.GetComponent<Visibility>().opaque = false;
            }
        }
    }
}
