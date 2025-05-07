using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages missions and their objectives for the player
    /// </summary>
    public class MissionManager
    {
        private Player player;
        private EnemyManager enemyManager;
        private LootManager lootManager;
        private List<MissionObjective> objectives;
        private Random random;

        /// <summary>
        /// Constructor for MissionManager
        /// </summary>
        /// <param name="player">Player reference</param>
        /// <param name="enemyManager">Enemy manager reference</param>
        /// <param name="lootManager">Loot manager reference</param>
        public MissionManager(Player player, EnemyManager enemyManager, LootManager lootManager)
        {
            this.player = player;
            this.enemyManager = enemyManager;
            this.lootManager = lootManager;
            objectives = new List<MissionObjective>();
            random = new Random();
        }

        /// <summary>
        /// Sets up missions for a new raid
        /// </summary>
        public void SetupMissions()
        {
            objectives.Clear();

            // Based on player level, set harder or easier missions
            int playerLevel = player.Level;
            int objectiveCount = Math.Min(1 + playerLevel / 2, 5); // More objectives as player levels up

            for (int i = 0; i < objectiveCount; i++)
            {
                MissionObjective objective = GenerateRandomObjective(playerLevel);
                objectives.Add(objective);
            }
        }

        /// <summary>
        /// Generates a random mission objective
        /// </summary>
        /// <param name="playerLevel">Player's current level</param>
        /// <returns>A new mission objective</returns>
        private MissionObjective GenerateRandomObjective(int playerLevel)
        {
            // Various objective types
            int type = random.Next(5);
            MissionObjective objective;

            switch (type)
            {
                case 0:
                    // Kill enemies
                    int enemyCount = playerLevel + random.Next(3, 7);
                    objective = new MissionObjective
                    {
                        Type = MissionObjectiveType.KillEnemies,
                        Description = $"Kill {enemyCount} enemies",
                        TargetCount = enemyCount,
                        CurrentCount = 0,
                        CompletionXP = enemyCount * 50
                    };
                    break;
                case 1:
                    // Collect valuables
                    int valueAmount = (playerLevel + 1) * 500 + random.Next(500);
                    objective = new MissionObjective
                    {
                        Type = MissionObjectiveType.CollectValue,
                        Description = $"Collect items worth {valueAmount}₽",
                        TargetCount = valueAmount,
                        CurrentCount = 0,
                        CompletionXP = valueAmount / 10
                    };
                    break;
                case 2:
                    // Find specific item
                    string[] itemNames = { "Gold Watch", "Secret Documents", "Encrypted Drive", "Rare Weapon", "Medical Supplies" };
                    string itemName = itemNames[random.Next(itemNames.Length)];
                    objective = new MissionObjective
                    {
                        Type = MissionObjectiveType.FindItem,
                        Description = $"Find {itemName}",
                        ItemName = itemName,
                        CompletionXP = 300 + playerLevel * 50
                    };

                    // Create the special mission item
                    Valuable missionItem = new Valuable(itemName, 1000 + playerLevel * 200, true);
                    lootManager.AddMissionItem(missionItem);
                    break;
                case 3:
                    // Visit location
                    string[] locationTypes = { "Warehouse", "Medical Station", "Ammo Cache", "Water Source", "Military Base" };
                    string locationType = locationTypes[random.Next(locationTypes.Length)];
                    objective = new MissionObjective
                    {
                        Type = MissionObjectiveType.VisitLocation,
                        Description = $"Visit {locationType}",
                        LocationType = locationType,
                        CompletionXP = 200 + playerLevel * 30
                    };
                    break;
                default:
                    // Survive time
                    int timeToSurvive = (playerLevel + 2) * 300; // Time in game ticks
                    objective = new MissionObjective
                    {
                        Type = MissionObjectiveType.SurviveTime,
                        Description = $"Survive for {timeToSurvive / 60} minutes",
                        TargetCount = timeToSurvive,
                        CurrentCount = 0,
                        CompletionXP = timeToSurvive / 3
                    };
                    break;
            }

            return objective;
        }

        /// <summary>
        /// Updates all mission objectives based on player progress
        /// </summary>
        public void Update()
        {
            foreach (MissionObjective objective in objectives)
            {
                UpdateObjective(objective);
            }
        }

        /// <summary>
        /// Updates a single mission objective
        /// </summary>
        /// <param name="objective">Objective to update</param>
        private void UpdateObjective(MissionObjective objective)
        {
            if (objective.IsCompleted)
                return;

            switch (objective.Type)
            {
                case MissionObjectiveType.KillEnemies:
                    objective.CurrentCount = player.KillCount;
                    break;
                case MissionObjectiveType.CollectValue:
                    objective.CurrentCount = player.GetTotalItemsValue();
                    break;
                case MissionObjectiveType.FindItem:
                    objective.IsCompleted = player.HasItem(objective.ItemName);
                    break;
                case MissionObjectiveType.VisitLocation:
                    // Check if player is at the correct location type
                    var mapFeatures = player.GetNearbyMapFeatures();
                    foreach (MapFeature feature in mapFeatures)
                    {
                        if (feature.Name.Contains(objective.LocationType))
                        {
                            objective.IsCompleted = true;
                            break;
                        }
                    }
                    break;
                case MissionObjectiveType.SurviveTime:
                    objective.CurrentCount++;
                    break;
            }

            // Check if objective is completed
            if (!objective.IsCompleted)
            {
                if (objective.Type == MissionObjectiveType.KillEnemies ||
                    objective.Type == MissionObjectiveType.CollectValue ||
                    objective.Type == MissionObjectiveType.SurviveTime)
                {
                    if (objective.CurrentCount >= objective.TargetCount)
                    {
                        objective.IsCompleted = true;
                        player.AddExperience(objective.CompletionXP);
                    }
                }
            }
            // Award XP for first-time completion
            else if (objective.IsCompleted && objective.CompletionXP > 0)
            {
                player.AddExperience(objective.CompletionXP);
                objective.CompletionXP = 0; // Don't award XP again
            }
        }

        /// <summary>
        /// Gets all mission objectives
        /// </summary>
        /// <returns>Enumerable of objectives</returns>
        public IEnumerable<MissionObjective> GetObjectives()
        {
            return objectives;
        }

        /// <summary>
        /// Gets the count of completed objectives
        /// </summary>
        /// <returns>Number of completed objectives</returns>
        public int GetCompletedObjectivesCount()
        {
            return objectives.Count(o => o.IsCompleted);
        }

        /// <summary>
        /// Renders mission objective markers on the map
        /// </summary>
        /// <param name="world">World to render on</param>
        public void RenderObjectives(World world)
        {
            // Highlight mission-specific locations if needed
            foreach (MissionObjective objective in objectives)
            {
                if (objective.Type == MissionObjectiveType.VisitLocation && !objective.IsCompleted)
                {
                    // Highlight relevant locations
                    foreach (MapFeature feature in world.GetMapFeatures())
                    {
                        if (feature.Name.Contains(objective.LocationType))
                        {
                            // Highlight this feature
                            world.SetTile(feature.X, feature.Y, '!');
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total XP available from all current objectives
        /// </summary>
        /// <returns>Total XP available</returns>
        public int GetTotalAvailableXP()
        {
            return objectives.Sum(o => o.CompletionXP);
        }
    }
}