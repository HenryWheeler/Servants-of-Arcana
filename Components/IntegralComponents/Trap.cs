﻿using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Trap : Component
    {
        public Action<Entity> onStep;
        public Draw defaultDraw = null;
        public bool revealed = true;
        public bool set { get; set; } = true;
        public void TriggerTrap(Entity entity)
        {
            onStep?.Invoke(entity);

            if (set)
            {
                if (defaultDraw != null && onStep != null)
                {
                    Reveal();

                    this.entity.GetComponent<Description>().description += " + The trap has been sprung.";
                }

                onStep = null;
            }
        }
        public void Reveal()
        {
            if (!revealed)
            {
                Draw draw = entity.GetComponent<Draw>();
                draw.fColor = defaultDraw.fColor;
                draw.bColor = defaultDraw.bColor;
                draw.character = defaultDraw.character;

                Log.Add($"The {entity.GetComponent<Description>().name} is revealed!");

                revealed = true;
            }
        }
        public void SetTrap(Room room)
        {
            defaultDraw = new Draw(entity.GetComponent<Draw>());
            entity.GetComponent<Draw>().character = '.';
            revealed = false;
        }
        public override void SetDelegates() 
        {
            if (entity.GetComponent<SpawnDetails>() != null && set)
            {
                entity.GetComponent<SpawnDetails>().onSpawn += SetTrap;
            }
        }
        public Trap(bool set = true) 
        {
            set = true;
        }
        public Trap() { }
    }
    [Serializable]
    public class Thorns : Component
    {
        public void Thorn(Entity target)
        {
            target.GetComponent<Harmable>().RecieveDamage(1);
            Log.Add($"The thorns pricks {target.GetComponent<Description>().name}!");
        }
        public override void SetDelegates()
        {
            entity.GetComponent<Trap>().onStep += Thorn;
        }
        public Thorns() { }
    }
}
