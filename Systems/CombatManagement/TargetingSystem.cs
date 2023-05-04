using SadConsole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using SadRogue.Primitives;

namespace Servants_of_Arcana
{
    public class TargetingSystem
    {
        public static Vector targetedPosition = new Vector();
        public static bool isTargeting = false;
        public static bool isTargetValid { get; set; }
        public static bool requiredTargetsMet { get; set; }
        public static Usable currentUsedItem { get; set; }
        public static void BeginTargeting(Usable item)
        {
            Vector playerPosition = Program.player.GetComponent<Vector>();
            targetedPosition = new Vector(playerPosition.x, playerPosition.y);

            Program.player.GetComponent<TurnComponent>().isTurnActive = false;
            isTargeting = true;

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

            if (returnToInventory)
            {
                InventoryManager.OpenInventoryDisplay(currentUsedItem.entity);
            }

            currentUsedItem = null;

            Program.ClearUISFX();
            Program.MoveCamera(Program.player.GetComponent<Vector>());
        }
        public static void MoveReticle(Vector direction)
        {
            Vector playerPosition = Program.player.GetComponent<Vector>();
            if (Math.CheckBounds(targetedPosition.x + direction.x, targetedPosition.y + direction.y))
            {
                Program.ClearUISFX();
                Program.targetConsole.Clear();

                targetedPosition.x += direction.x;
                targetedPosition.y += direction.y;

                Program.CreateConsoleBorder(Program.targetConsole);
                Program.MoveCamera(targetedPosition);

                if (Program.tiles[targetedPosition.x, targetedPosition.y].GetComponent<Visibility>().visible)
                {
                    if (Math.Distance(playerPosition.x, playerPosition.y, targetedPosition.x, targetedPosition.y) <= currentUsedItem.range &&
                        currentUsedItem.tileTypes.Contains(Program.tiles[targetedPosition.x, targetedPosition.y].terrainType))
                    {
                        ShowAffectedTiles(false);
                        isTargetValid = true;
                    }
                    else
                    {
                        ShowAffectedTiles(true);
                        isTargetValid = false;
                    }
                }
                else
                {
                    isTargetValid = false;
                    Program.uiSfx[targetedPosition.x, targetedPosition.y] = new Draw(Color.Red, Color.Black, 'X');
                }
                Program.CreateConsoleBorder(Program.targetConsole);
            }
        }
        public static void UseSelectedItem()
        {
            currentUsedItem.Use(Program.player, targetedPosition);

            if (currentUsedItem.entity.GetComponent<Charges>() != null)
            {
                currentUsedItem.entity.GetComponent<Charges>().chargesRemaining--;
                if (currentUsedItem.entity.GetComponent<Charges>().chargesRemaining <= 0)
                {
                    if (currentUsedItem.entity.GetComponent<Equipable>() != null && currentUsedItem.entity.GetComponent<Equipable>().equipped)
                    {
                        InventoryManager.UnequipItem(Program.player, currentUsedItem.entity, false);
                    }

                    Program.player.GetComponent<InventoryComponent>().items.Remove(currentUsedItem.entity);
                    Log.Add($"The {currentUsedItem.entity.GetComponent<Description>().name} is spent!");
                }
            }

            StopTargeting(false);
            Program.player.GetComponent<TurnComponent>().EndTurn();
        }
        public static void ShowAffectedTiles(bool outOfRange)
        {
            List<Vector> visitedTiles = new List<Vector>();

            if (currentUsedItem.areaOfEffect == null)
            {
                Program.uiSfx[targetedPosition.x, targetedPosition.y] = ReturnTileAppearance(targetedPosition, outOfRange);
                visitedTiles.Add(targetedPosition);
            }
            else
            {
                foreach (Vector position in currentUsedItem.areaOfEffect.Invoke(Program.player.GetComponent<Vector>(), targetedPosition, currentUsedItem.strength))
                {
                    if (Math.CheckBounds(position.x, position.y))
                    {
                        Program.uiSfx[position.x, position.y] = ReturnTileAppearance(position, outOfRange);
                        visitedTiles.Add(position);
                    }
                }
            }

            int actorCount = 0;
            int y = 6;
            foreach (Vector vector in visitedTiles)
            {
                Tile tile = Program.tiles[vector.x, vector.y];
                if (tile.actor != null)
                {
                    actorCount++;

                    int x = 3;
                    Draw draw = tile.actor.GetComponent<Draw>();

                    Program.targetConsole.DrawLine(new Point(0, y), new Point(Program.targetConsole.Width, y), '-', Color.Gray, Color.Black);

                    Math.DisplayToConsole(Program.targetConsole, $"<#> <{tile.actor.GetComponent<Description>().name}>", 1, x, 0, y, false);
                    Program.targetConsole.SetCellAppearance(x + 2, y, new ColoredGlyph(draw.fColor, draw.bColor, draw.character));
                    y += 2;
                }
            }

            if (actorCount < currentUsedItem.requiredTargets)
            {
                requiredTargetsMet = false;
            }
            else
            {
                requiredTargetsMet = true;
            }
        }
        public static Draw ReturnTileAppearance(Vector tilePosition, bool outOfRange)
        {
            if (Math.CheckBounds(tilePosition.x, tilePosition.y))
            {
                Draw returnDraw;

                Color color = Color.Black;

                if (Program.tiles[tilePosition.x, tilePosition.y].actor != null)
                {
                    returnDraw = new Draw(Program.tiles[tilePosition.x, tilePosition.y].actor.GetComponent<Draw>());
                    color = Color.Yellow;
                }
                else if (Program.tiles[tilePosition.x, tilePosition.y].item != null)
                {
                    returnDraw = new Draw(Program.tiles[tilePosition.x, tilePosition.y].item.GetComponent<Draw>());
                }
                else
                {
                    returnDraw = new Draw(Program.tiles[tilePosition.x, tilePosition.y].GetComponent<Draw>());
                }

                if (color == Color.Black)
                {
                    if (!outOfRange)
                    {
                        color = Color.DarkBlue;
                    }
                    else
                    {
                        color = Color.Red;
                    }
                }

                returnDraw.bColor = color;

                return returnDraw;
            }
            else
            {
                return null;
            }
        }
    }
}
