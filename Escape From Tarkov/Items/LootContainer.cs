using System.Collections.Generic;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.ObjectManagement;

namespace ConsoleEscapeFromTarkov.Items
{
    /// <summary>
    /// Container that holds lootable items in the world
    /// </summary>
    public class LootContainer : IPoolable
    {
        private List<Item> items;
        private string name;

        /// <summary>
        /// X coordinate of the container
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y coordinate of the container
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Name of the container
        /// </summary>
        public string Name => name;

        /// <summary>
        /// List of items in the container
        /// </summary>
        public List<Item> Items => items;

        /// <summary>
        /// Whether the container is currently active
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Constructor for LootContainer
        /// </summary>
        public LootContainer()
        {
            items = new List<Item>();
            Reset();
        }

        /// <summary>
        /// Initializes the container with position and name
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="name">Name of the container</param>
        public void Initialize(int x, int y, string name)
        {
            X = x;
            Y = y;
            this.name = name;
            IsActive = true;
        }

        /// <summary>
        /// Adds an item to the container
        /// </summary>
        /// <param name="item">Item to add</param>
        public void AddItem(Item item)
        {
            items.Add(item);
        }

        /// <summary>
        /// Removes an item from the container
        /// </summary>
        /// <param name="item">Item to remove</param>
        public void RemoveItem(Item item)
        {
            items.Remove(item);

            // Auto-deactivate when empty
            if (items.Count == 0)
            {
                Deactivate();
            }
        }

        /// <summary>
        /// Activates the container
        /// </summary>
        public void Activate()
        {
            IsActive = true;
        }

        /// <summary>
        /// Deactivates the container
        /// </summary>
        public void Deactivate()
        {
            IsActive = false;
        }

        /// <summary>
        /// Updates the container (no-op)
        /// </summary>
        public void Update()
        {
            // Containers don't need updates
        }

        /// <summary>
        /// Renders the container in the world
        /// </summary>
        /// <param name="world">World to render in</param>
        public void Render(World world)
        {
            if (IsActive && items.Count > 0)
            {
                world.SetTile(X, Y, '▣');
            }
        }

        /// <summary>
        /// Resets the container to initial state
        /// </summary>
        public void Reset()
        {
            X = 0;
            Y = 0;
            name = "";
            items.Clear();
            IsActive = false;
        }
    }
}