using System;
using System.Text;
using System.Linq;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.UI
{
    /// <summary>
    /// Manages all UI rendering and user interface elements
    /// </summary>
    public partial class UIManager
    {
        private World world;
        private Player player;
        private LootManager lootManager;
        private EnemyManager enemyManager;
        private MessageLog messageLog;
        private MissionManager missionManager;
        private WeatherSystem weatherSystem;
        private LootContainer activeLootContainer;
        private int selectedInventoryIndex;
        private int selectedLootIndex;
        private int currentInventoryTab;
        private StringBuilder uiBuffer;

        // UI state tracking for efficient updates
        private int lastHealth;
        private string lastWeaponInfo;
        private int lastEnemyCount;

        /// <summary>
        /// Selected index in the inventory UI
        /// </summary>
        public int SelectedInventoryIndex => selectedInventoryIndex;

        /// <summary>
        /// Selected index in the looting UI
        /// </summary>
        public int SelectedLootIndex => selectedLootIndex;

        /// <summary>
        /// Current tab in the inventory UI
        /// </summary>
        public int CurrentInventoryTab => currentInventoryTab;

        /// <summary>
        /// Constructor for UIManager
        /// </summary>
        /// <param name="world">World reference</param>
        /// <param name="player">Player reference</param>
        /// <param name="lootManager">Loot manager reference</param>
        /// <param name="enemyManager">Enemy manager reference</param>
        /// <param name="messageLog">Message log reference</param>
        /// <param name="missionManager">Mission manager reference</param>
        /// <param name="weatherSystem">Weather system reference</param>
        public UIManager(World world, Player player, LootManager lootManager, EnemyManager enemyManager,
                        MessageLog messageLog, MissionManager missionManager, WeatherSystem weatherSystem)
        {
            this.world = world;
            this.player = player;
            this.lootManager = lootManager;
            this.enemyManager = enemyManager;
            this.messageLog = messageLog;
            this.missionManager = missionManager;
            this.weatherSystem = weatherSystem;

            uiBuffer = new StringBuilder(1000);
            selectedInventoryIndex = 0;
            selectedLootIndex = 0;
            currentInventoryTab = 0;

            // Initialize tracking state
            lastHealth = player.Health;
            lastWeaponInfo = player.EquippedWeapon?.GetDescription() ?? "None";
            lastEnemyCount = 0;
        }

        #region Main Game UI

        /// <summary>
        /// Renders the main game UI overlays
        /// </summary>
        public void RenderGameUI()
        {
            // Draw player stats
            DrawPlayerStats();

            // Draw message log
            DrawMessageLog();

            // Draw mission info
            DrawMissionInfo();

            // Draw controls hint
            DrawControlsHint();
        }

        /// <summary>
        /// Draws player stats (health, weapon, etc.)
        /// </summary>
        private void DrawPlayerStats()
        {
            Console.SetCursorPosition(world.Width + 2, 1);
            Console.Write($"Player Level: {player.Level}");

            Console.SetCursorPosition(world.Width + 2, 2);
            Console.Write($"XP: {player.Experience}");

            Console.SetCursorPosition(world.Width + 2, 4);

            // Health bar with color
            int healthBarWidth = 20;
            int healthFill = (int)((player.Health / (float)player.MaxHealth) * healthBarWidth);

            Console.Write($"Health: {player.Health}/{player.MaxHealth} [");

            Console.ForegroundColor = GetHealthColor();
            Console.Write(new string('█', healthFill));
            Console.ResetColor();

            Console.Write(new string(' ', healthBarWidth - healthFill));
            Console.Write("]");

            // Weapon info
            Console.SetCursorPosition(world.Width + 2, 6);
            if (player.EquippedWeapon != null)
            {
                Console.Write($"Weapon: {player.EquippedWeapon.GetDescription()}");
            }
            else
            {
                Console.Write("Weapon: None");
            }

            // Quickslot items
            Console.SetCursorPosition(world.Width + 2, 8);
            Console.Write("Quickslots:");
            for (int i = 0; i < player.QuickSlots.Length; i++)
            {
                Console.SetCursorPosition(world.Width + 2, 9 + i);
                Console.Write($"{i + 1}: {(player.QuickSlots[i] != null ? player.QuickSlots[i].Name : "Empty")}");
            }
        }

        /// <summary>
        /// Gets color for health bar based on current health
        /// </summary>
        /// <returns>Console color for health</returns>
        private ConsoleColor GetHealthColor()
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
        /// Draws the message log
        /// </summary>
        private void DrawMessageLog()
        {
            int logStartY = world.Height + 1;
            int logWidth = world.Width;

            Console.SetCursorPosition(0, logStartY);
            Console.Write(new string('=', logWidth));

            Console.SetCursorPosition(0, logStartY + 1);
            Console.Write("Messages:".PadRight(logWidth));

            // Prepare all messages at once
            string[] displayMessages = new string[6];
            int messageIndex = 0;
            foreach (string message in messageLog.GetMessages().Take(6))
            {
                // Truncate message if too long
                string displayMessage = message;
                if (displayMessage.Length > logWidth - 2)
                {
                    displayMessage = displayMessage.Substring(0, logWidth - 5) + "...";
                }

                displayMessages[messageIndex] = " " + displayMessage;
                messageIndex++;
            }

            // Fill remaining lines with empty space
            for (int i = messageIndex; i < 6; i++)
            {
                displayMessages[i] = new string(' ', logWidth);
            }

            // Write all messages at once
            for (int i = 0; i < 6; i++)
            {
                Console.SetCursorPosition(0, logStartY + 2 + i);
                Console.Write(displayMessages[i]);
            }
        }

        /// <summary>
        /// Draws mission objectives and info
        /// </summary>
        private void DrawMissionInfo()
        {
            int missionStartY = 15;

            Console.SetCursorPosition(world.Width + 2, missionStartY);
            Console.Write("Mission Objectives:");

            int objectiveIndex = 0;
            foreach (MissionObjective objective in missionManager.GetObjectives().Take(4))
            {
                Console.SetCursorPosition(world.Width + 2, missionStartY + 1 + objectiveIndex);

                // Objective with status
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

                Console.Write($" {statusText} {objectiveText}");
                objectiveIndex++;
            }

            // Weather info
            Console.SetCursorPosition(world.Width + 2, missionStartY + 6);
            Console.Write($"Weather: {weatherSystem.CurrentWeather}");

            // Enemy count
            Console.SetCursorPosition(world.Width + 2, missionStartY + 7);
            Console.Write($"Enemies: {enemyManager.Enemies.Count}");
        }

        /// <summary>
        /// Draws controls hint
        /// </summary>
        private void DrawControlsHint()
        {
            int controlsY = world.Height - 3;

            Console.SetCursorPosition(world.Width + 2, controlsY);
            Console.Write("Controls: WASD=Move SPACE=Shoot");
            Console.SetCursorPosition(world.Width + 2, controlsY + 1);
            Console.Write("I=Inventory E=Interact R=Reload");
            Console.SetCursorPosition(world.Width + 2, controlsY + 2);
            Console.Write("M=Map C=Character H=Help 1-5=Quickslots");
        }

        /// <summary>
        /// Renders the world grid using optimized double-buffering
        /// </summary>
        public void RenderWorldEfficient()
        {
            for (int y = 0; y < world.Height; y++)
            {
                for (int x = 0; x < world.Width; x++)
                {
                    // Only update the console if the tile changed
                    if (world.HasTileChanged(x, y))
                    {
                        Console.SetCursorPosition(x, y);
                        char tile = world.GetTile(x, y);

                        // Apply color based on tile type
                        SetTileColor(tile);

                        Console.Write(tile);
                        Console.ResetColor();
                    }
                }
            }
        }

        /// <summary>
        /// Sets the appropriate color for a tile
        /// </summary>
        /// <param name="tile">The tile character</param>
        private void SetTileColor(char tile)
        {
            switch (tile)
            {
                case '@': // Player
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case 'e': // Enemy
                case 'E':
                case 's':
                case 'r':
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case '↑': // Projectiles
                case '→':
                case '↓':
                case '←':
                case '*':
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case '↟': // Enemy projectiles
                case '↠':
                case '↡':
                case '↞':
                case '•':
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case '▣': // Loot
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case 'X': // Extraction point
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case '+': // Medical station
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case '⚡': // Ammo cache
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case '!': // Mission objective
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case 'T': // Trees
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                case '~': // Water
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                default:
                    // Default color
                    Console.ResetColor();
                    break;
            }
        }

        #endregion

        #region Game Screens

        /// <summary>
        /// Renders the main menu
        /// </summary>
        public void RenderMainMenu()
        {
            // Title
            string title = @"
  _____                           _____                     _____ _           _         
 / ____|                         |  __ \                   |_   _| |         | |        
| |     ___  _ __  ___  ___  __ _| |__) |__ _ _ __ ___   __ | | | |__   __ _| |_       
| |    / _ \| '_ \/ __|/ _ \/ _` |  ___/ __| | '_ ` _ \ / _ \| | | '_ \ / _` | __|      
| |___| (_) | | | \__ \  __/ (_| | |   \__ \ | | | | | | (_) | |_| | | | (_| | |_ _ _ _ 
 \_____\___/|_| |_|___/\___|\__,_|_|   |___/_|_| |_| |_|\___/_____|_| |_|\__,_|\__(_|_|_)
                                                                                        
";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(title);
            Console.ResetColor();

            Console.WriteLine("\n\n     Welcome to Console Escape from Tarkov!");
            Console.WriteLine("     A roguelike survival game inspired by Escape from Tarkov\n\n");

            Console.WriteLine("     [ENTER] Start Game");
            Console.WriteLine("     [H] Help");
            Console.WriteLine("     [ESC] Exit\n\n");

            Console.WriteLine("     Survive. Loot. Extract.");
        }

        /// <summary>
        /// Renders the inventory screen
        /// </summary>
        public void RenderInventory()
        {
            // Draw inventory frame
            DrawFrame(10, 3, 60, 30, "Inventory");

            // Draw tabs
            string[] tabs = { "Items", "Equipment", "Weapons" };
            DrawTabs(12, 5, tabs, currentInventoryTab);

            // Draw item list based on current tab
            switch (currentInventoryTab)
            {
                case 0: // Items
                    DrawItemList(player.Inventory, 12, 7, 56, 20);
                    break;
                case 1: // Equipment
                    DrawItemList(player.Equipment, 12, 7, 56, 20);
                    break;
                case 2: // Weapons
                    DrawWeaponList(12, 7, 56, 20);
                    break;
            }

            // Draw controls
            Console.SetCursorPosition(12, 29);
            Console.Write("Controls: [↑/↓] Navigate [TAB] Equip [ENTER] Use [DEL] Drop [1-5] Assign to quickslot");
            Console.SetCursorPosition(12, 30);
            Console.Write("[←/→] Change Tab [ESC] Close");
        }

        /// <summary>
        /// Renders the looting UI
        /// </summary>
        public void RenderLootingUI()
        {
            if (activeLootContainer == null)
                return;

            // Draw loot container frame
            DrawFrame(20, 5, 50, 25, activeLootContainer.Name);

            // Draw item list
            DrawItemList(activeLootContainer.Items, 22, 7, 46, 20);

            // Draw inventory info
            Console.SetCursorPosition(22, 28);
            Console.Write($"Inventory: {player.Inventory.Count}/{player.MaxInventorySize} items");

            // Draw controls
            Console.SetCursorPosition(22, 29);
            Console.Write("Controls: [↑/↓] Navigate [ENTER] Take [T] Take All [ESC] Close");
        }

        /// <summary>
        /// Renders the map screen
        /// </summary>
        public void RenderMapScreen()
        {
            DrawFrame(10, 2, 100, 35, "Map");

            // Draw a mini-map version of the world
            for (int y = 0; y < world.Height; y++)
            {
                Console.SetCursorPosition(12, 4 + y);
                for (int x = 0; x < world.Width; x++)
                {
                    char tile = world.GetTile(x, y);

                    // Only show certain features on the map
                    if (tile == '@') // Player
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write('@');
                    }
                    else if (tile == 'X') // Extraction point
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.Write('X');
                    }
                    else if (world.IsCollision(x, y)) // Walls and obstacles
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write('█');
                    }
                    else // Empty space
                    {
                        Console.Write(' ');
                    }

                    Console.ResetColor();
                }
            }

            // Draw map legend
            Console.SetCursorPosition(15, 36);
            Console.Write("Legend: ");

            Console.SetCursorPosition(24, 36);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("@ ");
            Console.ResetColor();
            Console.Write("- Player  ");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write("X ");
            Console.ResetColor();
            Console.Write("- Extraction  ");

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("█ ");
            Console.ResetColor();
            Console.Write("- Obstacle");

            // Controls
            Console.SetCursorPosition(15, 38);
            Console.Write("Press [M] or [ESC] to return to game");
        }

        /// <summary>
        /// Renders the character screen
        /// </summary>
        public void RenderCharacterScreen()
        {
            DrawFrame(10, 2, 80, 35, "Character");

            // Player stats
            Console.SetCursorPosition(15, 5);
            Console.Write($"Name: Player");

            Console.SetCursorPosition(15, 7);
            Console.Write($"Level: {player.Level}");

            Console.SetCursorPosition(15, 8);
            Console.Write($"Experience: {player.Experience}/{player.Level * 1000 + 1000} XP");

            Console.SetCursorPosition(15, 10);
            Console.Write($"Health: {player.Health}/{player.MaxHealth}");

            Console.SetCursorPosition(15, 12);
            Console.Write($"Inventory Capacity: {player.MaxInventorySize} slots");

            Console.SetCursorPosition(15, 14);
            Console.Write($"Kills: {player.KillCount}");

            // Equipment section
            Console.SetCursorPosition(15, 16);
            Console.Write("Equipped Items:");

            int itemIndex = 0;
            foreach (Item item in player.Equipment)
            {
                Console.SetCursorPosition(15, 17 + itemIndex);
                Console.Write($" - {item.GetDescription()}");
                itemIndex++;
            }

            if (player.Equipment.Count == 0)
            {
                Console.SetCursorPosition(15, 17);
                Console.Write(" - None");
            }

            // Weapon section
            Console.SetCursorPosition(15, 21);
            Console.Write("Weapons:");

            if (player.EquippedWeapon != null)
            {
                Console.SetCursorPosition(15, 22);
                Console.Write($" - {player.EquippedWeapon.GetDescription()}");
            }
            else
            {
                Console.SetCursorPosition(15, 22);
                Console.Write(" - None");
            }

            // Controls
            Console.SetCursorPosition(15, 36);
            Console.Write("Press [C] or [ESC] to return to game");
        }

        /// <summary>
        /// Renders the help screen
        /// </summary>
        public void RenderHelpScreen()
        {
            DrawFrame(10, 2, 80, 35, "Help");

            // Game description
            Console.SetCursorPosition(15, 5);
            Console.Write("Console Escape from Tarkov - A roguelike survival game");

            // Controls
            Console.SetCursorPosition(15, 8);
            Console.Write("Controls:");

            string[,] controls = {
                {"WASD", "Move"},
                {"SPACE", "Shoot"},
                {"R", "Reload"},
                {"E", "Interact with objects"},
                {"I", "Open inventory"},
                {"M", "View map"},
                {"C", "Character screen"},
                {"F", "Extract (when at extraction point)"},
                {"TAB", "Switch weapons"},
                {"1-5", "Use quickslot items"},
                {"P", "Pause game"},
                {"ESC", "Main menu"}
            };

            for (int i = 0; i < controls.GetLength(0); i++)
            {
                Console.SetCursorPosition(15, 9 + i);
                Console.Write($"{controls[i, 0],-10} - {controls[i, 1]}");
            }

            // Game tips
            Console.SetCursorPosition(15, 22);
            Console.Write("Tips:");

            string[] tips = {
                "Find and extract valuable items to increase your score",
                "Complete mission objectives for bonus XP",
                "Use medical items when your health is low",
                "Different enemy types have different behaviors",
                "Look for ammo caches to resupply your weapons",
                "Medical stations can heal you for free",
                "Extraction points are marked with 'X' on the map",
                "Loot containers can contain valuable items",
                "Level up to increase your max health and inventory space"
            };

            for (int i = 0; i < tips.Length; i++)
            {
                Console.SetCursorPosition(15, 23 + i);
                Console.Write($"• {tips[i]}");
            }

            // Return
            Console.SetCursorPosition(15, 36);
            Console.Write("Press [H] or [ESC] to return");
        }

        /// <summary>
        /// Renders the game over screen
        /// </summary>
        public void RenderGameOver()
        {
            string gameOver = @"
   _____                        ____                 
  / ____|                      / __ \                
 | |  __  __ _ _ __ ___   ___ | |  | |_   _____ _ __ 
 | | |_ |/ _` | '_ ` _ \ / _ \| |  | \ \ / / _ \ '__|
 | |__| | (_| | | | | | |  __/| |__| |\ V /  __/ |   
  \_____|\__,_|_| |_| |_|\___| \____/  \_/ \___|_|   
                                                                  
";

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(gameOver);
            Console.ResetColor();

            Console.WriteLine("\n\n     You died in the raid!");
            Console.WriteLine($"     Final Stats: Level {player.Level}, {player.KillCount} Kills, {player.GetTotalItemsValue()}₽ Worth of Items\n\n");

            Console.WriteLine("     [ENTER] Start New Game");
            Console.WriteLine("     [ESC] Main Menu\n\n");
        }

        /// <summary>
        /// Renders the pause screen
        /// </summary>
        public void RenderPausedScreen()
        {
            DrawFrame(30, 10, 40, 15, "Game Paused");

            Console.SetCursorPosition(37, 15);
            Console.Write("Game is paused");

            Console.SetCursorPosition(32, 17);
            Console.Write("[P] or [ESC] - Resume Game");

            Console.SetCursorPosition(32, 18);
            Console.Write("[M] - Exit to Main Menu");
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Draws a frame with a title
        /// </summary>
        private void DrawFrame(int x, int y, int width, int height, string title)
        {
            // Draw top border with title
            Console.SetCursorPosition(x, y);
            Console.Write('┌');
            Console.Write(new string('─', 3));
            Console.Write(' ' + title + ' ');
            Console.Write(new string('─', width - title.Length - 7));
            Console.Write('┐');

            // Draw sides
            for (int i = 1; i < height - 1; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write('│');
                Console.SetCursorPosition(x + width - 1, y + i);
                Console.Write('│');
            }

            // Draw bottom
            Console.SetCursorPosition(x, y + height - 1);
            Console.Write('└');
            Console.Write(new string('─', width - 2));
            Console.Write('┘');
        }

        /// <summary>
        /// Draws tab headers with one selected
        /// </summary>
        private void DrawTabs(int x, int y, string[] tabs, int selectedTab)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                Console.SetCursorPosition(x + i * 12, y);

                if (i == selectedTab)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write('[' + tabs[i] + ']');
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(' ' + tabs[i] + ' ');
                }
            }
        }

        /// <summary>
        /// Draws a list of items with selection
        /// </summary>
        private void DrawItemList<T>(List<T> items, int x, int y, int width, int maxItems) where T : Item
        {
            // Clear the item area
            for (int i = 0; i < maxItems; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(new string(' ', width));
            }

            // Draw items
            int visibleCount = Math.Min(items.Count, maxItems);

            for (int i = 0; i < visibleCount; i++)
            {
                Console.SetCursorPosition(x, y + i);

                if (i == selectedInventoryIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("► ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("  ");
                }

                Console.Write(items[i].GetDescription());
            }

            // Draw empty slots indication
            if (items.Count == 0)
            {
                Console.SetCursorPosition(x + 2, y);
                Console.Write("No items");
            }
        }

        /// <summary>
        /// Draws a list of weapons
        /// </summary>
        private void DrawWeaponList(int x, int y, int width, int maxItems)
        {
            // Clear the item area
            for (int i = 0; i < maxItems; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(new string(' ', width));
            }

            // Draw weapons
            int visibleCount = Math.Min(player.Weapons.Count, maxItems);

            for (int i = 0; i < visibleCount; i++)
            {
                Console.SetCursorPosition(x, y + i);

                // Mark equipped weapon
                if (player.EquippedWeapon == player.Weapons[i])
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("* ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("  ");
                }

                if (i == selectedInventoryIndex)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("► ");
                    Console.ResetColor();
                }
                else
                {
                    Console.Write("  ");
                }

                Console.Write(player.Weapons[i].GetDescription());
            }

            // Draw empty slots indication
            if (player.Weapons.Count == 0)
            {
                Console.SetCursorPosition(x + 2, y);
                Console.Write("No weapons");
            }
        }

        #endregion

        #region UI State Management

        /// <summary>
        /// Moves the inventory selector up or down
        /// </summary>
        /// <param name="direction">Direction to move (positive down, negative up)</param>
        public void MoveInventorySelector(int direction)
        {
            List<Item> currentList = null;

            // Get the current list based on tab
            switch (currentInventoryTab)
            {
                case 0:
                    currentList = player.Inventory;
                    break;
                case 1:
                    currentList = player.Equipment;
                    break;
                case 2:
                    currentList = null; // We'll handle weapons separately
                    break;
            }

            // Handle weapons tab
            if (currentInventoryTab == 2)
            {
                int count = player.Weapons.Count;
                if (count > 0)
                {
                    selectedInventoryIndex = (selectedInventoryIndex + direction + count) % count;
                }
                return;
            }

            // Handle items tab
            if (currentList != null && currentList.Count > 0)
            {
                selectedInventoryIndex = (selectedInventoryIndex + direction + currentList.Count) % currentList.Count;
            }
        }

        /// <summary>
        /// Switches the inventory tab
        /// </summary>
        /// <param name="direction">Direction to move (positive right, negative left)</param>
        public void SwitchInventoryTab(int direction)
        {
            int tabCount = 3; // Total number of tabs
            currentInventoryTab = (currentInventoryTab + direction + tabCount) % tabCount;
            selectedInventoryIndex = 0; // Reset selection on tab change
        }

        /// <summary>
        /// Moves the loot container selector up or down
        /// </summary>
        /// <param name="direction">Direction to move (positive down, negative up)</param>
        public void MoveLootSelector(int direction)
        {
            if (activeLootContainer != null && activeLootContainer.Items.Count > 0)
            {
                selectedLootIndex = (selectedLootIndex + direction + activeLootContainer.Items.Count) % activeLootContainer.Items.Count;
            }
        }

        /// <summary>
        /// Sets the active loot container for the looting UI
        /// </summary>
        /// <param name="container">Container to loot</param>
        public void SetActiveLootContainer(LootContainer container)
        {
            activeLootContainer = container;
        }

        /// <summary>
        /// Resets the loot selector to the first item
        /// </summary>
        public void ResetLootSelector()
        {
            selectedLootIndex = 0;
        }

        #endregion
    }
}