using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleEscapeFromTarkov.Utils;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Represents the game world, handling map generation, collision, and rendering
    /// </summary>
    public class World
    {
        private char[,] grid;
        private char[,] previousGrid; // For double-buffering
        private bool[,] collisionMap;
        private List<MapFeature> mapFeatures;
        private List<Point> extractionPoints;
        private Random random;

        /// <summary>
        /// Width of the world grid
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the world grid
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Constructor for the World class
        /// </summary>
        /// <param name="width">Width of the world</param>
        /// <param name="height">Height of the world</param>
        public World(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new char[width, height];
            previousGrid = new char[width, height];
            collisionMap = new bool[width, height];
            mapFeatures = new List<MapFeature>();
            extractionPoints = new List<Point>();
            random = new Random();

            // Initialize grids
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    grid[x, y] = ' ';
                    previousGrid[x, y] = ' ';
                }
            }

            Generate();
        }

        /// <summary>
        /// Generates a new world map with zones, features, and extraction points
        /// </summary>
        public void Generate()
        {
            // Reset maps
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    collisionMap[x, y] = false;
                    grid[x, y] = ' ';
                    previousGrid[x, y] = ' ';
                }
            }

            // Clear existing features
            mapFeatures.Clear();
            extractionPoints.Clear();

            // Create walls around the map
            for (int x = 0; x < Width; x++)
            {
                SetCollision(x, 0, true);
                SetCollision(x, Height - 1, true);
            }

            for (int y = 0; y < Height; y++)
            {
                SetCollision(0, y, true);
                SetCollision(Width - 1, y, true);
            }

            // Generate map zones
            GenerateForestZone(5, 5, 25, 20);
            GenerateUrbanZone(35, 5, 30, 20);
            GenerateIndustrialZone(70, 5, 25, 20);

            // Add map features
            AddMapFeatures();

            // Add extraction points
            AddExtractionPoints();

            // Clear center area for player start
            ClearPlayerStartArea();
        }

        /// <summary>
        /// Clears an area in the center of the map for the player to start
        /// </summary>
        private void ClearPlayerStartArea()
        {
            int centerX = Width / 2;
            int centerY = Height / 2;
            int clearRadius = 3;

            for (int x = centerX - clearRadius; x <= centerX + clearRadius; x++)
            {
                for (int y = centerY - clearRadius; y <= centerY + clearRadius; y++)
                {
                    if (x > 0 && x < Width - 1 && y > 0 && y < Height - 1)
                    {
                        SetCollision(x, y, false);
                    }
                }
            }
        }

        /// <summary>
        /// Generates a forest zone in the specified area
        /// </summary>
        private void GenerateForestZone(int startX, int startY, int width, int height)
        {
            // Add some trees and rocks
            for (int i = 0; i < 40; i++)
            {
                int x = random.Next(startX, startX + width);
                int y = random.Next(startY, startY + height);

                if (x > 0 && x < Width - 1 && y > 0 && y < Height - 1)
                {
                    if (random.Next(2) == 0)
                    {
                        // Tree
                        mapFeatures.Add(new MapFeature(x, y, 'T', "Tree", "A tall tree", true));
                        SetCollision(x, y, true);
                    }
                    else
                    {
                        // Rock
                        mapFeatures.Add(new MapFeature(x, y, 'ᴖ', "Rock", "A large rock", true));
                        SetCollision(x, y, true);
                    }
                }
            }

            // Add small water pond
            int pondX = random.Next(startX + 5, startX + width - 5);
            int pondY = random.Next(startY + 5, startY + height - 5);

            for (int x = pondX - 2; x <= pondX + 2; x++)
            {
                for (int y = pondY - 1; y <= pondY + 1; y++)
                {
                    if (x > 0 && x < Width - 1 && y > 0 && y < Height - 1)
                    {
                        mapFeatures.Add(new MapFeature(x, y, '~', "Water", "A small pond", true, waterPond: true));
                        SetCollision(x, y, true);
                    }
                }
            }
        }

        /// <summary>
        /// Generates an urban zone with buildings in the specified area
        /// </summary>
        private void GenerateUrbanZone(int startX, int startY, int width, int height)
        {
            // Generate buildings
            int buildingCount = random.Next(3, 6);

            for (int i = 0; i < buildingCount; i++)
            {
                int buildingX = random.Next(startX + 2, startX + width - 7);
                int buildingY = random.Next(startY + 2, startY + height - 7);
                int buildingWidth = random.Next(5, 8);
                int buildingHeight = random.Next(4, 6);

                // Create building
                for (int x = buildingX; x < buildingX + buildingWidth; x++)
                {
                    for (int y = buildingY; y < buildingY + buildingHeight; y++)
                    {
                        // Building walls
                        if (x == buildingX || x == buildingX + buildingWidth - 1 ||
                            y == buildingY || y == buildingY + buildingHeight - 1)
                        {
                            SetCollision(x, y, true);
                            mapFeatures.Add(new MapFeature(x, y, '▓', "Building", "An abandoned building", true));
                        }
                        else
                        {
                            // Inside of building - sometimes put obstacles
                            if (random.Next(5) == 0)
                            {
                                SetCollision(x, y, true);
                                mapFeatures.Add(new MapFeature(x, y, '░', "Furniture", "Abandoned furniture", true));
                            }
                        }
                    }
                }

                // Add a door
                AddBuildingDoor(buildingX, buildingY, buildingWidth, buildingHeight);
            }

            // Add some urban decorations: street lamps, benches, etc.
            for (int i = 0; i < 10; i++)
            {
                int x = random.Next(startX, startX + width);
                int y = random.Next(startY, startY + height);

                if (x > 0 && x < Width - 1 && y > 0 && y < Height - 1 && !IsCollision(x, y))
                {
                    char decoration = random.Next(2) == 0 ? '¶' : '┬';
                    string name = decoration == '¶' ? "Street Lamp" : "Bench";
                    string description = decoration == '¶' ? "A broken street lamp" : "A wooden bench";

                    mapFeatures.Add(new MapFeature(x, y, decoration, name, description, random.Next(2) == 0));
                    if (mapFeatures.Last().HasCollision)
                    {
                        SetCollision(x, y, true);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a door to a building
        /// </summary>
        private void AddBuildingDoor(int buildingX, int buildingY, int buildingWidth, int buildingHeight)
        {
            int doorSide = random.Next(4);
            int doorX = buildingX, doorY = buildingY;

            switch (doorSide)
            {
                case 0: // Top
                    doorX = buildingX + random.Next(1, buildingWidth - 1);
                    doorY = buildingY;
                    break;
                case 1: // Right
                    doorX = buildingX + buildingWidth - 1;
                    doorY = buildingY + random.Next(1, buildingHeight - 1);
                    break;
                case 2: // Bottom
                    doorX = buildingX + random.Next(1, buildingWidth - 1);
                    doorY = buildingY + buildingHeight - 1;
                    break;
                case 3: // Left
                    doorX = buildingX;
                    doorY = buildingY + random.Next(1, buildingHeight - 1);
                    break;
            }

            // Remove collision for door
            SetCollision(doorX, doorY, false);

            // Remove map feature for the door position
            mapFeatures.RemoveAll(f => f.X == doorX && f.Y == doorY);

            // Add door feature
            mapFeatures.Add(new MapFeature(doorX, doorY, '╬', "Door", "A building entrance", false));
        }

        /// <summary>
        /// Generates an industrial zone with warehouses in the specified area
        /// </summary>
        private void GenerateIndustrialZone(int startX, int startY, int width, int height)
        {
            // Generate warehouses and factories
            int warehouseCount = random.Next(2, 4);

            for (int i = 0; i < warehouseCount; i++)
            {
                int warehouseX = random.Next(startX + 2, startX + width - 10);
                int warehouseY = random.Next(startY + 2, startY + height - 8);
                int warehouseWidth = random.Next(8, 10);
                int warehouseHeight = random.Next(6, 8);

                // Create warehouse
                for (int x = warehouseX; x < warehouseX + warehouseWidth; x++)
                {
                    for (int y = warehouseY; y < warehouseY + warehouseHeight; y++)
                    {
                        // Warehouse walls
                        if (x == warehouseX || x == warehouseX + warehouseWidth - 1 ||
                            y == warehouseY || y == warehouseY + warehouseHeight - 1)
                        {
                            SetCollision(x, y, true);
                            mapFeatures.Add(new MapFeature(x, y, '█', "Warehouse", "A large warehouse", true));
                        }
                        else
                        {
                            // Inside of warehouse - storage containers
                            if (random.Next(3) == 0)
                            {
                                SetCollision(x, y, true);
                                mapFeatures.Add(new MapFeature(x, y, '▣', "Container", "A storage container", true));
                            }
                        }
                    }
                }

                // Add a door
                int doorX = warehouseX + random.Next(1, warehouseWidth - 1);
                int doorY = warehouseY + warehouseHeight - 1; // Doors on bottom side

                // Remove collision for door
                SetCollision(doorX, doorY, false);

                // Remove map feature for the door position
                mapFeatures.RemoveAll(f => f.X == doorX && f.Y == doorY);

                // Add door feature
                mapFeatures.Add(new MapFeature(doorX, doorY, '╬', "Door", "A warehouse entrance", false));
            }

            // Add industrial props: barrels, crates, machinery
            for (int i = 0; i < 15; i++)
            {
                int x = random.Next(startX, startX + width);
                int y = random.Next(startY, startY + height);

                if (x > 0 && x < Width - 1 && y > 0 && y < Height - 1 && !IsCollision(x, y))
                {
                    char prop;
                    string name;
                    string description;
                    bool hasLoot = random.Next(5) == 0; // 20% chance for loot

                    switch (random.Next(3))
                    {
                        case 0:
                            prop = 'O';
                            name = "Barrel";
                            description = "A metal barrel" + (hasLoot ? " (might contain loot)" : "");
                            break;
                        case 1:
                            prop = '▦';
                            name = "Crate";
                            description = "A wooden crate" + (hasLoot ? " (might contain loot)" : "");
                            break;
                        default:
                            prop = '⚙';
                            name = "Machinery";
                            description = "Industrial machinery" + (hasLoot ? " (might contain loot)" : "");
                            break;
                    }

                    mapFeatures.Add(new MapFeature(x, y, prop, name, description, true, hasLoot));
                    SetCollision(x, y, true);
                }
            }
        }

        /// <summary>
        /// Adds special map features like healing stations and ammo caches
        /// </summary>
        private void AddMapFeatures()
        {
            // Add a healing station
            int medStationX = random.Next(10, Width - 10);
            int medStationY = random.Next(10, Height - 10);

            if (!IsCollision(medStationX, medStationY))
            {
                mapFeatures.Add(new MapFeature(medStationX, medStationY, '+', "Medical Station", "A place to heal", false, false, true));
            }

            // Add an ammo cache
            int ammoCacheX = random.Next(10, Width - 10);
            int ammoCacheY = random.Next(10, Height - 10);

            if (!IsCollision(ammoCacheX, ammoCacheY))
            {
                mapFeatures.Add(new MapFeature(ammoCacheX, ammoCacheY, '⚡', "Ammo Cache", "A cache of ammunition", false, false, false, true));
            }
        }

        /// <summary>
        /// Adds extraction points to the map, allowing the player to exit the raid
        /// </summary>
        private void AddExtractionPoints()
        {
            // Add 3 extraction points
            for (int i = 0; i < 3; i++)
            {
                bool locationFound = false;
                int x = 0, y = 0;

                // Try to find a valid location near the edges of the map
                for (int attempts = 0; attempts < 100 && !locationFound; attempts++)
                {
                    bool nearEdge = false;

                    x = random.Next(5, Width - 5);
                    y = random.Next(5, Height - 5);

                    // Check if it's near an edge
                    if (x < 10 || x > Width - 10 || y < 10 || y > Height - 10)
                    {
                        nearEdge = true;
                    }

                    if (nearEdge && !IsCollision(x, y))
                    {
                        locationFound = true;
                    }
                }

                if (locationFound)
                {
                    extractionPoints.Add(new Point(x, y));
                }
            }
        }

        /// <summary>
        /// Prepares the grid for rendering a new frame
        /// </summary>
        public void PrepareForRendering()
        {
            // Copy current grid to previous grid for double-buffering
            Array.Copy(grid, previousGrid, grid.Length);

            // Clear current grid
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    grid[x, y] = ' ';
                }
            }

            // Draw walls
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (IsCollision(x, y))
                    {
                        // Check if this is a map feature
                        MapFeature feature = GetFeatureAt(x, y);
                        if (feature != null)
                        {
                            grid[x, y] = feature.Symbol;
                        }
                        else
                        {
                            grid[x, y] = '█';
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renders map features that don't have collision
        /// </summary>
        public void RenderMapFeatures()
        {
            // Render map features that don't have collision
            foreach (MapFeature feature in mapFeatures)
            {
                if (!feature.HasCollision)
                {
                    SetTile(feature.X, feature.Y, feature.Symbol);
                }
            }

            // Render extraction points
            foreach (Point extraction in extractionPoints)
            {
                SetTile(extraction.X, extraction.Y, 'X');
            }
        }

        /// <summary>
        /// Sets a tile at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="tile">Character to set</param>
        public void SetTile(int x, int y, char tile)
        {
            if (IsInBounds(x, y))
            {
                grid[x, y] = tile;
            }
        }

        /// <summary>
        /// Gets the tile at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The character at the position</returns>
        public char GetTile(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return grid[x, y];
            }

            return ' ';
        }

        /// <summary>
        /// Gets the previous frame's tile at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The character at the position from the previous frame</returns>
        public char GetPreviousTile(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return previousGrid[x, y];
            }

            return ' ';
        }

        /// <summary>
        /// Checks if a tile has changed since the last frame
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if the tile changed</returns>
        public bool HasTileChanged(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return grid[x, y] != previousGrid[x, y];
            }

            return false;
        }

        /// <summary>
        /// Sets whether a position has collision
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="hasCollision">Whether the position has collision</param>
        public void SetCollision(int x, int y, bool hasCollision)
        {
            if (IsInBounds(x, y))
            {
                collisionMap[x, y] = hasCollision;
            }
        }

        /// <summary>
        /// Checks if a position has collision
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if the position has collision</returns>
        public bool IsCollision(int x, int y)
        {
            if (IsInBounds(x, y))
            {
                return collisionMap[x, y];
            }

            return true; // Out of bounds is always collision
        }

        /// <summary>
        /// Checks if a position is within the bounds of the world
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if the position is in bounds</returns>
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// Gets a map feature at the specified position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The map feature, or null if none exists</returns>
        public MapFeature GetFeatureAt(int x, int y)
        {
            return mapFeatures.FirstOrDefault(f => f.X == x && f.Y == y);
        }

        /// <summary>
        /// Checks if a position is an extraction point
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if the position is an extraction point</returns>
        public bool IsExtractionPoint(int x, int y)
        {
            return extractionPoints.Any(p => p.X == x && p.Y == y);
        }

        /// <summary>
        /// Gets all map features
        /// </summary>
        /// <returns>List of all map features</returns>
        public List<MapFeature> GetMapFeatures()
        {
            return mapFeatures;
        }

        /// <summary>
        /// Gets all extraction points
        /// </summary>
        /// <returns>List of extraction points</returns>
        public List<Point> GetExtractionPoints()
        {
            return extractionPoints;
        }
    }
}