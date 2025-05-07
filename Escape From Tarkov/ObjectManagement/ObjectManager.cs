using System.Collections.Generic;
using ConsoleEscapeFromTarkov.Entities;
using ConsoleEscapeFromTarkov.GameCore;
using ConsoleEscapeFromTarkov.Items;

namespace ConsoleEscapeFromTarkov.ObjectManagement
{
    /// <summary>
    /// Manages all object pools for the game
    /// </summary>
    public class ObjectManager
    {
        private ObjectPool<Projectile> projectilePool;
        private ObjectPool<LootContainer> lootContainerPool;
        private ObjectPool<VisualEffect> effectPool;

        /// <summary>
        /// Constructor for ObjectManager
        /// </summary>
        public ObjectManager()
        {
            projectilePool = new ObjectPool<Projectile>(50, 200);
            lootContainerPool = new ObjectPool<LootContainer>(20, 50);
            effectPool = new ObjectPool<VisualEffect>(20, 50);
        }

        /// <summary>
        /// Gets a projectile from the pool
        /// </summary>
        /// <returns>An activated projectile</returns>
        public Projectile GetProjectile()
        {
            return projectilePool.Get();
        }

        /// <summary>
        /// Gets a loot container from the pool
        /// </summary>
        /// <returns>An activated loot container</returns>
        public LootContainer GetLootContainer()
        {
            return lootContainerPool.Get();
        }

        /// <summary>
        /// Gets a visual effect from the pool
        /// </summary>
        /// <returns>An activated visual effect</returns>
        public VisualEffect GetEffect()
        {
            return effectPool.Get();
        }

        /// <summary>
        /// Updates all active objects
        /// </summary>
        public void Update()
        {
            projectilePool.Update();
            effectPool.Update();
        }

        /// <summary>
        /// Renders all active objects
        /// </summary>
        /// <param name="world">World to render in</param>
        public void Render(World world)
        {
            projectilePool.Render(world);
            effectPool.Render(world);
        }

        /// <summary>
        /// Gets all active projectiles
        /// </summary>
        /// <returns>List of active projectiles</returns>
        public List<Projectile> GetActiveProjectiles()
        {
            return projectilePool.GetActiveObjects();
        }

        /// <summary>
        /// Gets all active loot containers
        /// </summary>
        /// <returns>List of active loot containers</returns>
        public List<LootContainer> GetActiveLootContainers()
        {
            return lootContainerPool.GetActiveObjects();
        }

        /// <summary>
        /// Resets all object pools
        /// </summary>
        public void Reset()
        {
            projectilePool.ReturnAll();
            lootContainerPool.ReturnAll();
            effectPool.ReturnAll();
        }
    }
}