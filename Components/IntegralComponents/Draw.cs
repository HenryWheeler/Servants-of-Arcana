using SadConsole;
using System;
using Console = SadConsole.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    [Serializable]
    public class Draw : Component
    {
        public Color fColor { get; set; }
        public Color bColor { get; set; }
        public char character { get; set; }
        public override void SetDelegates() { }
        public Draw(Color fColor, Color bColor, char character) 
        {
            this.fColor = fColor; 
            this.bColor = bColor; 
            this.character = character;
        }
        public Draw(Draw draw) 
        { 
            fColor = draw.fColor; 
            bColor = draw.bColor; 
            character = draw.character;
        }
        public Draw() { }
        public void DrawToScreen(Console console, int x, int y)
        {
            console.SetCellAppearance(x, y, new ColoredGlyph(fColor, bColor, character));
        }
    }
}
