using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.ObjectManagement;

namespace ConsoleEscapeFromTarkov.Entities
{
    /// <summary>
    /// Manages all enemies in the game
    /// </summary>
    public class EnemyManager
    {
        private List<Enemy> enemies;
        private World world;
        private Player player;
        private LootManager lootManager;
        private ObjectManager objectManager;
        private MessageLog messageLog;
        private Random random;

        /// <summary>
        /// List of all active enemies
        /// </summary>
        public List<Enemy> Enemies => enemies;

        /// <summary>
        /// Constructor for EnemyManager
        /// </summary>
        /// <param name="world">World reference</param>
        /// <param name="player">Player reference</param>
        /// <param name="lootManager">Loot manager reference</param>
        /// <param name="objectManager">Object manager reference</param>
        /// <param name="messageLog">Message log reference</param>
        public EnemyManager(World world, Player player, LootManager lootManager, ObjectManager objectManager, MessageLog messageLog)
        {
            this.world = world;
            this.player = player;
            this.lootManager = lootManager;
            this.objectManager = objectManager;
            this.messageLog = messageLog;
            enemies = new List<Enemy>();
            random = new Random();
        }

        /// <summary>
        /// Generates enemies for a new raid
        /// </summary>
        /// <param name="world">World reference</param>
        /// <param name="count">Base number of enemies to generate</param>
        public void GenerateEnemies(World world, int count)
        {
            enemies.Clear();

            // Scale enemy count based on player level
            int scaledCount = count + (player.Level - 1) / 2;

            for (int i = 0; i < scaledCount; i++)
            {
                int tries = 0;
                int maxTries = 100;

                while (tries < maxTries)
                {
                    int x = random.Next(2, world.Width - 2);
                    int y = random.Next(2, world.Height - 2);

                    // Check if position is valid (not a wall, not on player, not on another enemy)
                    if (!world.IsCollision(x, y) &&
                        !(player.X == x && player.Y == y) &&
                        !IsEnemyAt(x, y) &&
                        !lootManager.IsLootAt(x, y) &&
                        Math.Abs(player.X - x) + Math.Abs(player.Y - y) > 10) // Not too close to player
                    {
                        enemies.Add(new Enemy(x, y, world, player, lootManager, objectManager, messageLog));
                        break;
                    }

                    tries++;
                }
            }
        }

        /// <summary>
        /// Spawns a random enemy away from the player
        /// </summary>
        public void SpawnRandomEnemy()
        {
            // Find a valid spawn point away from the player
            for (int attempts = 0; attempts < 50; attempts++)
            {
                int x = random.Next(2, world.Width - 2);
                int y = random.Next(2, world.Height - 2);

                // Check if it's far enough from player and valid
                if (!world.IsCollision(x, y) &&
                    !IsEnemyAt(x, y) &&
                    !lootManager.IsLootAt(x, y) &&
                    Math.Abs(player.X - x) + Math.Abs(player.Y - y) > 15)
                {
                    enemies.Add(new Enemy(x, y, world, player, lootManager, objectManager, messageLog));
                    messageLog.AddMessage("You hear movement in the distance...");
                    return;
                }
            }
        }

        /// <summary>
        /// Updates all enemies
        /// </summary>
        public void Update()
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                enemy.Update();

                if (!enemy.IsAlive)
                {
                    // Remove dead enemies
                    enemies.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Renders all enemies
        /// </summary>
        /// <param name="world">World to render on</param>
        public void Render(World world)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Render(world);
            }
        }

        /// <summary>
        /// Checks if an enemy is at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if an enemy is at the position</returns>
        public bool IsEnemyAt(int x, int y)
        {
            return enemies.Any(e => e.X == x && e.Y == y && e.IsAlive);
        }

        /// <summary>
        /// Gets an enemy at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Enemy at the position, or null if none</returns>
        public Enemy GetEnemyAt(int x, int y)
        {
            return enemies.FirstOrDefault(e => e.X == x && e.Y == y && e.IsAlive);
        }

        /// <summary>
        /// Gets the count of enemies of a specific type
        /// </summary>
        /// <param name="type">Enemy type to count</param>
        /// <returns>Number of enemies of that type</returns>
        public int GetEnemyCountByType(Enemy.EnemyType type)
        {
            return enemies.Count(e => e.Type == type && e.IsAlive);
        }

        /// <summary>
        /// Gets the closest enemy to a position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The closest enemy, or null if none</returns>
        public Enemy GetClosestEnemy(int x, int y)
        {
            if (enemies.Count == 0)
                return null;

            Enemy closest = null;
            int closestDistance = int.MaxValue;

            foreach (Enemy enemy in enemies)
            {
                if (!enemy.IsAlive)
                    continue;

                int distance = Math.Abs(x - enemy.X) + Math.Abs(y - enemy.Y);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }
    }
}