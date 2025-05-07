namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Types of mission objectives
    /// </summary>
    public enum MissionObjectiveType
    {
        KillEnemies,
        CollectValue,
        FindItem,
        VisitLocation,
        SurviveTime
    }

    /// <summary>
    /// Represents a single mission objective for the player to complete
    /// </summary>
    public class MissionObjective
    {
        /// <summary>
        /// Type of objective
        /// </summary>
        public MissionObjectiveType Type { get; set; }

        /// <summary>
        /// Human-readable description of the objective
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Target count for countable objectives (kills, value, time)
        /// </summary>
        public int TargetCount { get; set; }

        /// <summary>
        /// Current progress toward the target
        /// </summary>
        public int CurrentCount { get; set; }

        /// <summary>
        /// Name of the item to find (for FindItem objectives)
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// Type of location to visit (for VisitLocation objectives)
        /// </summary>
        public string LocationType { get; set; }

        /// <summary>
        /// Whether the objective has been completed
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Experience points awarded for completing this objective
        /// </summary>
        public int CompletionXP { get; set; }

        /// <summary>
        /// Gets the completion percentage for progress display
        /// </summary>
        /// <returns>Percentage complete (0-100)</returns>
        public int GetCompletionPercentage()
        {
            if (IsCompleted)
                return 100;

            if (Type == MissionObjectiveType.FindItem || Type == MissionObjectiveType.VisitLocation)
                return IsCompleted ? 100 : 0;

            if (TargetCount <= 0)
                return 0;

            int percentage = (int)((CurrentCount / (float)TargetCount) * 100);
            return System.Math.Min(percentage, 100);
        }

        /// <summary>
        /// Gets a status string showing progress
        /// </summary>
        /// <returns>Status string</returns>
        public string GetStatusString()
        {
            if (IsCompleted)
                return "[COMPLETED]";

            if (Type == MissionObjectiveType.FindItem || Type == MissionObjectiveType.VisitLocation)
                return "[PENDING]";

            return $"[{CurrentCount}/{TargetCount}]";
        }

        /// <summary>
        /// Gets a detailed description with progress
        /// </summary>
        /// <returns>Full objective description with status</returns>
        public string GetDetailedDescription()
        {
            return $"{Description} {GetStatusString()} - {CompletionXP} XP";
        }
    }
}