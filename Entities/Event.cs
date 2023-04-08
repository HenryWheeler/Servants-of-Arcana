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
        public Event(int x, int y, List<Component> components, int minWidth = 0, int minHeight = 0, int maxWidth = 0, int maxHeight = 0, string type = "None") 
        {
            AddComponent(new Vector(x, y));
            if (minWidth != 0 && minHeight != 0 && maxWidth != 0 && maxHeight != 0 && type != "None")
            {
                AddComponent(new SpawnDetails(minWidth, minHeight, maxWidth, maxHeight, type));
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
