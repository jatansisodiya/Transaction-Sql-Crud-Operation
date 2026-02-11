using System.Collections;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Transaction.SQLConnection.Mapping;

/// <summary>
/// Provides entity mapping from SqlDataReader to strongly-typed objects.
/// </summary>
internal static class EntityMapper
{
    private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new();
    private static readonly object CacheLock = new();

    /// <summary>
    /// Reads a single entity from the current reader position.
    /// </summary>
    public static async Task<T?> ReadSingleAsync<T>(SqlDataReader reader, CancellationToken cancellationToken) where T : new()
    {
        if (!await reader.ReadAsync(cancellationToken))
        {
            return default;
        }

        return MapReaderToEntity<T>(reader);
    }

    /// <summary>
    /// Reads all entities from the current result set.
    /// </summary>
    public static async Task<List<T>> ReadListAsync<T>(SqlDataReader reader, CancellationToken cancellationToken) where T : new()
    {
        var list = new List<T>();

        while (await reader.ReadAsync(cancellationToken))
        {
            list.Add(MapReaderToEntity<T>(reader));
        }

        return list;
    }

    /// <summary>
    /// Reads a result set based on the target type (handles int, T, List&lt;T&gt;).
    /// </summary>
    public static async Task<T> ReadResultSetAsync<T>(SqlDataReader reader, CancellationToken cancellationToken)
    {
        var targetType = typeof(T);

        // Handle int type (scalar from result set)
        if (targetType == typeof(int))
        {
            if (await reader.ReadAsync(cancellationToken) && !reader.IsDBNull(0))
            {
                return (T)(object)Convert.ToInt32(reader.GetValue(0));
            }
            return (T)(object)0;
        }

        // Handle List<TElement>
        if (IsGenericList(targetType, out var elementType))
        {
            return await ReadListFromReaderAsync<T>(reader, elementType!, cancellationToken);
        }

        // Handle single entity
        return await ReadSingleFromReaderAsync<T>(reader, cancellationToken);
    }

    /// <summary>
    /// Checks if a type is a generic List&lt;T&gt; and extracts the element type.
    /// </summary>
    public static bool IsGenericList(Type type, out Type? elementType)
    {
        elementType = null;

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            elementType = type.GetGenericArguments()[0];
            return true;
        }

        return false;
    }

    private static async Task<T> ReadListFromReaderAsync<T>(SqlDataReader reader, Type elementType, CancellationToken cancellationToken)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        var columnMappings = GetColumnMappings(reader, elementType);

        while (await reader.ReadAsync(cancellationToken))
        {
            var entity = Activator.CreateInstance(elementType)!;
            MapReaderToEntity(reader, entity, columnMappings);
            list.Add(entity);
        }

        return (T)list;
    }

    private static async Task<T> ReadSingleFromReaderAsync<T>(SqlDataReader reader, CancellationToken cancellationToken)
    {
        if (!await reader.ReadAsync(cancellationToken))
        {
            return default!;
        }

        var entity = Activator.CreateInstance<T>()!;
        var columnMappings = GetColumnMappings(reader, typeof(T));
        MapReaderToEntity(reader, entity, columnMappings);

        return entity;
    }

    private static T MapReaderToEntity<T>(SqlDataReader reader) where T : new()
    {
        var entity = new T();
        var columnMappings = GetColumnMappings(reader, typeof(T));
        MapReaderToEntity(reader, entity, columnMappings);
        return entity;
    }

    private static void MapReaderToEntity(SqlDataReader reader, object entity, Dictionary<string, (PropertyInfo Property, int Ordinal)> columnMappings)
    {
        foreach (var (_, (property, ordinal)) in columnMappings)
        {
            if (reader.IsDBNull(ordinal))
            {
                continue;
            }

            var value = reader.GetValue(ordinal);
            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            try
            {
                var convertedValue = targetType.IsEnum
                    ? Enum.ToObject(targetType, value)
                    : Convert.ChangeType(value, targetType);

                property.SetValue(entity, convertedValue);
            }
            catch (InvalidCastException)
            {
                // Handle type mismatch gracefully - skip this property
            }
        }
    }

    private static Dictionary<string, (PropertyInfo Property, int Ordinal)> GetColumnMappings(SqlDataReader reader, Type entityType)
    {
        var mappings = new Dictionary<string, (PropertyInfo, int)>(StringComparer.OrdinalIgnoreCase);
        var properties = GetCachedProperties(entityType);

        var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columnNames.Add(reader.GetName(i));
        }

        foreach (var prop in properties)
        {
            if (columnNames.Contains(prop.Name))
            {
                var ordinal = reader.GetOrdinal(prop.Name);
                mappings[prop.Name] = (prop, ordinal);
            }
        }

        return mappings;
    }

    private static PropertyInfo[] GetCachedProperties(Type type)
    {
        lock (CacheLock)
        {
            if (!PropertyCache.TryGetValue(type, out var properties))
            {
                properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanWrite)
                    .ToArray();

                PropertyCache[type] = properties;
            }

            return properties;
        }
    }
}