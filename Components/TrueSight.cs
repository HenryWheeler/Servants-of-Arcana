using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    [Serializable]
    public class TrueSight : Component
    {
        public int range { get; set; }
        public string type { get; set; }
        public override void SetDelegates()
        {
            entity.GetComponent<Equipable>().onEquip += SetPercieve;
        }
        public void SetPercieve(Entity wearer, bool equip)
        {
            if (wearer.GetComponent<PlayerController>() != null)
            {
                if (equip)
                {
                    wearer.GetComponent<TurnComponent>().onTurnEnd += Percieve;
                }
                else
                {
                    wearer.GetComponent<TurnComponent>().onTurnEnd -= Percieve;

                    foreach (Tile tile in Program.tiles)
                    {
                        if (tile != null)
                        {
                            Visibility visibility = tile.GetComponent<Visibility>();

                            if (visibility.visible)
                            {
                                switch (type)
                                {
                                    case "Actor":
                                        {
                                            if (tile.actor != null)
                                            {
                                                visibility.visible = false;
                                            }
                                            break;
                                        }
                                    case "Item":
                                        {
                                            if (tile.item != null)
                                            {
                                                visibility.visible = false;
                                            }
                                            break;
                                        }
                                }
                            }
                        }
                    }
                }
            }
        }
        public void Percieve(Vector location)
        {
            foreach (Tile tile in Program.tiles)
            {
                if (tile != null)
                {
                    Vector vector = tile.GetComponent<Vector>();

                    if (Math.Distance(vector.x, vector.y, location.x, location.y) <= range)
                    {
                        Visibility visibility = tile.GetComponent<Visibility>();

                        switch (type)
                        {
                            case "Actor":
                                {
                                    if (tile.actor != null)
                                    {
                                        visibility.visible = true;
                                        ShadowcastFOV.visibleTiles.Add(vector);
                                    }
                                    break;
                                }
                            case "Item":
                                {
                                    if (tile.item != null)
                                    {
                                        visibility.visible = true;
                                        ShadowcastFOV.visibleTiles.Add(vector);
                                    }
                                    break;
                                }
                        }
                    }
                }
            }
        }
        public TrueSight(int range, string type)
        {
            this.range = range;
            this.type = type;
        }
        public TrueSight() { }
    }
}
