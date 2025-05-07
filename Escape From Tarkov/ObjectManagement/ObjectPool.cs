using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.GameCore;

namespace ConsoleEscapeFromTarkov.ObjectManagement
{
    /// <summary>
    /// Generic object pool for managing reusable game objects
    /// </summary>
    /// <typeparam name="T">Type of object to pool (must implement IPoolable)</typeparam>
    public class ObjectPool<T> where T : IPoolable, new()
    {
        private List<T> pool;
        private int initialSize;
        private int maxSize;

        /// <summary>
        /// Constructor for ObjectPool
        /// </summary>
        /// <param name="initialSize">Initial number of objects to create</param>
        /// <param name="maxSize">Maximum pool size</param>
        public ObjectPool(int initialSize, int maxSize)
        {
            this.initialSize = initialSize;
            this.maxSize = maxSize;
            pool = new List<T>(maxSize);

            // Pre-allocate initial objects
            for (int i = 0; i < initialSize; i++)
            {
                pool.Add(new T());
            }
        }

        /// <summary>
        /// Gets an object from the pool
        /// </summary>
        /// <returns>An activated object</returns>
        public T Get()
        {
            // Try to reuse an inactive object first
            foreach (T obj in pool)
            {
                if (!obj.IsActive)
                {
                    obj.Activate();
                    return obj;
                }
            }

            // If we need more objects and haven't hit the max size
            if (pool.Count < maxSize)
            {
                T newObj = new T();
                pool.Add(newObj);
                newObj.Activate();
                return newObj;
            }

            // If we've reached the max size, find the oldest active object
            // This could be enhanced with more sophisticated logic
            T oldest = pool[0];
            oldest.Reset();
            oldest.Activate();
            return oldest;
        }

        /// <summary>
        /// Returns all objects to the pool
        /// </summary>
        public void ReturnAll()
        {
            foreach (T obj in pool)
            {
                obj.Deactivate();
            }
        }

        /// <summary>
        /// Updates all active objects
        /// </summary>
        public void Update()
        {
            foreach (T obj in pool)
            {
                if (obj.IsActive)
                {
                    obj.Update();
                }
            }
        }

        /// <summary>
        /// Renders all active objects
        /// </summary>
        /// <param name="world">World to render in</param>
        public void Render(World world)
        {
            foreach (T obj in pool)
            {
                if (obj.IsActive)
                {
                    obj.Render(world);
                }
            }
        }

        /// <summary>
        /// Gets all active objects in the pool
        /// </summary>
        /// <returns>List of active objects</returns>
        public List<T> GetActiveObjects()
        {
            return pool.Where(obj => obj.IsActive).ToList();
        }
    }
}