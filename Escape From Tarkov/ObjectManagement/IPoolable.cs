using ConsoleEscapeFromTarkov.GameCore;

namespace ConsoleEscapeFromTarkov.ObjectManagement
{
    /// <summary>
    /// Interface for objects that can be pooled and reused
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// Whether the object is currently active
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Activates the object
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates the object
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Updates the object
        /// </summary>
        void Update();

        /// <summary>
        /// Renders the object
        /// </summary>
        /// <param name="world">World to render in</param>
        void Render(World world);

        /// <summary>
        /// Resets the object to initial state
        /// </summary>
        void Reset();
    }
}