using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class Vector : Component
    {
        public int x { get; set; }
        public int y { get; set; }
        public Vector(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public Vector(Vector _1, Vector _2)
        {
            x = _1.x + _2.x;
            y = _1.y + _2.y;
        }
        public override int GetHashCode()
        {
            return 17 + 31 * x.GetHashCode() + 31 * y.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            Vector other = obj as Vector;
            return other != null && x == other.x && y == other.y;
        }
        public Vector() { }
    }
}
