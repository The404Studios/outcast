using System;
using System.Diagnostics;
using System.Threading;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.ObjectManagement;
using ConsoleEscapeFromTarkov.UI;
using ConsoleEscapeFromTarkov.Utils;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Central manager for game state, loop, and component coordination
    /// </summary>
    public class GameManager
    {
        // Game components
        private World world;
        private Player player;
        private UIManager uiManager;
        private InputHandler inputHandler;
        private LootManager lootManager;
        private EnemyManager enemyManager;
        private ObjectManager objectManager;
        private MessageLog messageLog;
        private MissionManager missionManager;
        private WeatherSystem weatherSystem;

        // Game state
        private bool isRunning;
        private GameState currentState;
        private GameState previousState;
        private Stopwatch frameTimer;
        private long targetFrameTimeMs = 50; // 20 FPS target (increased from 10 FPS to reduce input lag)
        private int gameTime; // In-game time counter
        private Random random;

        /// <summary>
        /// Game state enum defining the different screens/modes
        /// </summary>
        public enum GameState
        {
            MainMenu,
            Playing,
            Inventory,
            Looting,
            GameOver,
            Paused,
            Map,
            Character,
            Help
        }

        /// <summary>
        /// Constructor for GameManager, initializes all game components
        /// </summary>
        public GameManager()
        {
            isRunning = true;
            currentState = GameState.MainMenu;
            previousState = GameState.MainMenu;
            frameTimer = new Stopwatch();
            gameTime = 0;
            random = new Random();

            // Calculate world size to fit in console window
            int worldWidth = Constants.WorldWidth;
            int worldHeight = Constants.WorldHeight;

            // Initialize object manager first for pooling
            objectManager = new ObjectManager();

            // Initialize message log
            messageLog = new MessageLog(10);

            // Initialize game components
            world = new World(worldWidth, worldHeight);
            lootManager = new LootManager(objectManager);
            player = new Player(10, 10, world, lootManager, objectManager, messageLog);
            enemyManager = new EnemyManager(world, player, lootManager, objectManager, messageLog);
            missionManager = new MissionManager(player, enemyManager, lootManager);
            weatherSystem = new WeatherSystem();
            inputHandler = new InputHandler();
            uiManager = new UIManager(world, player, lootManager, enemyManager, messageLog, missionManager, weatherSystem);
        }

        /// <summary>
        /// Starts the game loop
        /// </summary>
        public void StartGame()
        {
            Console.Clear();

            // Initial delay to give the console time to stabilize
            Thread.Sleep(100);

            // Render once to initialize the screen
            uiManager.RenderBufferedGameUI();

            while (isRunning)
            {
                frameTimer.Restart();

                // Handle input based on current state
                HandleInput();

                // Update game state
                Update();

                // Render the current frame
                Render();

                // Wait to maintain stable frame rate
                frameTimer.Stop();
                int sleepTime = (int)(targetFrameTimeMs - frameTimer.ElapsedMilliseconds);
                if (sleepTime > 0)
                {
                    Thread.Sleep(sleepTime);
                }
            }
        }

        #region Input Handling

        /// <summary>
        /// Routes input handling based on current game state
        /// </summary>
        private void HandleInput()
        {
            ConsoleKeyInfo? keyInfo = inputHandler.GetKeyPress();
            if (keyInfo.HasValue)
            {
                switch (currentState)
                {
                    case GameState.MainMenu:
                        HandleMainMenuInput(keyInfo.Value);
                        break;
                    case GameState.Playing:
                        HandleGameplayInput(keyInfo.Value);
                        break;
                    case GameState.Inventory:
                        HandleInventoryInput(keyInfo.Value);
                        break;
                    case GameState.Looting:
                        HandleLootingInput(keyInfo.Value);
                        break;
                    case GameState.GameOver:
                        HandleGameOverInput(keyInfo.Value);
                        break;
                    case GameState.Paused:
                        HandlePausedInput(keyInfo.Value);
                        break;
                    case GameState.Map:
                        HandleMapInput(keyInfo.Value);
                        break;
                    case GameState.Character:
                        HandleCharacterInput(keyInfo.Value);
                        break;
                    case GameState.Help:
                        HandleHelpInput(keyInfo.Value);
                        break;
                }
            }
        }

        private void HandleMainMenuInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    StartNewGame();
                    break;
                case ConsoleKey.H:
                    SetGameState(GameState.Help);
                    break;
                case ConsoleKey.Escape:
                    isRunning = false;
                    break;
            }
        }

        private void HandleGameplayInput(ConsoleKeyInfo keyInfo)
        {
            // Make sure player controls are only processed in gameplay state
            if (currentState != GameState.Playing)
                return;

            switch (keyInfo.Key)
            {
                case ConsoleKey.W:
                    player.Move(0, -1);
                    break;
                case ConsoleKey.S:
                    player.Move(0, 1);
                    break;
                case ConsoleKey.A:
                    player.Move(-1, 0);
                    break;
                case ConsoleKey.D:
                    player.Move(1, 0);
                    break;
                case ConsoleKey.Spacebar:
                    player.Shoot();
                    break;
                case ConsoleKey.I:
                    OpenInventory();
                    break;
                case ConsoleKey.E:
                    TryInteract();
                    break;
                case ConsoleKey.R:
                    player.Reload();
                    break;
                case ConsoleKey.C:
                    SetGameState(GameState.Character);
                    break;
                case ConsoleKey.M:
                    SetGameState(GameState.Map);
                    break;
                case ConsoleKey.H:
                    SetGameState(GameState.Help);
                    break;
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                    // Quick-use items from hotbar (1-5)
                    int slotIndex = (int)keyInfo.Key - (int)ConsoleKey.D1;
                    player.QuickUseItem(slotIndex);
                    break;
                case ConsoleKey.F:
                    // Extract if at extraction point
                    if (world.IsExtractionPoint(player.X, player.Y))
                    {
                        messageLog.AddMessage("Extracting from raid...");
                        Thread.Sleep(1000); // Simulate extraction time
                        EndMission(true);
                    }
                    break;
                case ConsoleKey.Tab:
                    // Cycle through weapon slots
                    player.CycleWeapons();
                    break;
                case ConsoleKey.P:
                    PauseGame();
                    break;
                case ConsoleKey.Escape:
                    currentState = GameState.MainMenu;
                    Console.Clear();
                    break;
            }
        }

        private void HandleInventoryInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.I:
                case ConsoleKey.Escape:
                    CloseInventory();
                    break;
                case ConsoleKey.UpArrow:
                    uiManager.MoveInventorySelector(-1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.DownArrow:
                    uiManager.MoveInventorySelector(1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.LeftArrow:
                    uiManager.SwitchInventoryTab(-1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.RightArrow:
                    uiManager.SwitchInventoryTab(1);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.Enter:
                    player.UseSelectedItem(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.Delete:
                    player.DropSelectedItem(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.Tab:
                    player.EquipSelectedItem(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab);
                    uiManager.RenderInventory();
                    break;
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                    // Assign to quickslot
                    int slotIndex = (int)keyInfo.Key - (int)ConsoleKey.D1;
                    player.AssignToQuickSlot(uiManager.SelectedInventoryIndex, uiManager.CurrentInventoryTab, slotIndex);
                    uiManager.RenderInventory();
                    break;
            }
        }

        private void HandleLootingInput(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.E:
                case ConsoleKey.Escape:
                    CloseLootingMenu();
                    break;
                case ConsoleKey.UpArrow:
                    uiManager.MoveLootSelector(-1);
                    uiManager.RenderLootingUI();
                    break;
                case ConsoleKey.DownArrow:
                    uiManager.MoveLootSelector(1);
                    uiManager.RenderLootingUI();
                    break;
                case ConsoleKey.Enter:
                    LootContainer container = player.GetNearbyLootContainer();
                    if (container != null && uiManager.SelectedLootIndex < container.Items.Count)
                    {
                        Item item = container.Items[uiManager.SelectedLootIndex];
                        bool taken = player.TakeLootItem(container, item);

                        if (taken)
                        {
                            messageLog.AddMessage($"Picked up {item.Name}");
                        }
                        else
                        {
                            messageLog.AddMessage("Inventory full!");
                        }

                        // Redraw looting UI to update items
                        uiManager.RenderLootingUI();

                        // If container is now empty, exit looting state
                        if (container.Items.Count == 0)
                        {
                            CloseLootingMenu();
                        }
                    }
                    break;
                case ConsoleKey.T:
                    // Take all items
                    LootContainer cont = player.GetNearbyLootContainer();
                    if (cont != null && cont.Items.Count > 0)
                    {
                        int takenCount = 0;

                        foreach (Item item in cont.Items.ToList())
                        {
                            if (player.TakeLootItem(cont, item))
                            {
                                takenCount++;
                            }
                            else
                            {
                                messageLog.AddMessage("Inventory full!");
                                break;
                            }
                        }

                        if (takenCount > 0)
                        {
                            messageLog.AddMessage($"Took {takenCount} items");
                        }

                        // If container is now empty, exit looting state
                        if (cont.Items.Count == 0)
                        {
                            CloseLootingMenu();
                        }
                        else
                        {
                            // Otherwise redraw looting UI
                            uiManager.RenderLootingUI();
                        }
                    }
                    break;
            }
        }

        private void HandleGameOverInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                StartNewGame();
            }
            else if (keyInfo.Key == ConsoleKey.Escape)
            {
                currentState = GameState.MainMenu;
                Console.Clear();
            }
        }

        private void HandlePausedInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.P || keyInfo.Key == ConsoleKey.Escape)
            {
                ResumeGame();
            }
            else if (keyInfo.Key == ConsoleKey.M)
            {
                // Exit to main menu
                SetGameState(GameState.MainMenu);
                Console.Clear();
            }
        }

        private void HandleMapInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.M || keyInfo.Key == ConsoleKey.Escape)
            {
                CloseMap();
            }
        }

        private void HandleCharacterInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.C || keyInfo.Key == ConsoleKey.Escape)
            {
                CloseCharacterScreen();
            }
        }

        private void HandleHelpInput(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.H || keyInfo.Key == ConsoleKey.Escape)
            {
                CloseHelpScreen();
            }
        }

        #endregion

        #region Game State Management

        /// <summary>
        /// Updates game state for the current frame
        /// </summary>
        private void Update()
        {
            // Only update game logic if we're in Playing state
            if (currentState == GameState.Playing)
            {
                // Update game time
                gameTime++;

                // Update weather conditions every 500 ticks
                if (gameTime % 500 == 0)
                {
                    weatherSystem.UpdateWeather();
                    messageLog.AddMessage($"Weather changed: {weatherSystem.CurrentWeather.ToString()}");
                }

                // Update mission objectives
                missionManager.Update();

                // Update all game objects
                objectManager.Update();

                // Update game entities
                enemyManager.Update();
                player.Update();

                // Check if player is dead
                if (player.Health <= 0)
                {
                    SetGameState(GameState.GameOver);
                    messageLog.AddMessage("You died!");
                }

                // Spawn additional enemies periodically
                if (gameTime % 500 == 0 && enemyManager.Enemies.Count < 15)
                {
                    enemyManager.SpawnRandomEnemy();
                }
            }
        }

        /// <summary>
        /// Renders the current frame based on game state
        /// </summary>
        private void Render()
        {
            switch (currentState)
            {
                case GameState.MainMenu:
                    Console.Clear();
                    uiManager.RenderMainMenu();
                    break;
                case GameState.Playing:
                    // Instead of our buffer system, go back to direct rendering but optimize it
                    world.PrepareForRendering();

                    // Apply weather effects
                    weatherSystem.ApplyWeatherEffects(world);

                    // Render world entities
                    lootManager.Render(world);
                    enemyManager.Render(world);
                    player.Render(world);
                    objectManager.Render(world);

                    // Render map decorations and mission objectives
                    world.RenderMapFeatures();
                    missionManager.RenderObjectives(world);

                    // Use direct but optimized rendering
                    uiManager.RenderWorldDirectly();
                    uiManager.RenderGameUI();
                    break;
                case GameState.Inventory:
                    RenderInventoryScreen();
                    break;
                case GameState.Looting:
                    RenderLootingScreen();
                    break;
                case GameState.GameOver:
                    Console.Clear();
                    uiManager.RenderGameOver();
                    break;
                case GameState.Paused:
                    uiManager.RenderPausedScreen();
                    break;
                case GameState.Map:
                    uiManager.RenderMapScreen();
                    break;
                case GameState.Character:
                    uiManager.RenderCharacterScreen();
                    break;
                case GameState.Help:
                    uiManager.RenderHelpScreen();
                    break;
            }
        }

        private void RenderInventoryScreen()
        {
            // For inventory, we keep the world visible underneath
            if (previousState == GameState.Playing)
            {
                // First render the world underneath
                world.PrepareForRendering();
                weatherSystem.ApplyWeatherEffects(world);
                lootManager.Render(world);
                enemyManager.Render(world);
                player.Render(world);
                objectManager.Render(world);
                world.RenderMapFeatures();
                uiManager.RenderWorldEfficient();
            }

            // Then render inventory over top
            uiManager.RenderInventory();
        }

        private void RenderLootingScreen()
        {
            // For looting, we keep the world visible underneath
            if (previousState == GameState.Playing)
            {
                // First render the world underneath
                world.PrepareForRendering();
                weatherSystem.ApplyWeatherEffects(world);
                lootManager.Render(world);
                enemyManager.Render(world);
                player.Render(world);
                objectManager.Render(world);
                world.RenderMapFeatures();
                uiManager.RenderWorldEfficient();
            }

            // Then render looting UI over top
            uiManager.RenderLootingUI();
        }

        #endregion

        #region Game Actions

        /// <summary>
        /// Starts a new game, resetting everything
        /// </summary>
        private void StartNewGame()
        {
            // Reset game time
            gameTime = 0;

            // Reset message log
            messageLog.Clear();

            // Reset object pools
            objectManager.Reset();

            // Generate a new world
            world.Generate();

            // Reset player
            player.Reset(world.Width / 2, world.Height / 2);

            // Generate loot and enemies
            lootManager.GenerateLoot(world);
            enemyManager.GenerateEnemies(world, 10);

            // Set up missions
            missionManager.SetupMissions();

            // Reset weather
            weatherSystem.Reset();

            // Welcome message
            messageLog.AddMessage("Welcome to Console Escape from Tarkov!");
            messageLog.AddMessage("Find valuable loot and extract safely.");

            // Clear the screen before starting new game
            Console.Clear();

            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Ends the current mission/raid
        /// </summary>
        /// <param name="success">Whether the player successfully extracted</param>
        private void EndMission(bool success)
        {
            if (success)
            {
                messageLog.AddMessage("Mission successful! You extracted with your loot.");
                player.AddExperience(500); // Base XP for extraction

                // Add bonus XP for completed objectives
                int completedObjectives = missionManager.GetCompletedObjectivesCount();
                if (completedObjectives > 0)
                {
                    int bonusXP = completedObjectives * 250;
                    player.AddExperience(bonusXP);
                    messageLog.AddMessage($"Completed {completedObjectives} objectives: +{bonusXP} XP");
                }

                // Keep player stats but reset the world
                player.SaveProgress();
            }
            else
            {
                messageLog.AddMessage("Mission failed. You lost all your loot.");
                // Player keeps base stats but loses mission items
                player.ResetMissionItems();
            }

            // Start a new raid
            StartNewGame();
        }

        private void OpenInventory()
        {
            SetGameState(GameState.Inventory);
        }

        private void CloseInventory()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void TryInteract()
        {
            // Check for loot containers
            LootContainer container = player.GetNearbyLootContainer();
            if (container != null)
            {
                SetGameState(GameState.Looting);
                uiManager.SetActiveLootContainer(container);
                uiManager.ResetLootSelector();
                return;
            }

            // Check for interactable map features
            MapFeature feature = world.GetFeatureAt(player.X, player.Y);
            if (feature != null)
            {
                feature.Interact(player, messageLog);
                return;
            }

            // Check for extraction points
            if (world.IsExtractionPoint(player.X, player.Y))
            {
                messageLog.AddMessage("Press F to extract from the raid");
                return;
            }

            messageLog.AddMessage("Nothing to interact with here.");
        }

        private void CloseLootingMenu()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void CloseMap()
        {
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void CloseCharacterScreen()
        {
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        private void CloseHelpScreen()
        {
            if (previousState == GameState.MainMenu)
            {
                Console.Clear();
                SetGameState(GameState.MainMenu);
            }
            else
            {
                Console.Clear();
                SetGameState(GameState.Playing);
            }
        }

        private void PauseGame()
        {
            SetGameState(GameState.Paused);
        }

        private void ResumeGame()
        {
            // Refresh the screen when returning to gameplay
            Console.Clear();
            SetGameState(GameState.Playing);
        }

        /// <summary>
        /// Changes the current game state and stores the previous state
        /// </summary>
        /// <param name="newState">The new game state</param>
        private void SetGameState(GameState newState)
        {
            previousState = currentState;
            currentState = newState;
        }

        #endregion
    }
}