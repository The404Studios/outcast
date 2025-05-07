using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.Utils;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages a log of game messages for display to the player
    /// </summary>
    public class MessageLog
    {
        private Queue<string> messages;
        private int capacity;

        /// <summary>
        /// Constructor for MessageLog
        /// </summary>
        /// <param name="capacity">Maximum number of messages to store</param>
        public MessageLog(int capacity)
        {
            this.capacity = capacity;
            messages = new Queue<string>(capacity);
        }

        /// <summary>
        /// Adds a message to the log
        /// </summary>
        /// <param name="message">Message to add</param>
        public void AddMessage(string message)
        {
            messages.Enqueue(message);

            // Remove oldest message if we exceed capacity
            if (messages.Count > capacity)
            {
                messages.Dequeue();
            }
        }

        /// <summary>
        /// Gets all messages in the log, newest first
        /// </summary>
        /// <returns>Enumerable of messages</returns>
        public IEnumerable<string> GetMessages()
        {
            return messages.Reverse(); // Return newest first
        }

        /// <summary>
        /// Clears all messages from the log
        /// </summary>
        public void Clear()
        {
            messages.Clear();
        }

        /// <summary>
        /// Gets the most recent message
        /// </summary>
        /// <returns>The most recent message, or empty string if none</returns>
        public string GetLatestMessage()
        {
            if (messages.Count > 0)
            {
                return messages.Last();
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets the specified number of recent messages
        /// </summary>
        /// <param name="count">Number of messages to get</param>
        /// <returns>Array of recent messages</returns>
        public string[] GetRecentMessages(int count)
        {
            return messages.Reverse().Take(count).ToArray();
        }
    }
}