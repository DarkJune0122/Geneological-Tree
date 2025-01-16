using System;
using System.Threading.Tasks;

namespace Gen.Networking
{
    public static class AsyncHelpers
    {
        public static void WhenCompleted<T>(this Task<T> task, Action<T> action)
        {
            _ = WaitForCompletion();
            async Task WaitForCompletion()
            {
                T result = await task;
                await Task.Yield();
                action(result);
            }
        }
    }
}
