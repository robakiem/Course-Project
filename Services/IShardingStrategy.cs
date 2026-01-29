namespace CourseProject.Services;

/// <summary>
/// Стратегія вибору шарду для розподіленого зберігання
/// Реалізує Strategy Pattern
/// </summary>
public interface IShardingStrategy
{
    int GetShardId(string key);
    string GetStrategyName();
}

/// <summary>
/// Hash-sharding: розподіл даних на основі хеша ключа
/// Забезпечує рівномірний розподіл навіть для впорядкованих даних
/// </summary>
public class HashShardingStrategy : IShardingStrategy
{
    private readonly int _shardCount;
    
    public HashShardingStrategy(int shardCount)
    {
        if (shardCount <= 0)
            throw new ArgumentException("Кількість шардів мусить бути > 0", nameof(shardCount));
        _shardCount = shardCount;
    }

    public int GetShardId(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException("Ключ не може бути пустий", nameof(key));
        
        return Math.Abs(key.GetHashCode()) % _shardCount;
    }

    public string GetStrategyName() => $"Hash-Sharding (Шардів: {_shardCount})";
}

/// <summary>
/// Range-sharding: розподіл даних за числовими інтервалами (наприклад, регіони)
/// Оптимальний для географічного розподілу
/// </summary>
public class RangeShardingStrategy : IShardingStrategy
{
    private readonly Dictionary<int, (int min, int max)> _ranges;

    public RangeShardingStrategy(Dictionary<int, (int min, int max)> ranges)
    {
        _ranges = ranges ?? throw new ArgumentNullException(nameof(ranges));
    }

    public int GetShardId(string key)
    {
        if (!int.TryParse(key, out int value))
            throw new ArgumentException("Range-sharding вимагає числовий ключ", nameof(key));

        var shard = _ranges.FirstOrDefault(r => value >= r.Value.min && value <= r.Value.max);
        if (shard.Key == 0 && !_ranges.ContainsKey(0))
            throw new InvalidOperationException($"Значення {value} не входить в жодний діапазон");

        return shard.Key;
    }

    public string GetStrategyName() => "Range-Sharding (Географічний розподіл)";
}

/// <summary>
/// Directory-based sharding: явна таблиця відповідності ключ->шард
/// Найгнучкіший варіант, але вимагає обновлення за змінами
/// </summary>
public class DirectoryShardingStrategy : IShardingStrategy
{
    private readonly Dictionary<string, int> _directory;

    public DirectoryShardingStrategy(Dictionary<string, int> directory)
    {
        _directory = directory ?? throw new ArgumentNullException(nameof(directory));
    }

    public int GetShardId(string key)
    {
        if (!_directory.TryGetValue(key, out int shardId))
            throw new KeyNotFoundException($"Ключ '{key}' не знайдено в директорії шардингу");

        return shardId;
    }

    public string GetStrategyName() => "Directory-Sharding (Явна таблиця)";
}