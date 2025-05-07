using System;

namespace ConsoleEscapeFromTarkov.UI
{
    /// <summary>
    /// Extension of UIManager with optimized direct rendering methods
    /// </summary>
    public partial class UIManager
    {
        private bool lastFrameWasPlaying = false;

        /// <summary>
        /// Renders the world directly to the console in an optimized way
        /// </summary>
        public void RenderWorldDirectly()
        {
            // If this wasn't previously rendered as a playing frame, clear the console
            if (!lastFrameWasPlaying)
            {
                Console.Clear();
                lastFrameWasPlaying = true;
            }

            // Optimize world rendering by using string builders for each line
            StringBuilder[] lines = new StringBuilder[world.Height];
            for (int y = 0; y < world.Height; y++)
            {
                lines[y] = new StringBuilder(world.Width);
            }

            // First pass: Build all lines as strings
            for (int y = 0; y < world.Height; y++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    lines[y].Append(world.GetTile(x, y));
                }
            }

            // Second pass: Render all lines at once with minimal cursor movement
            for (int y = 0; y < world.Height; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write(lines[y].ToString());
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Clears a specific rectangle of the console
        /// </summary>
        private void ClearConsoleArea(int left, int top, int width, int height)
        {
            string emptyLine = new string(' ', width);
            for (int y = 0; y < height; y++)
            {
                Console.SetCursorPosition(left, top + y);
                Console.Write(emptyLine);
            }
        }

        /// <summary>
        /// Gets the appropriate color for a tile
        /// </summary>
        /// <param name="tile">The tile character</param>
        /// <returns>Console color for the tile</returns>
        private ConsoleColor GetTileColor(char tile)
        {
            switch (tile)
            {
                case '@': // Player
                    return ConsoleColor.Cyan;
                case 'e': // Enemy
                case 'E':
                case 's':
                case 'r':
                    return ConsoleColor.Red;
                case '↑': // Projectiles
                case '→':
                case '↓':
                case '←':
                case '*':
                    return ConsoleColor.Yellow;
                case '↟': // Enemy projectiles
                case '↠':
                case '↡':
                case '↞':
                case '•':
                    return ConsoleColor.Red;
                case '▣': // Loot
                    return ConsoleColor.Green;
                case 'X': // Extraction point
                    return ConsoleColor.Magenta;
                case '+': // Medical station
                    return ConsoleColor.Green;
                case '⚡': // Ammo cache
                    return ConsoleColor.Yellow;
                case '!': // Mission objective
                    return ConsoleColor.Cyan;
                case 'T': // Trees
                    return ConsoleColor.DarkGreen;
                case '~': // Water
                    return ConsoleColor.Blue;
                default:
                    // Default color
                    return ConsoleColor.Gray;
            }
        }
    }
}