using System;
using System.Collections.Generic;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Handles keyboard input for the game with buffering
    /// </summary>
    public class InputHandler
    {
        private Queue<ConsoleKeyInfo> keyBuffer;
        private const int MaxBufferSize = 5;

        /// <summary>
        /// Constructor for InputHandler
        /// </summary>
        public InputHandler()
        {
            keyBuffer = new Queue<ConsoleKeyInfo>(MaxBufferSize);
        }

        /// <summary>
        /// Gets the next key press, or null if no keys are available
        /// </summary>
        /// <returns>ConsoleKeyInfo if a key was pressed, null otherwise</returns>
        public ConsoleKeyInfo? GetKeyPress()
        {
            // Process all waiting key presses first to avoid input delay
            while (Console.KeyAvailable && keyBuffer.Count < MaxBufferSize)
            {
                keyBuffer.Enqueue(Console.ReadKey(true));
            }

            // Return a key from the buffer if available
            if (keyBuffer.Count > 0)
            {
                return keyBuffer.Dequeue();
            }

            return null;
        }

        /// <summary>
        /// Clears the key buffer, useful when changing game states
        /// </summary>
        public void ClearBuffer()
        {
            keyBuffer.Clear();
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true); // Clear console key buffer
            }
        }

        /// <summary>
        /// Checks if a specific key is being pressed without consuming it
        /// </summary>
        /// <param name="key">The key to check for</param>
        /// <returns>True if the key is in the buffer</returns>
        public bool IsKeyPressed(ConsoleKey key)
        {
            foreach (var keyInfo in keyBuffer)
            {
                if (keyInfo.Key == key)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Waits for any key press and returns it
        /// </summary>
        /// <param name="intercept">Whether to hide the key from the console</param>
        /// <returns>The key that was pressed</returns>
        public ConsoleKeyInfo WaitForKeyPress(bool intercept = true)
        {
            ClearBuffer();
            return Console.ReadKey(intercept);
        }
    }
}