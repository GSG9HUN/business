using System.Runtime.CompilerServices;

namespace DC_bot_tests.TestHelperFiles;

public static class AsyncEnumerableAwaiterExtensions
{
    public static TaskAwaiter<IReadOnlyList<T>> GetAwaiter<T>(this IAsyncEnumerable<T> source)
    {
        return ToReadOnlyListAsync(source).GetAwaiter();
    }

    private static async Task<IReadOnlyList<T>> ToReadOnlyListAsync<T>(IAsyncEnumerable<T> source)
    {
        var items = new List<T>();

        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }
}
