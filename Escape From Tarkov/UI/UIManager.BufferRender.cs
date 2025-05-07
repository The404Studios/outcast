using System;
using ConsoleEscapeFromTarkov.Utils;

namespace ConsoleEscapeFromTarkov.UI
{
    /// <summary>
    /// Extension of UIManager with buffer-based rendering methods
    /// </summary>
    public partial class UIManager
    {
        private ConsoleBuffer screenBuffer; // Reference to the console buffer

        /// <summary>
        /// Initialize the buffer for the UI manager
        /// </summary>
        /// <param name="buffer">The console buffer to use</param>
        public void InitializeBuffer(ConsoleBuffer buffer)
        {
            this.screenBuffer = buffer;
        }

        /// <summary>
        /// Renders the world to the console buffer
        /// </summary>
        public void RenderWorldToBuffer()
        {
            if (screenBuffer == null) return;

            // Clear only the world area of the buffer
            for (int y = 0; y < world.Height; y++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    screenBuffer.SetChar(x, y, ' ');
                }
            }

            // Render the world to the buffer
            for (int y = 0; y < world.Height; y++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    char tile = world.GetTile(x, y);
                    if (tile != ' ')
                    {
                        ConsoleColor color = GetTileColorForBuffer(tile);
                        screenBuffer.SetChar(x, y, tile, color);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the appropriate color for a tile
        /// </summary>
        /// <param name="tile">The tile character</param>
        /// <returns>Console color for the tile</returns>
        private ConsoleColor GetTileColorForBuffer(char tile)
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

        /// <summary>
        /// Renders player stats to the buffer
        /// </summary>
        public void RenderPlayerStatsToBuffer()
        {
            if (screenBuffer == null) return;

            int statsX = world.Width + 2;

            // Clear the stats area
            for (int y = 0; y < 15; y++)
            {
                for (int x = statsX; x < statsX + 30; x++)
                {
                    screenBuffer.SetChar(x, y, ' ');
                }
            }

            // Player level and XP
            screenBuffer.WriteString(statsX, 1, $"Player Level: {player.Level}", ConsoleColor.White);
            screenBuffer.WriteString(statsX, 2, $"XP: {player.Experience}", ConsoleColor.White);

            // Health bar
            screenBuffer.WriteString(statsX, 4, $"Health: {player.Health}/{player.MaxHealth} [", ConsoleColor.White);

            int healthBarWidth = 20;
            int healthFill = (int)((player.Health / (float)player.MaxHealth) * healthBarWidth);

            ConsoleColor healthColor = GetHealthColorForBuffer();
            for (int i = 0; i < healthFill; i++)
            {
                screenBuffer.SetChar(statsX + 9 + i, 4, '█', healthColor);
            }

            for (int i = healthFill; i < healthBarWidth; i++)
            {
                screenBuffer.SetChar(statsX + 9 + i, 4, ' ', ConsoleColor.White);
            }

            screenBuffer.SetChar(statsX + 9 + healthBarWidth, 4, ']', ConsoleColor.White);

            // Weapon info
            if (player.EquippedWeapon != null)
            {
                screenBuffer.WriteString(statsX, 6, $"Weapon: {player.EquippedWeapon.GetDescription()}", ConsoleColor.White);
            }
            else
            {
                screenBuffer.WriteString(statsX, 6, "Weapon: None", ConsoleColor.White);
            }

            // Quickslots
            screenBuffer.WriteString(statsX, 8, "Quickslots:", ConsoleColor.White);
            for (int i = 0; i < player.QuickSlots.Length; i++)
            {
                screenBuffer.WriteString(statsX, 9 + i, $"{i + 1}: {(player.QuickSlots[i] != null ? player.QuickSlots[i].Name : "Empty")}", ConsoleColor.White);
            }
        }

        /// <summary>
        /// Renders message log to the buffer
        /// </summary>
        public void RenderMessageLogToBuffer()
        {
            if (screenBuffer == null) return;

            int logStartY = world.Height + 1;
            int logWidth = world.Width;

            // Clear the message area
            for (int y = logStartY; y < logStartY + 8; y++)
            {
                for (int x = 0; x < logWidth; x++)
                {
                    screenBuffer.SetChar(x, y, ' ');
                }
            }

            // Draw separator line
            for (int x = 0; x < logWidth; x++)
            {
                screenBuffer.SetChar(x, logStartY, '=', ConsoleColor.White);
            }

            // Write header
            screenBuffer.WriteString(0, logStartY + 1, "Messages:", ConsoleColor.White);

            // Write messages
            int messageIndex = 0;
            foreach (string message in messageLog.GetMessages().Take(6))
            {
                // Truncate message if too long
                string displayMessage = message;
                if (displayMessage.Length > logWidth - 2)
                {
                    displayMessage = displayMessage.Substring(0, logWidth - 5) + "...";
                }

                screenBuffer.WriteString(1, logStartY + 2 + messageIndex, displayMessage, ConsoleColor.White);
                messageIndex++;
            }
        }

        /// <summary>
        /// Renders mission objectives to the buffer
        /// </summary>
        public void RenderMissionObjectivesToBuffer()
        {
            if (screenBuffer == null) return;

            int missionStartY = 15;
            int missionX = world.Width + 2;

            // Clear the mission area
            for (int y = missionStartY; y < missionStartY + 8; y++)
            {
                for (int x = missionX; x < missionX + 40; x++)
                {
                    screenBuffer.SetChar(x, y, ' ');
                }
            }

            // Write header
            screenBuffer.WriteString(missionX, missionStartY, "Mission Objectives:", ConsoleColor.White);

            // Write objectives
            int objectiveIndex = 0;
            foreach (MissionObjective objective in missionManager.GetObjectives().Take(4))
            {
                // Status text
                string statusText = objective.IsCompleted ? "[✓]" :
                    (objective.Type == MissionObjectiveType.FindItem ||
                     objective.Type == MissionObjectiveType.VisitLocation) ?
                        "[ ]" : $"[{objective.CurrentCount}/{objective.TargetCount}]";

                // Truncate if too long
                string objectiveText = objective.Description;
                if (objectiveText.Length > 25)
                {
                    objectiveText = objectiveText.Substring(0, 22) + "...";
                }

                screenBuffer.WriteString(missionX, missionStartY + 1 + objectiveIndex, $" {statusText} {objectiveText}", ConsoleColor.White);
                objectiveIndex++;
            }

            // Weather info
            screenBuffer.WriteString(missionX, missionStartY + 6, $"Weather: {weatherSystem.CurrentWeather}", ConsoleColor.White);

            // Enemy count
            screenBuffer.WriteString(missionX, missionStartY + 7, $"Enemies: {enemyManager.Enemies.Count}", ConsoleColor.White);
        }

        /// <summary>
        /// Renders controls hint to the buffer
        /// </summary>
        public void RenderControlsToBuffer()
        {
            if (screenBuffer == null) return;

            int controlsY = world.Height - 3;
            int controlsX = world.Width + 2;

            // Clear the controls area
            for (int y = controlsY; y < controlsY + 3; y++)
            {
                for (int x = controlsX; x < controlsX + 40; x++)
                {
                    screenBuffer.SetChar(x, y, ' ');
                }
            }

            // Write controls
            screenBuffer.WriteString(controlsX, controlsY, "Controls: WASD=Move SPACE=Shoot", ConsoleColor.White);
            screenBuffer.WriteString(controlsX, controlsY + 1, "I=Inventory E=Interact R=Reload", ConsoleColor.White);
            screenBuffer.WriteString(controlsX, controlsY + 2, "M=Map C=Character H=Help 1-5=Quickslots", ConsoleColor.White);
        }

        /// <summary>
        /// Gets color for health bar based on current health
        /// </summary>
        /// <returns>Console color for health</returns>
        private ConsoleColor GetHealthColorForBuffer()
        {
            float healthPercent = player.Health / (float)player.MaxHealth;

            if (healthPercent > 0.7f)
                return ConsoleColor.Green;
            else if (healthPercent > 0.3f)
                return ConsoleColor.Yellow;
            else
                return ConsoleColor.Red;
        }

        /// <summary>
        /// Renders the game UI using the buffer system
        /// </summary>
        public void RenderBufferedGameUI()
        {
            if (screenBuffer == null) return;

            // Render all UI components to the buffer
            RenderWorldToBuffer();
            RenderPlayerStatsToBuffer();
            RenderMessageLogToBuffer();
            RenderMissionObjectivesToBuffer();
            RenderControlsToBuffer();

            // Render the buffer to the console
            screenBuffer.Render();
        }
    }
}