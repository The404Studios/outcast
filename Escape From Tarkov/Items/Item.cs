namespace ConsoleEscapeFromTarkov.Items
{
    /// <summary>
    /// Base class for all inventory items
    /// </summary>
    public abstract class Item
    {
        /// <summary>
        /// Name of the item
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Constructor for Item
        /// </summary>
        /// <param name="name">Name of the item</param>
        public Item(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets a description of the item
        /// </summary>
        /// <returns>Item description</returns>
        public virtual string GetDescription()
        {
            return Name;
        }
    }

    /// <summary>
    /// Weapon item that can be equipped and fired
    /// </summary>
    public class Weapon : Item
    {
        /// <summary>
        /// Damage dealt by the weapon
        /// </summary>
        public int Damage { get; private set; }

        /// <summary>
        /// Magazine capacity
        /// </summary>
        public int MagazineSize { get; private set; }

        /// <summary>
        /// Current ammo in the magazine
        /// </summary>
        public int CurrentAmmo { get; set; }

        /// <summary>
        /// Range of the weapon
        /// </summary>
        public int Range { get; private set; }

        /// <summary>
        /// Fire rate (in frames)
        /// </summary>
        public int FireRate { get; private set; }

        /// <summary>
        /// Spread of weapon projectiles
        /// </summary>
        public int Spread { get; private set; }

        /// <summary>
        /// Constructor for Weapon
        /// </summary>
        public Weapon(string name, int damage, int magazineSize, int range, int fireRate, int spread = 0)
            : base(name)
        {
            Damage = damage;
            MagazineSize = magazineSize;
            CurrentAmmo = 0;
            Range = range;
            FireRate = fireRate;
            Spread = spread;
        }

        /// <summary>
        /// Gets a description of the weapon
        /// </summary>
        /// <returns>Weapon description</returns>
        public override string GetDescription()
        {
            return $"{Name} (DMG:{Damage} | AMMO:{CurrentAmmo}/{MagazineSize})";
        }
    }

    /// <summary>
    /// Ammunition item for weapons
    /// </summary>
    public class Ammo : Item
    {
        /// <summary>
        /// Type of weapon this ammo is for
        /// </summary>
        public string WeaponType { get; private set; }

        /// <summary>
        /// Number of rounds
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Constructor for Ammo
        /// </summary>
        public Ammo(string name, string weaponType, int count)
            : base(name)
        {
            WeaponType = weaponType;
            Count = count;
        }

        /// <summary>
        /// Gets a description of the ammo
        /// </summary>
        /// <returns>Ammo description</returns>
        public override string GetDescription()
        {
            return $"{Name} x{Count} (For {WeaponType})";
        }
    }

    /// <summary>
    /// Medical item for healing
    /// </summary>
    public class MedKit : Item
    {
        /// <summary>
        /// Amount of health restored
        /// </summary>
        public int HealAmount { get; private set; }

        /// <summary>
        /// Constructor for MedKit
        /// </summary>
        public MedKit(string name, int healAmount)
            : base(name)
        {
            HealAmount = healAmount;
        }

        /// <summary>
        /// Gets a description of the medkit
        /// </summary>
        /// <returns>Medkit description</returns>
        public override string GetDescription()
        {
            return $"{Name} (+{HealAmount} HP)";
        }
    }

    /// <summary>
    /// Armor item for damage reduction
    /// </summary>
    public class Armor : Item
    {
        /// <summary>
        /// Protection amount (damage reduction %)
        /// </summary>
        public int Protection { get; private set; }

        /// <summary>
        /// Constructor for Armor
        /// </summary>
        public Armor(string name, int protection)
            : base(name)
        {
            Protection = protection;
        }

        /// <summary>
        /// Gets a description of the armor
        /// </summary>
        /// <returns>Armor description</returns>
        public override string GetDescription()
        {
            return $"{Name} (PROT:{Protection})";
        }
    }

    /// <summary>
    /// Valuable item with monetary worth
    /// </summary>
    public class Valuable : Item
    {
        /// <summary>
        /// Value of the item
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Whether this is a mission-specific item
        /// </summary>
        public bool IsMissionItem { get; private set; }

        /// <summary>
        /// Constructor for Valuable
        /// </summary>
        public Valuable(string name, int value, bool isMissionItem = false)
            : base(name)
        {
            Value = value;
            IsMissionItem = isMissionItem;
        }

        /// <summary>
        /// Gets a description of the valuable
        /// </summary>
        /// <returns>Valuable description</returns>
        public override string GetDescription()
        {
            return $"{Name} (₽{Value})" + (IsMissionItem ? " [MISSION]" : "");
        }
    }
}