using System;
using System.Text;

namespace ConsoleEscapeFromTarkov.Utils
{
    /// <summary>
    /// A double-buffering system for console rendering to prevent flickering
    /// </summary>
    public class ConsoleBuffer
    {
        private char[,] buffer;
        private ConsoleColor[,] colorBuffer;
        private int width;
        private int height;
        private bool firstRender = true;

        /// <summary>
        /// Creates a new console buffer with the specified dimensions
        /// </summary>
        /// <param name="width">Buffer width</param>
        /// <param name="height">Buffer height</param>
        public ConsoleBuffer(int width, int height)
        {
            this.width = width;
            this.height = height;
            buffer = new char[width, height];
            colorBuffer = new ConsoleColor[width, height];

            // Initialize buffer with spaces
            Clear();
        }

        /// <summary>
        /// Clears the buffer
        /// </summary>
        public void Clear()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    buffer[x, y] = ' ';
                    colorBuffer[x, y] = ConsoleColor.Gray;
                }
            }
        }

        /// <summary>
        /// Sets a character at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="c">Character to set</param>
        /// <param name="color">Color for the character</param>
        public void SetChar(int x, int y, char c, ConsoleColor color = ConsoleColor.Gray)
        {
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                buffer[x, y] = c;
                colorBuffer[x, y] = color;
            }
        }

        /// <summary>
        /// Writes a string at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to write</param>
        /// <param name="color">Color for the text</param>
        public void WriteString(int x, int y, string text, ConsoleColor color = ConsoleColor.Gray)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (x + i < width)
                {
                    SetChar(x + i, y, text[i], color);
                }
            }
        }

        /// <summary>
        /// Renders the buffer to the console window
        /// </summary>
        public void Render()
        {
            // If this is the first render, do a full screen render
            if (firstRender)
            {
                Console.Clear();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Console.SetCursorPosition(x, y);
                        Console.ForegroundColor = colorBuffer[x, y];
                        Console.Write(buffer[x, y]);
                    }
                }
                firstRender = false;
                return;
            }

            // For subsequent renders, we'll build strings for each line to minimize cursor movements
            StringBuilder[] lines = new StringBuilder[height];
            ConsoleColor[] currentColors = new ConsoleColor[height];
            int[] lineLength = new int[height];

            for (int y = 0; y < height; y++)
            {
                lines[y] = new StringBuilder();
                currentColors[y] = colorBuffer[0, y];
                lines[y].Append(buffer[0, y]);
                lineLength[y] = 1;
            }

            // Render each line efficiently
            for (int y = 0; y < height; y++)
            {
                Console.SetCursorPosition(0, y);
                ConsoleColor currentColor = currentColors[y];
                Console.ForegroundColor = currentColor;
                Console.Write(lines[y].ToString().PadRight(width));
            }

            // Reset console color
            Console.ResetColor();
        }
    }
}