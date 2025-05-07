using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.ObjectManagement;

namespace ConsoleEscapeFromTarkov.Items
{
    /// <summary>
    /// Manages loot generation and placement in the world
    /// </summary>
    public class LootManager
    {
        private ObjectManager objectManager;
        private Random random;
        private List<Item> missionItems;

        /// <summary>
        /// Constructor for LootManager
        /// </summary>
        /// <param name="objectManager">Object manager reference</param>
        public LootManager(ObjectManager objectManager)
        {
            this.objectManager = objectManager;
            random = new Random();
            missionItems = new List<Item>();
        }

        /// <summary>
        /// Generates and places loot in the world
        /// </summary>
        /// <param name="world">World to place loot in</param>
        public void GenerateLoot(World world)
        {
            // Generate common containers
            int containerCount = random.Next(15, 25);

            for (int i = 0; i < containerCount; i++)
            {
                int tries = 0;
                int maxTries = 100;

                while (tries < maxTries)
                {
                    int x = random.Next(2, world.Width - 2);
                    int y = random.Next(2, world.Height - 2);

                    if (!world.IsCollision(x, y) && !IsLootAt(x, y))
                    {
                        LootContainer container = objectManager.GetLootContainer();
                        container.Initialize(x, y, "Container");
                        GenerateRandomLoot(container);
                        break;
                    }

                    tries++;
                }
            }

            // Place mission items in special locations
            PlaceMissionItems(world);

            // Generate some specific loot crates
            GenerateSpecialCrates(world);
        }

        /// <summary>
        /// Places mission items in suitable locations
        /// </summary>
        /// <param name="world">World to place in</param>
        private void PlaceMissionItems(World world)
        {
            // Place any mission items in suitable locations
            foreach (Item item in missionItems)
            {
                PlaceMissionItem(world, item);
            }

            // Clear the mission items list after placing them
            missionItems.Clear();
        }

        /// <summary>
        /// Places a single mission item in a suitable location
        /// </summary>
        /// <param name="world">World to place in</param>
        /// <param name="item">Item to place</param>
        private void PlaceMissionItem(World world, Item item)
        {
            // Try to place in a suitable location
            bool placed = false;

            // Check if there are buildings or warehouses
            List<MapFeature> features = world.GetMapFeatures()
                .Where(f => f.Name.Contains("Building") ||
                           f.Name.Contains("Warehouse") ||
                           f.Name.Contains("Container"))
                .ToList();

            if (features.Count > 0)
            {
                // Pick a random suitable feature
                MapFeature feature = features[random.Next(features.Count)];

                // Find a nearby valid spot
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        int x = feature.X + dx;
                        int y = feature.Y + dy;

                        if (!world.IsCollision(x, y) && !IsLootAt(x, y))
                        {
                            LootContainer container = objectManager.GetLootContainer();
                            container.Initialize(x, y, "Valuable Container");
                            container.AddItem(item);
                            placed = true;
                            break;
                        }
                    }
                    if (placed) break;
                }
            }

            // If couldn't place near a feature, place it randomly
            if (!placed)
            {
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(2, world.Width - 2);
                    int y = random.Next(2, world.Height - 2);

                    if (!world.IsCollision(x, y) && !IsLootAt(x, y))
                    {
                        LootContainer container = objectManager.GetLootContainer();
                        container.Initialize(x, y, "Valuable Container");
                        container.AddItem(item);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Generates special loot crates with high-value items
        /// </summary>
        /// <param name="world">World to place in</param>
        private void GenerateSpecialCrates(World world)
        {
            // Weapon crate
            CreateSpecialCrate(world, "Weapon Crate", (container) => {
                // High-tier weapon
                switch (random.Next(3))
                {
                    case 0:
                        container.AddItem(new Weapon("Shotgun", 40, 6, 8, 8, 1)); // Spread weapon
                        container.AddItem(new Ammo("Shotgun Shells", "Shotgun", random.Next(10, 20)));
                        break;
                    case 1:
                        container.AddItem(new Weapon("Rifle", 30, 20, 20, 3));
                        container.AddItem(new Ammo("Rifle Ammo", "Rifle", random.Next(15, 30)));
                        break;
                    case 2:
                        container.AddItem(new Weapon("SMG", 8, 30, 15, 1)); // Fast firing
                        container.AddItem(new Ammo("SMG Ammo", "SMG", random.Next(20, 50)));
                        break;
                }
            });

            // Medical crate
            CreateSpecialCrate(world, "Medical Crate", (container) => {
                container.AddItem(new MedKit("Large Medkit", 70));
                container.AddItem(new MedKit("Small Medkit", 30));
                container.AddItem(new MedKit("Small Medkit", 30));

                // Chance for rare item
                if (random.Next(3) == 0)
                {
                    container.AddItem(new MedKit("Military Medkit", 100));
                }
            });

            // Valuable crate
            CreateSpecialCrate(world, "Valuable Crate", (container) => {
                container.AddItem(new Valuable("Gold Chain", 5000));
                container.AddItem(new Valuable("Watch", 2000));

                // Chance for rare valuable
                if (random.Next(3) == 0)
                {
                    container.AddItem(new Valuable("Diamond", 10000));
                }
                else
                {
                    container.AddItem(new Valuable("Silver Bar", 3000));
                }
            });

            // Armor crate
            CreateSpecialCrate(world, "Armor Crate", (container) => {
                container.AddItem(new Armor("Light Armor", 15));

                // Chance for better armor
                if (random.Next(3) == 0)
                {
                    container.AddItem(new Armor("Heavy Armor", 30));
                }
                else
                {
                    container.AddItem(new Armor("Medium Armor", 20));
                }
            });
        }

        /// <summary>
        /// Creates a special loot crate with a custom item generator
        /// </summary>
        /// <param name="world">World to place in</param>
        /// <param name="name">Name of the crate</param>
        /// <param name="populateAction">Action to populate the crate with items</param>
        private void CreateSpecialCrate(World world, string name, Action<LootContainer> populateAction)
        {
            int tries = 0;
            int maxTries = 100;

            while (tries < maxTries)
            {
                int x = random.Next(5, world.Width - 5);
                int y = random.Next(5, world.Height - 5);

                if (!world.IsCollision(x, y) && !IsLootAt(x, y))
                {
                    LootContainer container = objectManager.GetLootContainer();
                    container.Initialize(x, y, name);
                    populateAction(container);
                    break;
                }

                tries++;
            }
        }

        /// <summary>
        /// Generates random loot for a container
        /// </summary>
        /// <param name="container">Container to fill</param>
        private void GenerateRandomLoot(LootContainer container)
        {
            int itemCount = random.Next(1, 4);

            for (int i = 0; i < itemCount; i++)
            {
                int itemType = random.Next(7);

                switch (itemType)
                {
                    case 0:
                        container.AddItem(new Ammo("9mm Ammo", "Pistol", random.Next(5, 15)));
                        break;
                    case 1:
                        container.AddItem(new Ammo("Rifle Ammo", "Rifle", random.Next(5, 15)));
                        break;
                    case 2:
                        container.AddItem(new MedKit("Small Medkit", 30));
                        break;
                    case 3:
                        container.AddItem(new Armor("Light Armor", 15));
                        break;
                    case 4:
                        container.AddItem(new Valuable("Cash", random.Next(100, 1000)));
                        break;
                    case 5:
                        if (random.Next(3) == 0) // Rarer weapon drop
                        {
                            container.AddItem(new Weapon("Rifle", 30, 20, 20, 3));
                        }
                        break;
                    case 6:
                        if (random.Next(5) == 0) // Very rare item
                        {
                            container.AddItem(new Weapon("Sniper Rifle", 80, 5, 40, 10));
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Adds a mission item to be placed in the world
        /// </summary>
        /// <param name="item">Mission item to add</param>
        public void AddMissionItem(Item item)
        {
            missionItems.Add(item);
        }

        /// <summary>
        /// Renders loot containers
        /// </summary>
        /// <param name="world">World to render on</param>
        public void Render(World world)
        {
            // Rendering is now handled in the ObjectManager
        }

        /// <summary>
        /// Checks if there is loot at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if loot is at the position</returns>
        public bool IsLootAt(int x, int y)
        {
            return objectManager.GetActiveLootContainers().Any(c => c.X == x && c.Y == y);
        }

        /// <summary>
        /// Gets the loot container at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Loot container at position, or null if none</returns>
        public LootContainer GetLootAt(int x, int y)
        {
            return objectManager.GetActiveLootContainers().FirstOrDefault(c => c.X == x && c.Y == y);
        }
    }
}