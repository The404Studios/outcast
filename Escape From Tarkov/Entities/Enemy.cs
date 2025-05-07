using System;
using System.Collections.Generic;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.ObjectManagement;

namespace ConsoleEscapeFromTarkov.Entities
{
    /// <summary>
    /// AI-controlled enemy entity
    /// </summary>
    public class Enemy
    {
        private World world;
        private Player player;
        private LootManager lootManager;
        private ObjectManager objectManager;
        private MessageLog messageLog;
        private Random random;
        private int moveTimer;
        private int shootTimer;
        private EnemyType type;
        private string name;

        /// <summary>
        /// X coordinate of the enemy
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate of the enemy
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Current health of the enemy
        /// </summary>
        public int Health { get; private set; }

        /// <summary>
        /// Maximum health of the enemy
        /// </summary>
        public int MaxHealth { get; private set; }

        /// <summary>
        /// Range at which the enemy can detect the player
        /// </summary>
        public int ViewRange { get; private set; }

        /// <summary>
        /// Whether the enemy is alive
        /// </summary>
        public bool IsAlive => Health > 0;

        /// <summary>
        /// Type of the enemy
        /// </summary>
        public EnemyType Type => type;

        /// <summary>
        /// Name of the enemy
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Types of enemies
        /// </summary>
        public enum EnemyType
        {
            /// <summary>Basic enemy</summary>
            Scav,
            /// <summary>Tougher enemy with more health and damage</summary>
            HeavyScav,
            /// <summary>Long range with high damage</summary>
            Sniper,
            /// <summary>Fast moving enemy that charges the player</summary>
            Rusher
        }

        /// <summary>
        /// Constructor for Enemy
        /// </summary>
        /// <param name="x">X position</param>
        /// <param name="y">Y position</param>
        /// <param name="world">World reference</param>
        /// <param name="player">Player reference</param>
        /// <param name="lootManager">Loot manager reference</param>
        /// <param name="objectManager">Object manager reference</param>
        /// <param name="messageLog">Message log reference</param>
        public Enemy(int x, int y, World world, Player player, LootManager lootManager, ObjectManager objectManager, MessageLog messageLog)
        {
            this.world = world;
            this.player = player;
            this.lootManager = lootManager;
            this.objectManager = objectManager;
            this.messageLog = messageLog;
            X = x;
            Y = y;

            random = new Random();
            moveTimer = 0;
            shootTimer = 0;

            // Determine enemy type based on player level and randomness
            SetEnemyType();
        }

        /// <summary>
        /// Sets the enemy type and corresponding attributes
        /// </summary>
        private void SetEnemyType()
        {
            int roll = random.Next(100);

            // Bias towards more dangerous enemies as player levels up
            int levelBias = Math.Min(player.Level * 5, 40); // Max 40% bias from levels

            if (roll < 50 - levelBias)
            {
                type = EnemyType.Scav;
                name = "Scav";
                Health = MaxHealth = 50;
                ViewRange = 15;
            }
            else if (roll < 80 - levelBias / 2)
            {
                type = EnemyType.HeavyScav;
                name = "Heavy Scav";
                Health = MaxHealth = 100;
                ViewRange = 12;
            }
            else if (roll < 95)
            {
                type = EnemyType.Sniper;
                name = "Sniper";
                Health = MaxHealth = 60;
                ViewRange = 20;
            }
            else
            {
                type = EnemyType.Rusher;
                name = "Rusher";
                Health = MaxHealth = 70;
                ViewRange = 10;
            }
        }

        /// <summary>
        /// Updates the enemy's state and AI
        /// </summary>
        public void Update()
        {
            if (!IsAlive) return;

            // Check if player projectiles hit this enemy
            foreach (Projectile projectile in objectManager.GetActiveProjectiles())
            {
                if (projectile.X == X && projectile.Y == Y && projectile.IsPlayerProjectile)
                {
                    TakeDamage(projectile.Damage);
                    projectile.Deactivate();

                    // Create hit effect
                    VisualEffect hitEffect = objectManager.GetEffect();
                    hitEffect.Initialize(X, Y, '*', 3);

                    if (!IsAlive)
                    {
                        messageLog.AddMessage($"Killed {name}!");
                        player.IncrementKillCount();
                        player.AddExperience(type == EnemyType.Scav ? 25 :
                                             type == EnemyType.HeavyScav ? 50 :
                                             type == EnemyType.Sniper ? 75 : 100);
                        DropLoot();
                        return;
                    }
                }
            }

            // Calculate distance to player
            int distanceToPlayer = Math.Abs(player.X - X) + Math.Abs(player.Y - Y);

            // Only act if player is in view range
            if (distanceToPlayer <= ViewRange)
            {
                // Simple AI based on enemy type
                moveTimer--;
                shootTimer--;

                if (moveTimer <= 0)
                {
                    int moveRate = type == EnemyType.Rusher ? 2 :
                                  type == EnemyType.Sniper ? 6 : 4;

                    MoveBasedOnType();
                    moveTimer = random.Next(moveRate - 1, moveRate + 2);
                }

                if (shootTimer <= 0 && IsPlayerInRange(GetAttackRange()))
                {
                    ShootAtPlayer();

                    int shootDelay = type == EnemyType.Sniper ? 8 :
                                    type == EnemyType.HeavyScav ? 6 : 5;

                    shootTimer = random.Next(shootDelay - 1, shootDelay + 2);
                }
            }
        }

        /// <summary>
        /// Gets the attack range based on enemy type
        /// </summary>
        /// <returns>Attack range in tiles</returns>
        private int GetAttackRange()
        {
            return type == EnemyType.Sniper ? 20 :
                   type == EnemyType.HeavyScav ? 10 :
                   type == EnemyType.Rusher ? 8 : 12;
        }

        /// <summary>
        /// Moves the enemy based on its type and AI strategy
        /// </summary>
        private void MoveBasedOnType()
        {
            switch (type)
            {
                case EnemyType.Scav:
                    // Basic movement toward player
                    MoveTowardsPlayer();
                    break;
                case EnemyType.HeavyScav:
                    // Slower, more deliberate movement
                    if (random.Next(2) == 0) // 50% chance to move
                    {
                        MoveTowardsPlayer();
                    }
                    break;
                case EnemyType.Sniper:
                    // Try to maintain distance
                    int distanceToPlayer = Math.Abs(player.X - X) + Math.Abs(player.Y - Y);
                    if (distanceToPlayer < 10)
                    {
                        MoveAwayFromPlayer();
                    }
                    else if (distanceToPlayer > 15)
                    {
                        MoveTowardsPlayer();
                    }
                    else if (random.Next(3) == 0) // Sometimes move to different position
                    {
                        MoveSideways();
                    }
                    break;
                case EnemyType.Rusher:
                    // Aggressive movement directly toward player
                    MoveTowardsPlayer();
                    // Try to move twice per turn
                    if (random.Next(2) == 0)
                    {
                        MoveTowardsPlayer();
                    }
                    break;
            }
        }

        /// <summary>
        /// Moves the enemy toward the player
        /// </summary>
        private void MoveTowardsPlayer()
        {
            // Simple pathfinding: move in the direction of the player
            int dx = 0, dy = 0;

            if (player.X > X) dx = 1;
            else if (player.X < X) dx = -1;

            if (player.Y > Y) dy = 1;
            else if (player.Y < Y) dy = -1;

            // Try to move in the primary direction first
            if (dx != 0 && !world.IsCollision(X + dx, Y))
            {
                X += dx;
            }
            else if (dy != 0 && !world.IsCollision(X, Y + dy))
            {
                Y += dy;
            }
            // If blocked, try the other direction
            else if (dy != 0 && !world.IsCollision(X, Y + dy))
            {
                Y += dy;
            }
            else if (dx != 0 && !world.IsCollision(X + dx, Y))
            {
                X += dx;
            }
            // If still blocked, try a random direction
            else
            {
                int randomDir = random.Next(4);
                switch (randomDir)
                {
                    case 0: if (!world.IsCollision(X, Y - 1)) Y--; break;
                    case 1: if (!world.IsCollision(X + 1, Y)) X++; break;
                    case 2: if (!world.IsCollision(X, Y + 1)) Y++; break;
                    case 3: if (!world.IsCollision(X - 1, Y)) X--; break;
                }
            }
        }

        /// <summary>
        /// Moves the enemy away from the player
        /// </summary>
        private void MoveAwayFromPlayer()
        {
            // Move away from the player
            int dx = 0, dy = 0;

            if (player.X > X) dx = -1;
            else if (player.X < X) dx = 1;

            if (player.Y > Y) dy = -1;
            else if (player.Y < Y) dy = 1;

            // Try to move away
            if (dx != 0 && !world.IsCollision(X + dx, Y))
            {
                X += dx;
            }
            else if (dy != 0 && !world.IsCollision(X, Y + dy))
            {
                Y += dy;
            }
            else
            {
                // If blocked, try any other valid direction
                int randomDir = random.Next(4);
                switch (randomDir)
                {
                    case 0: if (!world.IsCollision(X, Y - 1)) Y--; break;
                    case 1: if (!world.IsCollision(X + 1, Y)) X++; break;
                    case 2: if (!world.IsCollision(X, Y + 1)) Y++; break;
                    case 3: if (!world.IsCollision(X - 1, Y)) X--; break;
                }
            }
        }

        /// <summary>
        /// Moves the enemy perpendicular to the player
        /// </summary>
        private void MoveSideways()
        {
            // Move perpendicular to the player
            int dx = player.Y - Y; // Perpendicular direction
            int dy = -(player.X - X);

            // Normalize
            if (dx != 0) dx = dx / Math.Abs(dx);
            if (dy != 0) dy = dy / Math.Abs(dy);

            // Try to move sideways
            if (!world.IsCollision(X + dx, Y + dy))
            {
                X += dx;
                Y += dy;
            }
            else if (!world.IsCollision(X - dx, Y - dy)) // Try other side
            {
                X -= dx;
                Y -= dy;
            }
        }

        /// <summary>
        /// Makes the enemy shoot at the player
        /// </summary>
        private void ShootAtPlayer()
        {
            // Calculate direction to player
            int dx = 0, dy = 0;

            if (player.X > X) dx = 1;
            else if (player.X < X) dx = -1;

            if (player.Y > Y) dy = 1;
            else if (player.Y < Y) dy = -1;

            int damage = type == EnemyType.Sniper ? 35 :
                         type == EnemyType.HeavyScav ? 25 :
                         type == EnemyType.Rusher ? 15 : 20;

            int range = type == EnemyType.Sniper ? 25 :
                       type == EnemyType.Rusher ? 8 : 15;

            Projectile projectile = objectManager.GetProjectile();
            projectile.Initialize(X, Y, dx, dy, damage, range, false);

            // Create muzzle flash effect
            VisualEffect muzzleFlash = objectManager.GetEffect();
            muzzleFlash.Initialize(X, Y, '*', 2);
        }

        /// <summary>
        /// Checks if the player is within attack range
        /// </summary>
        /// <param name="range">Attack range</param>
        /// <returns>True if player is in range</returns>
        private bool IsPlayerInRange(int range)
        {
            int distance = Math.Abs(player.X - X) + Math.Abs(player.Y - Y);
            return distance <= range;
        }

        /// <summary>
        /// Renders the enemy on the world grid
        /// </summary>
        /// <param name="world">World to render on</param>
        public void Render(World world)
        {
            if (IsAlive)
            {
                char symbol = type == EnemyType.Scav ? 'e' :
                             type == EnemyType.HeavyScav ? 'E' :
                             type == EnemyType.Sniper ? 's' : 'r';

                world.SetTile(X, Y, symbol);
            }
        }

        /// <summary>
        /// Makes the enemy take damage
        /// </summary>
        /// <param name="damage">Amount of damage</param>
        public void TakeDamage(int damage)
        {
            Health -= damage;
        }

        /// <summary>
        /// Drops loot when the enemy is killed
        /// </summary>
        private void DropLoot()
        {
            LootContainer lootDrop = objectManager.GetLootContainer();
            lootDrop.Initialize(X, Y, $"{name} Loot");

            // More valuable loot for harder enemies
            int baseLootChance = type == EnemyType.Scav ? 30 :
                                type == EnemyType.HeavyScav ? 60 :
                                type == EnemyType.Sniper ? 75 : 90;

            // Generate random loot
            Random random = new Random();

            // Always drop some ammo
            lootDrop.AddItem(new Ammo("9mm Ammo", "Pistol", random.Next(5, 10)));

            // Chance for medkit
            if (random.Next(100) < baseLootChance)
            {
                lootDrop.AddItem(new MedKit("Small Medkit", 30));
            }

            // Chance for valuable
            if (random.Next(100) < baseLootChance / 2)
            {
                int value = (500 + random.Next(100, 500)) *
                           (type == EnemyType.Scav ? 1 :
                            type == EnemyType.HeavyScav ? 2 :
                            type == EnemyType.Sniper ? 3 : 4);

                lootDrop.AddItem(new Valuable("Cash", value));
            }

            // Small chance for weapon drop from tougher enemies
            if (type != EnemyType.Scav && random.Next(100) < 15)
            {
                if (type == EnemyType.Sniper)
                {
                    lootDrop.AddItem(new Weapon("Sniper Rifle", 50, 5, 25, 10));
                    lootDrop.AddItem(new Ammo("Sniper Ammo", "Sniper Rifle", random.Next(5, 10)));
                }
                else if (type == EnemyType.HeavyScav)
                {
                    lootDrop.AddItem(new Weapon("Shotgun", 40, 6, 10, 8));
                    lootDrop.AddItem(new Ammo("Shotgun Shells", "Shotgun", random.Next(10, 15)));
                }
                else if (type == EnemyType.Rusher)
                {
                    lootDrop.AddItem(new Weapon("SMG", 8, 30, 15, 1));
                    lootDrop.AddItem(new Ammo("SMG Ammo", "SMG", random.Next(20, 40)));
                }
            }

            // Chance for armor
            if (random.Next(100) < baseLootChance / 3)
            {
                int protection = type == EnemyType.Scav ? 10 :
                                type == EnemyType.HeavyScav ? 20 :
                                type == EnemyType.Sniper ? 15 : 12;

                lootDrop.AddItem(new Armor(type + " Armor", protection));
            }
        }
    }
}