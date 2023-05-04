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
        public override void SetDelegates()
        {
            entity.GetComponent<Trap>().onStep += OpenDoor;
        }
        public void OpenDoor(Entity stepper)
        {
            if (stepper.GetComponent<PlayerController>() != null)
            {
                Log.Add($"{Program.playerName} opens the {entity.GetComponent<Description>().name}.");
            }
            if (!open)
            {
                open = true;

                entity.GetComponent<Draw>().character = '-';
                entity.GetComponent<Visibility>().opaque = false;
            }
        }
    }
}
