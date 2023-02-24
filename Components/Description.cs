using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class Description : Component
    {
        public string name { get; set; }
        public string description { get; set; }
        public Description(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
        public Description(Description description)
        {
            name = description.name;
            this.description = description.description;
        }
        public Description() { }
    }
}
