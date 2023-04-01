using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Event : Entity
    {
        public Event(int x, int y, List<Component> components, int width = 0, int height = 0, string type = "None") 
        {
            AddComponent(new Vector(x, y));
            if (width != 0 && height != 0 && type != "None")
            {
                AddComponent(new SpawnDetails(width, height, type));
            }
            else
            {
                AddComponent(new SpawnDetails());
            }

            foreach (Component component in components)
            {
                if (component != null)
                {
                    AddComponent(component);
                }
            }
        }
        public Event(List<Component> components)
        {
            foreach (Component component in components)
            {
                if (component != null)
                {
                    AddComponent(component);
                }
            }
        }
        public Event() { }
    }
}
