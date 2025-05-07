using System;

namespace ConsoleEscapeFromTarkov.UI
{
    /// <summary>
    /// Extension of UIManager with improved rendering methods
    /// </summary>
    public partial class UIManager
    {
        /// <summary>
        /// Renders the world grid with full refresh to prevent display artifacts
        /// </summary>
        public void RenderWorldFull()
        {
            // Clear the play area first
            for (int y = 0; y < world.Height; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write(new string(' ', world.Width));
            }

            // Then render everything fresh
            for (int y = 0; y < world.Height; y++)
            {
                Console.SetCursorPosition(0, y);
                for (int x = 0; x < world.Width; x++)
                {
                    char tile = world.GetTile(x, y);

                    // Apply color based on tile type
                    SetTileColor(tile);

                    Console.Write(tile);
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Clears all UI areas to prevent rendering artifacts
        /// </summary>
        public void ClearUI()
        {
            // Clear right sidebar area
            int sidebarWidth = Console.WindowWidth - world.Width - 2;
            for (int y = 0; y < Console.WindowHeight - 1; y++)
            {
                Console.SetCursorPosition(world.Width + 2, y);
                Console.Write(new string(' ', sidebarWidth));
            }

            // Clear message log area
            int logStartY = world.Height + 1;
            for (int y = logStartY; y < Console.WindowHeight - 1; y++)
            {
                Console.SetCursorPosition(0, y);
                Console.Write(new string(' ', world.Width));
            }
        }

        /// <summary>
        /// Performs a complete refresh of all UI elements
        /// </summary>
        public void RefreshAllUI()
        {
            Console.Clear();
            RenderWorldFull();
            RenderGameUI();
        }

        /// <summary>
        /// Draws a clean frame with proper double-buffering
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="title">Frame title</param>
        private void DrawCleanFrame(int x, int y, int width, int height, string title)
        {
            // Clear the frame area first
            for (int i = 0; i < height; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(new string(' ', width));
            }

            // Then draw the frame
            DrawFrame(x, y, width, height, title);
        }
    }
}