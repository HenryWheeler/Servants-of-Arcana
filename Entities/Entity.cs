using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Entity
    {
        public List<Component> collectionsToRemove = new List<Component>();
        public List<Component> components = new List<Component>();
        public void AddComponent(Component component)
        {
            if (component != null && !components.Contains(component))
            {
                components.Add(component);
                component.entity = this;
            }
        }
        public void RemoveComponent(Component component)
        {
            if (component != null)
            {
                components.Remove(component);
                component.entity = null;
            }
        }
        public T GetComponent<T>() where T : Component
        {
            foreach (Component component in components)
            {
                if (component.GetType().Equals(typeof(T)))
                {
                    return (T)component;
                }
            }
            return null;
        }
        public void SetDelegates()
        {
            foreach (Component component in components)
            {
                if (component != null)
                {
                    component.SetDelegates();
                }
            }
        }
        public Entity(List<Component> components)
        {
            foreach (Component component in components)
            {
                if (component != null)
                {
                    AddComponent(component);
                }
            }
        }
        public Entity() { }
    }
}
