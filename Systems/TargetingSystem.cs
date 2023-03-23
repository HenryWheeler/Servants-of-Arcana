using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace Servants_of_Arcana
{
    public class TargetingSystem
    {
        public static Vector targetedPosition = new Vector();
        public static bool isTargeting = false;
        public static Usable currentUsedItem { get; set; }
        public static void BeginTargeting(Usable item)
        {
            Vector playerPosition = Program.player.GetComponent<Vector>();
            targetedPosition = new Vector(playerPosition.x, playerPosition.y);

            isTargeting = true;
            Program.player.GetComponent<TurnComponent>().isTurnActive = false;

            Program.playerConsole.Clear();
            Program.rootConsole.Children.MoveToTop(Program.targetConsole);

            currentUsedItem = item;

            Program.ClearUISFX();
            MoveReticle(new Vector(0, 0));
        }
        public static void StopTargeting(bool returnToInventory)
        {
            isTargeting = false;
            Program.player.GetComponent<TurnComponent>().isTurnActive = true;

            Program.targetConsole.Clear();
            Program.rootConsole.Children.MoveToTop(Program.playerConsole);

            AttributeManager.UpdateAttributes(Program.player);

            currentUsedItem = null;

            Program.ClearUISFX();
            Program.MoveCamera(Program.player.GetComponent<Vector>());

            if (returnToInventory)
            {
                InventoryManager.OpenInventory();
            }
        }
        public static void MoveReticle(Vector direction)
        {
            Vector playerPosition = Program.player.GetComponent<Vector>();
            if (Math.CheckBounds(targetedPosition.x + direction.x, targetedPosition.y + direction.y))
            {
                Program.ClearUISFX();
                Program.lookConsole.Clear();

                targetedPosition.x += direction.x;
                targetedPosition.y += direction.y;

                Program.CreateConsoleBorder(Program.targetConsole);
                Program.MoveCamera(targetedPosition);

                if (Math.Distance(playerPosition.x, playerPosition.y, targetedPosition.x + direction.x, targetedPosition.y + direction.y) <= currentUsedItem.range)
                {
                    ShowAffectedTile(false);
                }
                else
                {
                    ShowAffectedTile(true);
                }
            }
        }
        public static void UseSelectedItem()
        {
            currentUsedItem.Use(Program.player, targetedPosition);

            if (currentUsedItem.entity.GetComponent<Consumable>() != null)
            {
                Program.player.GetComponent<InventoryComponent>().items.Remove(currentUsedItem.entity);
            }

            StopTargeting(false);
            Program.player.GetComponent<TurnComponent>().EndTurn();
            Log.Add($"{Program.player.GetComponent<Description>().name} {currentUsedItem.action} the {currentUsedItem.entity.GetComponent<Description>().name}!");
        }
        public static void ShowAffectedTile(bool outOfRange)
        {
            List<Vector> visitedTiles = new List<Vector>();
            if (currentUsedItem.areaOfEffect == null)
            {
                Program.uiSfx[targetedPosition.x, targetedPosition.y] = ReturnTileAppearance(targetedPosition, outOfRange);
                visitedTiles.Add(targetedPosition);
            }
            else
            {
                foreach (Vector position in currentUsedItem.areaOfEffect.Invoke(targetedPosition, currentUsedItem.range))
                {
                    Program.uiSfx[position.x, position.y] = ReturnTileAppearance(position, outOfRange);
                    visitedTiles.Add(position);
                }
            }

            foreach (Vector tile in visitedTiles)
            {

            }
        }
        public static Draw ReturnTileAppearance(Vector tilePosition, bool outOfRange) 
        {
            if (Math.CheckBounds(tilePosition.x, tilePosition.y))
            {
                Draw returnDraw;

                if (Program.tiles[tilePosition.x, tilePosition.y].actor != null)
                {
                    returnDraw = new Draw(Program.tiles[tilePosition.x, tilePosition.y].actor.GetComponent<Draw>());
                }
                else if (Program.tiles[tilePosition.x, tilePosition.y].item != null)
                {
                    returnDraw = new Draw(Program.tiles[tilePosition.x, tilePosition.y].item.GetComponent<Draw>());
                }
                else
                {
                    returnDraw = new Draw(Program.tiles[tilePosition.x, tilePosition.y].GetComponent<Draw>());
                }

                if (!outOfRange)
                {
                    returnDraw.bColor = SadRogue.Primitives.Color.Cyan;
                }
                else
                {
                    returnDraw.bColor = SadRogue.Primitives.Color.Red;
                }

                return returnDraw;
            }
            else
            {
                return null;
            }
        }
    }
}
