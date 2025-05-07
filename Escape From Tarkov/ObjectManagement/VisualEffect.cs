using ConsoleEscapeFromTarkov.GameCore;

namespace ConsoleEscapeFromTarkov.ObjectManagement
{
    /// <summary>
    /// Visual effect for impacts, flashes, etc.
    /// </summary>
    public class VisualEffect : IPoolable
    {
        /// <summary>
        /// X coordinate of the effect
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate of the effect
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Symbol used to render the effect
        /// </summary>
        public char Symbol { get; private set; }

        /// <summary>
        /// Duration of the effect in frames
        /// </summary>
        public int Duration { get; private set; }

        /// <summary>
        /// Current frame of the effect
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// Whether the effect is active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Constructor for VisualEffect
        /// </summary>
        public VisualEffect()
        {
            Reset();
        }

        /// <summary>
        /// Initializes the effect
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="symbol">Display symbol</param>
        /// <param name="duration">Duration in frames</param>
        public void Initialize(int x, int y, char symbol, int duration)
        {
            X = x;
            Y = y;
            Symbol = symbol;
            Duration = duration;
            CurrentFrame = 0;
            IsActive = true;
        }

        /// <summary>
        /// Updates the effect animation
        /// </summary>
        public void Update()
        {
            if (!IsActive) return;

            CurrentFrame++;

            if (CurrentFrame >= Duration)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Renders the effect on the world grid
        /// </summary>
        /// <param name="world">World to render in</param>
        public void Render(World world)
        {
            if (!IsActive) return;

            world.SetTile(X, Y, Symbol);
        }

        /// <summary>
        /// Activates the effect
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the effect
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Resets the effect to initial state
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;
            Symbol = ' ';
            Duration = 0;
            CurrentFrame = 0;
            IsActive = false;
        }
    }
}