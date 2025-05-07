using System;
using System.Numerics;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Represents an interactive feature on the map like trees, buildings, or special locations
    /// </summary>
    public class MapFeature
    {
        /// <summary>
        /// X coordinate of the feature
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate of the feature
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Symbol used to render the feature
        /// </summary>
        public char Symbol { get; private set; }

        /// <summary>
        /// Name of the feature
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of the feature
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Whether the feature blocks movement
        /// </summary>
        public bool HasCollision { get; private set; }

        /// <summary>
        /// Whether the feature contains loot
        /// </summary>
        public bool ContainsLoot { get; private set; }

        /// <summary>
        /// Whether the feature is a medical station
        /// </summary>
        public bool IsMedStation { get; private set; }

        /// <summary>
        /// Whether the feature is an ammo cache
        /// </summary>
        public bool IsAmmoCache { get; private set; }

        /// <summary>
        /// Whether the feature is a water source
        /// </summary>
        public bool IsWaterSource { get; private set; }

        /// <summary>
        /// Constructor for MapFeature
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="symbol">Display symbol</param>
        /// <param name="name">Feature name</param>
        /// <param name="description">Feature description</param>
        /// <param name="hasCollision">Whether it blocks movement</param>
        /// <param name="containsLoot">Whether it contains loot</param>
        /// <param name="isMedStation">Whether it's a medical station</param>
        /// <param name="isAmmoCache">Whether it's an ammo cache</param>
        /// <param name="waterPond">Whether it's a water source</param>
        public MapFeature(int x, int y, char symbol, string name, string description, bool hasCollision,
            bool containsLoot = false, bool isMedStation = false, bool isAmmoCache = false, bool waterPond = false)
        {
            X = x;
            Y = y;
            Symbol = symbol;
            Name = name;
            Description = description;
            HasCollision = hasCollision;
            ContainsLoot = containsLoot;
            IsMedStation = isMedStation;
            IsAmmoCache = isAmmoCache;
            IsWaterSource = waterPond;
        }

        /// <summary>
        /// Handles player interaction with the feature
        /// </summary>
        /// <param name="player">The player</param>
        /// <param name="messageLog">Message log for feedback</param>
        public void Interact(Player player, MessageLog messageLog)
        {
            if (IsMedStation)
            {
                HandleMedStationInteraction(player, messageLog);
            }
            else if (IsAmmoCache)
            {
                HandleAmmoCacheInteraction(player, messageLog);
            }
            else if (IsWaterSource)
            {
                HandleWaterSourceInteraction(player, messageLog);
            }
            else if (ContainsLoot)
            {
                HandleLootInteraction(player, messageLog);
            }
            else
            {
                // Just a description
                messageLog.AddMessage(Description);
            }
        }

        /// <summary>
        /// Handles interaction with a medical station
        /// </summary>
        private void HandleMedStationInteraction(Player player, MessageLog messageLog)
        {
            if (player.Health < player.MaxHealth)
            {
                int healAmount = 20;
                player.Heal(healAmount);
                messageLog.AddMessage($"You used the medical station. +{healAmount} HP");
            }
            else
            {
                messageLog.AddMessage("You're already at full health.");
            }
        }

        /// <summary>
        /// Handles interaction with an ammo cache
        /// </summary>
        private void HandleAmmoCacheInteraction(Player player, MessageLog messageLog)
        {
            if (player.EquippedWeapon != null)
            {
                // Create appropriate ammo type
                string ammoName = player.EquippedWeapon.Name + " Ammo";
                Ammo ammo = new Ammo(ammoName, player.EquippedWeapon.Name, 20);

                if (player.AddToInventory(ammo))
                {
                    messageLog.AddMessage($"Found ammunition: {ammo.Name} x{ammo.Count}");
                }
                else
                {
                    messageLog.AddMessage("Inventory full! Cannot take ammunition.");
                }
            }
            else
            {
                messageLog.AddMessage("You don't have a weapon equipped to take ammo for.");
            }
        }

        /// <summary>
        /// Handles interaction with a water source
        /// </summary>
        private void HandleWaterSourceInteraction(Player player, MessageLog messageLog)
        {
            messageLog.AddMessage("You drink from the water source. Refreshing!");

            // Add hydration mechanic if implementing survival mechanics
        }

        /// <summary>
        /// Handles interaction with a lootable object
        /// </summary>
        private void HandleLootInteraction(Player player, MessageLog messageLog)
        {
            // Generate random loot
            Random random = new Random();
            int lootType = random.Next(5);

            Item loot = null;

            switch (lootType)
            {
                case 0:
                    loot = new MedKit("Bandage", 15);
                    break;
                case 1:
                    loot = new Ammo("9mm Ammo", "Pistol", random.Next(5, 15));
                    break;
                case 2:
                    loot = new Valuable("Cash", random.Next(100, 500));
                    break;
                case 3:
                    loot = new Armor("Light Armor", 15);
                    break;
                case 4:
                    // Chance for rare weapon
                    if (random.Next(10) == 0)
                    {
                        loot = new Weapon("SMG", 10, 30, 15, 1);
                    }
                    else
                    {
                        loot = new Ammo("Rifle Ammo", "Rifle", random.Next(5, 10));
                    }
                    break;
            }

            if (loot != null)
            {
                if (player.AddToInventory(loot))
                {
                    messageLog.AddMessage($"Found: {loot.GetDescription()}");

                    // Remove loot from feature
                    ContainsLoot = false;
                }
                else
                {
                    messageLog.AddMessage("Inventory full! Cannot take item.");
                }
            }
        }
    }
}