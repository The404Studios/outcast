using System;

namespace ConsoleEscapeFromTarkov.GameCore
{
    /// <summary>
    /// Manages weather conditions and effects in the game world
    /// </summary>
    public class WeatherSystem
    {
        /// <summary>
        /// Types of weather in the game
        /// </summary>
        public enum WeatherType
        {
            Clear,
            Fog,
            Rain,
            Storm
        }

        private Random random;

        /// <summary>
        /// Current weather condition
        /// </summary>
        public WeatherType CurrentWeather { get; private set; }

        private int fogDensity;

        /// <summary>
        /// Constructor for WeatherSystem
        /// </summary>
        public WeatherSystem()
        {
            random = new Random();
            Reset();
        }

        /// <summary>
        /// Resets the weather to default state
        /// </summary>
        public void Reset()
        {
            CurrentWeather = WeatherType.Clear;
            fogDensity = 0;
        }

        /// <summary>
        /// Updates the weather condition with a chance to change
        /// </summary>
        public void UpdateWeather()
        {
            // 40% chance to change weather
            if (random.Next(100) < 40)
            {
                // Weather transition chances
                int roll = random.Next(100);

                switch (CurrentWeather)
                {
                    case WeatherType.Clear:
                        if (roll < 60) CurrentWeather = WeatherType.Fog;
                        else if (roll < 90) CurrentWeather = WeatherType.Rain;
                        else CurrentWeather = WeatherType.Storm;
                        break;
                    case WeatherType.Fog:
                        if (roll < 50) CurrentWeather = WeatherType.Clear;
                        else if (roll < 90) CurrentWeather = WeatherType.Rain;
                        else CurrentWeather = WeatherType.Storm;
                        break;
                    case WeatherType.Rain:
                        if (roll < 30) CurrentWeather = WeatherType.Clear;
                        else if (roll < 60) CurrentWeather = WeatherType.Fog;
                        else CurrentWeather = WeatherType.Storm;
                        break;
                    case WeatherType.Storm:
                        if (roll < 30) CurrentWeather = WeatherType.Clear;
                        else if (roll < 50) CurrentWeather = WeatherType.Fog;
                        else CurrentWeather = WeatherType.Rain;
                        break;
                }

                // Update fog density
                if (CurrentWeather == WeatherType.Fog)
                {
                    fogDensity = random.Next(3, 8);
                }
            }
        }

        /// <summary>
        /// Applies visual weather effects to the world
        /// </summary>
        /// <param name="world">World to apply effects to</param>
        public void ApplyWeatherEffects(World world)
        {
            switch (CurrentWeather)
            {
                case WeatherType.Fog:
                    ApplyFogEffect(world);
                    break;
                case WeatherType.Rain:
                    ApplyRainEffect(world);
                    break;
                case WeatherType.Storm:
                    ApplyStormEffect(world);
                    break;
                    // Clear weather has no special effects
            }
        }

        /// <summary>
        /// Applies fog visual effects
        /// </summary>
        /// <param name="world">World to apply to</param>
        private void ApplyFogEffect(World world)
        {
            // Add random fog particles
            for (int i = 0; i < fogDensity * 10; i++)
            {
                int x = random.Next(1, world.Width - 1);
                int y = random.Next(1, world.Height - 1);

                if (!world.IsCollision(x, y) && world.GetTile(x, y) == ' ')
                {
                    world.SetTile(x, y, '░');
                }
            }
        }

        /// <summary>
        /// Applies rain visual effects
        /// </summary>
        /// <param name="world">World to apply to</param>
        private void ApplyRainEffect(World world)
        {
            // Add random raindrops
            for (int i = 0; i < 50; i++)
            {
                int x = random.Next(1, world.Width - 1);
                int y = random.Next(1, world.Height - 1);

                if (!world.IsCollision(x, y) && world.GetTile(x, y) == ' ')
                {
                    world.SetTile(x, y, '.');
                }
            }
        }

        /// <summary>
        /// Applies storm visual effects with lightning
        /// </summary>
        /// <param name="world">World to apply to</param>
        private void ApplyStormEffect(World world)
        {
            // Add random raindrops and lightning flashes
            for (int i = 0; i < 100; i++)
            {
                int x = random.Next(1, world.Width - 1);
                int y = random.Next(1, world.Height - 1);

                if (!world.IsCollision(x, y) && world.GetTile(x, y) == ' ')
                {
                    char rainSymbol = random.Next(10) == 0 ? '|' : '.';
                    world.SetTile(x, y, rainSymbol);
                }
            }

            // Occasional lightning flash
            if (random.Next(30) == 0)
            {
                int flashX = random.Next(1, world.Width - 1);
                int flashY = random.Next(1, world.Height - 1);

                if (!world.IsCollision(flashX, flashY))
                {
                    world.SetTile(flashX, flashY, '*');
                }
            }
        }

        /// <summary>
        /// Gets a text description of the current weather
        /// </summary>
        /// <returns>Weather description</returns>
        public string GetWeatherDescription()
        {
            switch (CurrentWeather)
            {
                case WeatherType.Clear:
                    return "Clear skies";
                case WeatherType.Fog:
                    return $"Foggy (Density: {fogDensity})";
                case WeatherType.Rain:
                    return "Rainy";
                case WeatherType.Storm:
                    return "Thunderstorm";
                default:
                    return "Unknown weather";
            }
        }
    }
}