using System;
using System.Text;
using ConsoleEscapeFromTarkov.GameCore;

namespace ConsoleEscapeFromTarkov
{
    /// <summary>
    /// Entry point for the Console Escape from Tarkov game
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Console Escape from Tarkov";
            Console.CursorVisible = false;
            Console.OutputEncoding = Encoding.UTF8;

            // Set window size to optimize gameplay
            try
            {
                Console.SetWindowSize(120, 40);
                Console.SetBufferSize(120, 40);
                Console.Clear(); // Ensure a clean start
            }
            catch (Exception)
            {
                // Ignore if not supported on the platform
                Console.WriteLine("Unable to set console size. Game will adapt to current console dimensions.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
                Console.Clear();
            }

            // Initialize and start the game
            GameManager gameManager = new GameManager();
            gameManager.StartGame();
        }
    }
}