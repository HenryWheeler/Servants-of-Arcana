using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class EntityEffects
    {
        public static void MagicMap(Entity user, Vector origin)
        {
            if (user.GetComponent<PlayerController>() != null) 
            {
                foreach (Tile tile in Program.tiles)
                {
                    if (tile != null && tile.terrainType != 0)
                    {
                        tile.GetComponent<Visibility>().explored = true;
                        Vector vector = tile.GetComponent<Vector>();

                        int distance = (int)Math.Distance(origin.x, origin.y, vector.x, vector.y);

                        Program.CreateSFX(vector, new Draw[] 
                        {
                            new Draw(Color.OrangeRed, Color.Black, (char)176),
                        }, distance, "None", 2);
                    }
                }

                
            }
        }
    }
}
