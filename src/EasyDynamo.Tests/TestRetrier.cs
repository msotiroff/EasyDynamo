using System;
using System.Threading.Tasks;

namespace EasyDynamo.Tests
{
    public static class TestRetrier
    {
        public static async Task RetryAsync(Action action, int retries = 5, int delay = 100)
        {
            var passed = false;

            while (!passed && retries > 0)
            {
                try
                {
                    action();

                    passed = true;
                }
                catch
                {
                    retries--;

                    await Task.Delay(delay);
                }
            }
        }
    }
}
