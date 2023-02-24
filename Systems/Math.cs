using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class Math
    {
        public static bool CheckPath(int oX, int oY, int eX, int eY) 
        {
            bool passing = true;
            int t;
            int x = oX; int y = oY;
            int delta_x = eX - oX; int delta_y = eY - oY;
            int abs_delta_x = System.Math.Abs(delta_x); int abs_delta_y = System.Math.Abs(delta_y);
            int sign_x = System.Math.Sign(delta_x); int sign_y = System.Math.Sign(delta_y);

            if (oX == eX && oY == eY) return false;
            //if (Math.Abs(delta_x) > range || Math.Abs(delta_y) > range) return false;

            if (abs_delta_x > abs_delta_y)
            {
                t = abs_delta_y * 2 - abs_delta_x;
                do
                {
                    if (t >= 0) { y += sign_y; t -= abs_delta_x * 2; }
                    x += sign_x;
                    t += abs_delta_y * 2;
                    if (x == eX && y == eY) { return false; }

                    if (Program.tiles[x, y].terrainType == 0)
                    {
                        passing = false;
                    }
                }
                while (passing);
                return true;
            }
            else
            {
                t = abs_delta_x * 2 - abs_delta_y;
                do
                {
                    if (t >= 0) { x += sign_x; t -= abs_delta_y * 2; }
                    y += sign_y;
                    t += abs_delta_x * 2;
                    if (x == eX && y == eY) { return false; }

                    if (Program.tiles[x, y].terrainType == 0)
                    {
                        passing = false;
                    }
                }
                while (passing);
                return true;
            }
        }
        public static Controller ReturnController(Entity entity)
        {
            if (entity != null && entity.components != null)
            {
                foreach (Component component in entity.components)
                {
                    if (component.GetType().BaseType.Equals(typeof(Controller))) { return (Controller)component; }
                }
            }
            return null;
        }
        public static double Distance(int oX, int oY, int eX, int eY) 
        {
            return System.Math.Sqrt(System.Math.Pow(eX - oX, 2) + System.Math.Pow(eY - oY, 2)); 
        }
        public static bool CheckBounds(int x, int y)
        {
            if (x <= 0 || x >= Program.gameWidth || y <= 0 || y >= Program.gameHeight)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
