using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.Items;
using ConsoleEscapeFromTarkov.ObjectManagement;

namespace ConsoleEscapeFromTarkov.Entities
{
    /// <summary>
    /// Player character class with inventory, combat, and progression
    /// </summary>
    public class Player
    {
        private World world;
        private LootManager lootManager;
        private ObjectManager objectManager;
        private MessageLog messageLog;
        private List<Item> inventory;
        private List<Item> equipment;
        private Item[] quickSlots;
        private List<Weapon> weapons;
        private int selectedWeaponIndex;
        private int lastShotFrame;
        private int currentFrame;

        // Player stats and progression
        public int Level { get; private set; }
        public int Experience { get; private set; }
        public int KillCount { get; private set; }

        // Player state
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int MaxInventorySize { get; private set; }

        // Public accessors
        public List<Item> Inventory => inventory;
        public List<Item> Equipment => equipment;
        public List<Weapon> Weapons => weapons;
        public Weapon EquippedWeapon => (weapons.Count > 0 && selectedWeaponIndex >= 0) ? weapons[selectedWeaponIndex] : null;
        public Item[] QuickSlots => quickSlots;

        /// <summary>
        /// Constructor for the Player class
        /// </summary>
        public Player(int x, int y, World world, LootManager lootManager, ObjectManager objectManager, MessageLog messageLog)
        {
            this.world = world;
            this.lootManager = lootManager;
            this.objectManager = objectManager;
            this.messageLog = messageLog;
            X = x;
            Y = y;
            MaxHealth = 100;
            Health = MaxHealth;
            MaxInventorySize = 20;
            lastShotFrame = 0;
            currentFrame = 0;

            // Initialize collections
            inventory = new List<Item>();
            equipment = new List<Item>();
            weapons = new List<Weapon>();
            quickSlots = new Item[5]; // 5 quick slots
            selectedWeaponIndex = -1;

            // Initialize stats
            Level = 1;
            Experience = 0;
            KillCount = 0;

            // Give player starting equipment
            InitializeStartingEquipment();
        }

        /// <summary>
        /// Gives the player initial equipment
        /// </summary>
        private void InitializeStartingEquipment()
        {
            Weapon pistol = new Weapon("Pistol", 15, 8, 15, 3);
            Ammo pistolAmmo = new Ammo("9mm Ammo", "Pistol", 24);
            MedKit smallMed = new MedKit("Small Medkit", 30);

            inventory.Add(pistol);
            inventory.Add(pistolAmmo);
            inventory.Add(smallMed);

            // Put medkit in quickslot 0
            AssignToQuickSlot(2, 0, 0);

            // Equip weapon
            EquipWeapon(pistol);
        }

        /// <summary>
        /// Resets the player for a new game/raid
        /// </summary>
        /// <param name="x">Starting X position</param>
        /// <param name="y">Starting Y position</param>
        public void Reset(int x, int y)
        {
            X = x;
            Y = y;
            Health = MaxHealth;
            lastShotFrame = 0;
            currentFrame = 0;

            // Reset collections but keep level/XP
            inventory.Clear();
            equipment.Clear();
            weapons.Clear();
            for (int i = 0; i < quickSlots.Length; i++)
            {
                quickSlots[i] = null;
            }
            selectedWeaponIndex = -1;
            KillCount = 0;

            // Give player starting equipment again
            InitializeStartingEquipment();
        }

        /// <summary>
        /// Moves the player in the specified direction
        /// </summary>
        /// <param name="dx">X direction</param>
        /// <param name="dy">Y direction</param>
        public void Move(int dx, int dy)
        {
            int newX = X + dx;
            int newY = Y + dy;

            if (!world.IsCollision(newX, newY))
            {
                X = newX;
                Y = newY;
            }
        }

        /// <summary>
        /// Fires the player's equipped weapon
        /// </summary>
        public void Shoot()
        {
            currentFrame++;

            if (EquippedWeapon != null && EquippedWeapon.CurrentAmmo > 0)
            {
                // Respect fire rate
                if (currentFrame - lastShotFrame < EquippedWeapon.FireRate)
                {
                    return;
                }

                lastShotFrame = currentFrame;

                // Create projectiles based on weapon spread
                int spread = EquippedWeapon.Spread;

                if (spread == 0)
                {
                    // Single shot weapons - shoot in all 4 directions
                    ShootInDirections(new (int, int)[] { (0, -1), (1, 0), (0, 1), (-1, 0) });
                }
                else if (spread == 1)
                {
                    // Spread weapons like shotguns - shoot in 8 directions
                    ShootInDirections(new (int, int)[] {
                        (0, -1), (1, -1), (1, 0), (1, 1),
                        (0, 1), (-1, 1), (-1, 0), (-1, -1)
                    });
                }
                else
                {
                    // Only shoot in cardinal directions but with reduced ammo use
                    ShootInDirections(new (int, int)[] { (0, -1), (1, 0), (0, 1), (-1, 0) });

                    // Extra ammo isn't consumed for high spread weapons
                    EquippedWeapon.CurrentAmmo++; // Will be decremented below, so net 0
                }

                EquippedWeapon.CurrentAmmo--;

                // Create muzzle flash effect
                VisualEffect muzzleFlash = objectManager.GetEffect();
                muzzleFlash.Initialize(X, Y, '*', 2);

                messageLog.AddMessage($"Fired {EquippedWeapon.Name}. Ammo: {EquippedWeapon.CurrentAmmo}/{EquippedWeapon.MagazineSize}");
            }
            else if (EquippedWeapon != null)
            {
                messageLog.AddMessage("Click! Out of ammo. Press R to reload.");
            }
            else
            {
                messageLog.AddMessage("No weapon equipped!");
            }
        }

        /// <summary>
        /// Shoots projectiles in the specified directions
        /// </summary>
        /// <param name="directions">Array of direction tuples (dx, dy)</param>
        private void ShootInDirections((int, int)[] directions)
        {
            foreach (var (dx, dy) in directions)
            {
                Projectile projectile = objectManager.GetProjectile();
                projectile.Initialize(X, Y, dx, dy, EquippedWeapon.Damage, EquippedWeapon.Range, true);
            }
        }

        /// <summary>
        /// Reloads the currently equipped weapon
        /// </summary>
        public void Reload()
        {
            if (EquippedWeapon != null && EquippedWeapon.CurrentAmmo < EquippedWeapon.MagazineSize)
            {
                // Find compatible ammo
                foreach (Item item in inventory.ToList())
                {
                    if (item is Ammo ammo && ammo.WeaponType == EquippedWeapon.Name)
                    {
                        int ammoNeeded = EquippedWeapon.MagazineSize - EquippedWeapon.CurrentAmmo;
                        int ammoToUse = Math.Min(ammoNeeded, ammo.Count);

                        EquippedWeapon.CurrentAmmo += ammoToUse;
                        ammo.Count -= ammoToUse;

                        messageLog.AddMessage($"Reloaded {EquippedWeapon.Name}. Ammo: {EquippedWeapon.CurrentAmmo}/{EquippedWeapon.MagazineSize}");

                        // Remove ammo item if empty
                        if (ammo.Count <= 0)
                        {
                            inventory.Remove(ammo);
                        }

                        break;
                    }
                }
            }
            else if (EquippedWeapon != null)
            {
                messageLog.AddMessage($"{EquippedWeapon.Name} is already fully loaded.");
            }
            else
            {
                messageLog.AddMessage("No weapon equipped!");
            }
        }

        /// <summary>
        /// Cycles through equipped weapons
        /// </summary>
        public void CycleWeapons()
        {
            if (weapons.Count <= 1)
            {
                messageLog.AddMessage("No other weapons available.");
                return;
            }

            selectedWeaponIndex = (selectedWeaponIndex + 1) % weapons.Count;
            messageLog.AddMessage($"Switched to {weapons[selectedWeaponIndex].Name}");
        }

        /// <summary>
        /// Uses an item from the quick slots
        /// </summary>
        /// <param name="slotIndex">Index of the quick slot</param>
        public void QuickUseItem(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < quickSlots.Length && quickSlots[slotIndex] != null)
            {
                Item item = quickSlots[slotIndex];

                if (item is MedKit medKit)
                {
                    if (Health < MaxHealth)
                    {
                        Heal(medKit.HealAmount);
                        messageLog.AddMessage($"Used {medKit.Name}. +{medKit.HealAmount} HP");

                        // Remove from inventory and quickslot
                        inventory.Remove(medKit);
                        quickSlots[slotIndex] = null;
                    }
                    else
                    {
                        messageLog.AddMessage("You're already at full health.");
                    }
                }
                else if (item is Ammo ammo && EquippedWeapon != null)
                {
                    if (ammo.WeaponType == EquippedWeapon.Name)
                    {
                        Reload();
                    }
                    else
                    {
                        messageLog.AddMessage($"{ammo.Name} is not compatible with your equipped weapon.");
                    }
                }
                else
                {
                    messageLog.AddMessage($"Can't quick-use {item.Name}.");
                }
            }
            else
            {
                messageLog.AddMessage($"No item in quick slot {slotIndex + 1}.");
            }
        }

        /// <summary>
        /// Assigns an item to a quick slot
        /// </summary>
        /// <param name="inventoryIndex">Index of the item in inventory</param>
        /// <param name="tabIndex">Tab index (unused)</param>
        /// <param name="slotIndex">Quick slot index to assign to</param>
        public void AssignToQuickSlot(int inventoryIndex, int tabIndex, int slotIndex)
        {
            if (inventoryIndex >= 0 && inventoryIndex < inventory.Count &&
                slotIndex >= 0 && slotIndex < quickSlots.Length)
            {
                quickSlots[slotIndex] = inventory[inventoryIndex];
                messageLog.AddMessage($"Assigned {inventory[inventoryIndex].Name} to quick slot {slotIndex + 1}");
            }
        }

        /// <summary>
        /// Updates the player state for the current frame
        /// </summary>
        public void Update()
        {
            // Check if player projectiles hit enemies or walls
            foreach (Projectile projectile in objectManager.GetActiveProjectiles())
            {
                if (projectile.IsPlayerProjectile &&
                    (world.IsCollision(projectile.X, projectile.Y) ||
                     projectile.DistanceTraveled >= projectile.Range))
                {
                    projectile.Deactivate();

                    // Add impact effect
                    VisualEffect impact = objectManager.GetEffect();
                    impact.Initialize(projectile.X, projectile.Y, '×', 2);
                }
            }

            // Check if enemy projectiles hit the player
            foreach (Projectile projectile in objectManager.GetActiveProjectiles())
            {
                if (!projectile.IsPlayerProjectile && projectile.X == X && projectile.Y == Y)
                {
                    // Calculate actual damage based on armor
                    int damage = CalculateDamageWithArmor(projectile.Damage);

                    TakeDamage(damage);
                    projectile.Deactivate();

                    // Add hit effect
                    VisualEffect bloodEffect = objectManager.GetEffect();
                    bloodEffect.Initialize(X, Y, '!', 3);

                    messageLog.AddMessage($"Hit! -{damage} HP");
                }
            }
        }

        /// <summary>
        /// Calculates damage reduction from armor
        /// </summary>
        /// <param name="rawDamage">Raw damage amount</param>
        /// <returns>Damage after armor reduction</returns>
        private int CalculateDamageWithArmor(int rawDamage)
        {
            // Check for equipped armor
            int totalArmorRating = 0;

            foreach (Item item in equipment)
            {
                if (item is Armor armor)
                {
                    totalArmorRating += armor.Protection;
                }
            }

            // Apply armor reduction
            if (totalArmorRating > 0)
            {
                // Simple formula: damage reduced by percentage of armor
                int reduction = (int)(rawDamage * (totalArmorRating / 100.0f));
                return Math.Max(1, rawDamage - reduction); // Always at least 1 damage
            }

            return rawDamage;
        }

        /// <summary>
        /// Renders the player on the world grid
        /// </summary>
        /// <param name="world">The world to render on</param>
        public void Render(World world)
        {
            world.SetTile(X, Y, '@');
        }

        /// <summary>
        /// Takes damage from attacks
        /// </summary>
        /// <param name="damage">Amount of damage to take</param>
        public void TakeDamage(int damage)
        {
            Health -= damage;
            if (Health < 0) Health = 0;
        }

        /// <summary>
        /// Heals the player
        /// </summary>
        /// <param name="amount">Amount of health to heal</param>
        public void Heal(int amount)
        {
            Health += amount;
            if (Health > MaxHealth) Health = MaxHealth;
        }

        /// <summary>
        /// Adds an item to the player's inventory
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <returns>True if added successfully</returns>
        public bool AddToInventory(Item item)
        {
            if (inventory.Count < MaxInventorySize)
            {
                inventory.Add(item);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Uses an item from the inventory
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <param name="tabIndex">Tab index (unused)</param>
        public void UseSelectedItem(int index, int tabIndex)
        {
            if (index >= 0 && index < inventory.Count)
            {
                Item item = inventory[index];

                if (item is MedKit medKit)
                {
                    Heal(medKit.HealAmount);
                    messageLog.AddMessage($"Used {medKit.Name}. +{medKit.HealAmount} HP");
                    inventory.RemoveAt(index);

                    // Also remove from quickslots if it's there
                    for (int i = 0; i < quickSlots.Length; i++)
                    {
                        if (quickSlots[i] == medKit)
                        {
                            quickSlots[i] = null;
                        }
                    }
                }
                else if (item is Ammo ammo && EquippedWeapon != null)
                {
                    if (ammo.WeaponType == EquippedWeapon.Name)
                    {
                        Reload();
                    }
                    else
                    {
                        messageLog.AddMessage($"{ammo.Name} is not compatible with your equipped weapon.");
                    }
                }
                else
                {
                    messageLog.AddMessage($"Can't use {item.Name}.");
                }
            }
        }

        /// <summary>
        /// Equips an item from the inventory
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <param name="tabIndex">Tab index (unused)</param>
        public void EquipSelectedItem(int index, int tabIndex)
        {
            if (index >= 0 && index < inventory.Count)
            {
                Item item = inventory[index];

                if (item is Weapon weapon)
                {
                    EquipWeapon(weapon);
                }
                else if (item is Armor armor)
                {
                    EquipArmor(armor);
                }
                else
                {
                    messageLog.AddMessage($"Can't equip {item.Name}.");
                }
            }
        }

        /// <summary>
        /// Equips a weapon
        /// </summary>
        /// <param name="weapon">Weapon to equip</param>
        public void EquipWeapon(Weapon weapon)
        {
            inventory.Remove(weapon);
            weapons.Add(weapon);
            selectedWeaponIndex = weapons.Count - 1;
            messageLog.AddMessage($"Equipped {weapon.Name}");
        }

        /// <summary>
        /// Equips armor
        /// </summary>
        /// <param name="armor">Armor to equip</param>
        public void EquipArmor(Armor armor)
        {
            inventory.Remove(armor);
            equipment.Add(armor);
            messageLog.AddMessage($"Equipped {armor.Name}");
        }

        /// <summary>
        /// Drops an item from inventory
        /// </summary>
        /// <param name="index">Index of the item</param>
        /// <param name="tabIndex">Tab index (unused)</param>
        public void DropSelectedItem(int index, int tabIndex)
        {
            if (index >= 0 && index < inventory.Count)
            {
                Item item = inventory[index];
                inventory.RemoveAt(index);

                // Remove from quickslots if it's there
                for (int i = 0; i < quickSlots.Length; i++)
                {
                    if (quickSlots[i] == item)
                    {
                        quickSlots[i] = null;
                    }
                }

                // Create a loot container with the dropped item
                LootContainer droppedContainer = objectManager.GetLootContainer();
                droppedContainer.Initialize(X, Y, "Dropped Item");
                droppedContainer.AddItem(item);

                messageLog.AddMessage($"Dropped {item.Name}");
            }
        }

        /// <summary>
        /// Gets a loot container near the player
        /// </summary>
        /// <returns>Nearby loot container or null</returns>
        public LootContainer GetNearbyLootContainer()
        {
            foreach (LootContainer container in objectManager.GetActiveLootContainers())
            {
                if ((container.X == X && container.Y == Y) ||
                    (container.X == X + 1 && container.Y == Y) ||
                    (container.X == X - 1 && container.Y == Y) ||
                    (container.X == X && container.Y == Y + 1) ||
                    (container.X == X && container.Y == Y - 1))
                {
                    return container;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all map features near the player
        /// </summary>
        /// <returns>List of nearby map features</returns>
        public List<MapFeature> GetNearbyMapFeatures()
        {
            List<MapFeature> features = new List<MapFeature>();

            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue; // Skip player position

                    MapFeature feature = world.GetFeatureAt(X + dx, Y + dy);
                    if (feature != null)
                    {
                        features.Add(feature);
                    }
                }
            }

            // Also check player's current position
            MapFeature currentFeature = world.GetFeatureAt(X, Y);
            if (currentFeature != null)
            {
                features.Add(currentFeature);
            }

            return features;
        }

        /// <summary>
        /// Takes an item from a loot container
        /// </summary>
        /// <param name="container">The container to loot from</param>
        /// <param name="item">The item to take</param>
        /// <returns>True if item was taken</returns>
        public bool TakeLootItem(LootContainer container, Item item)
        {
            if (AddToInventory(item))
            {
                container.RemoveItem(item);

                // If container is empty, deactivate it
                if (container.Items.Count == 0)
                {
                    container.Deactivate();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds experience points and checks for level up
        /// </summary>
        /// <param name="amount">Amount of XP to add</param>
        public void AddExperience(int amount)
        {
            Experience += amount;

            // Check for level up - simple formula: 1000 XP per level
            int newLevel = 1 + (Experience / 1000);

            if (newLevel > Level)
            {
                int levelsGained = newLevel - Level;
                Level = newLevel;

                // Increase max health for each level gained
                MaxHealth += levelsGained * 10;
                Health += levelsGained * 10; // Also heal when leveling up
                MaxInventorySize += levelsGained; // More inventory space

                messageLog.AddMessage($"Level Up! You are now level {Level}");
                messageLog.AddMessage($"+{levelsGained * 10} Max HP, +{levelsGained} Inventory Space");
            }
        }

        /// <summary>
        /// Increments the player's kill count
        /// </summary>
        public void IncrementKillCount()
        {
            KillCount++;
        }

        /// <summary>
        /// Gets the total value of all valuable items in inventory
        /// </summary>
        /// <returns>Total value</returns>
        public int GetTotalItemsValue()
        {
            int totalValue = 0;

            foreach (Item item in inventory)
            {
                if (item is Valuable valuable)
                {
                    totalValue += valuable.Value;
                }
            }

            return totalValue;
        }

        /// <summary>
        /// Checks if the player has an item with the specified name
        /// </summary>
        /// <param name="itemName">Name of the item</param>
        /// <returns>True if the player has the item</returns>
        public bool HasItem(string itemName)
        {
            return inventory.Any(item => item.Name == itemName);
        }

        /// <summary>
        /// Saves player progress for future raids
        /// </summary>
        public void SaveProgress()
        {
            // This would save the player's current progress to be used in future raids
            // For now, just keep the stats in memory
        }

        /// <summary>
        /// Removes mission items from inventory
        /// </summary>
        public void ResetMissionItems()
        {
            // Remove mission items but keep regular gear
            inventory.RemoveAll(item =>
                item is Valuable valuable && valuable.IsMissionItem);
        }
    }
}