using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.ObjectManagement;

namespace ConsoleEscapeFromTarkov.Entities
{
    /// <summary>
    /// Represents a projectile (bullet) in the game
    /// </summary>
    public class Projectile : IPoolable
    {
        /// <summary>
        /// X coordinate of the projectile
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate of the projectile
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// X direction of movement
        /// </summary>
        public int DX { get; private set; }

        /// <summary>
        /// Y direction of movement
        /// </summary>
        public int DY { get; private set; }

        /// <summary>
        /// Damage dealt by the projectile
        /// </summary>
        public int Damage { get; private set; }

        /// <summary>
        /// Maximum range of the projectile
        /// </summary>
        public int Range { get; private set; }

        /// <summary>
        /// Distance the projectile has traveled
        /// </summary>
        public int DistanceTraveled { get; private set; }

        /// <summary>
        /// Whether the projectile was fired by the player
        /// </summary>
        public bool IsPlayerProjectile { get; private set; }

        /// <summary>
        /// Whether the projectile is active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Text representation of the direction
        /// </summary>
        public string Direction
        {
            get
            {
                if (DY < 0) return "up";
                if (DX > 0) return "right";
                if (DY > 0) return "down";
                if (DX < 0) return "left";
                return "";
            }
        }

        /// <summary>
        /// Constructor for Projectile
        /// </summary>
        public Projectile()
        {
            Reset();
        }

        /// <summary>
        /// Initializes the projectile
        /// </summary>
        /// <param name="x">Start X position</param>
        /// <param name="y">Start Y position</param>
        /// <param name="dx">X direction</param>
        /// <param name="dy">Y direction</param>
        /// <param name="damage">Damage amount</param>
        /// <param name="range">Maximum range</param>
        /// <param name="isPlayerProjectile">Whether fired by player</param>
        public void Initialize(int x, int y, int dx, int dy, int damage, int range, bool isPlayerProjectile)
        {
            X = x;
            Y = y;
            DX = dx;
            DY = dy;
            Damage = damage;
            Range = range;
            IsPlayerProjectile = isPlayerProjectile;
            DistanceTraveled = 0;
            IsActive = true;
        }

        /// <summary>
        /// Updates the projectile's position
        /// </summary>
        public void Update()
        {
            if (!IsActive) return;

            X += DX;
            Y += DY;
            DistanceTraveled++;

            // Auto-deactivate when max range is reached
            if (DistanceTraveled >= Range)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Renders the projectile on the world grid
        /// </summary>
        /// <param name="world">World to render in</param>
        public void Render(World world)
        {
            if (!IsActive) return;

            char projectileChar = IsPlayerProjectile ? '*' : '•';

            // Direction-based projectile characters for better visibility
            if (IsPlayerProjectile)
            {
                switch (Direction)
                {
                    case "up": projectileChar = '↑'; break;
                    case "right": projectileChar = '→'; break;
                    case "down": projectileChar = '↓'; break;
                    case "left": projectileChar = '←'; break;
                    default: projectileChar = '*'; break;
                }
            }
            else
            {
                switch (Direction)
                {
                    case "up": projectileChar = '↟'; break;
                    case "right": projectileChar = '↠'; break;
                    case "down": projectileChar = '↡'; break;
                    case "left": projectileChar = '↞'; break;
                    default: projectileChar = '•'; break;
                }
            }

            world.SetTile(X, Y, projectileChar);
        }

        /// <summary>
        /// Activates the projectile
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the projectile
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Resets the projectile to initial state
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;
            DX = 0;
            DY = 0;
            Damage = 0;
            Range = 0;
            DistanceTraveled = 0;
            IsPlayerProjectile = false;
            IsActive = false;
        }
    }
}