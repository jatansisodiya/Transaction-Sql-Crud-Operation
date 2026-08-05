using Dapper;

namespace Transaction.SQLConnection.Mapping;

/// <summary>
/// Provides entity mapping helpers powered by Dapper.
/// </summary>
internal static class EntityMapper
{
    /// <summary>
    /// Reads a result set from Dapper's GridReader based on the target type T (supports int, T, List&lt;T&gt;).
    /// </summary>
    public static async Task<T> ReadResultFromGridAsync<T>(SqlMapper.GridReader gridReader)
    {
        var targetType = typeof(T);

        // Case 1: Return int (scalar from result set)
        if (targetType == typeof(int))
        {
            var val = await gridReader.ReadFirstOrDefaultAsync<int>();
            return (T)(object)val;
        }

        // Case 2: Return List<TElement>
        if (IsGenericList(targetType, out var elementType))
        {
            var method = typeof(SqlMapper.GridReader)
                .GetMethods()
                .First(m => m.Name == nameof(SqlMapper.GridReader.ReadAsync) && m.IsGenericMethod && m.GetGenericArguments().Length == 1)
                .MakeGenericMethod(elementType!);

            var task = (Task)method.Invoke(gridReader, [true])!;
            await task.ConfigureAwait(false);

            var enumerableResult = task.GetType().GetProperty("Result")!.GetValue(task)!;

            var toListMethod = typeof(Enumerable)
                .GetMethod(nameof(Enumerable.ToList))!
                .MakeGenericMethod(elementType!);

            var list = toListMethod.Invoke(null, [enumerableResult])!;
            return (T)list;
        }

        // Case 3: Return single entity T
        var singleResult = await gridReader.ReadFirstOrDefaultAsync<T>();
        return singleResult!;
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
}