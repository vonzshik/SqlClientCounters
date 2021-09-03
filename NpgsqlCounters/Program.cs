using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace NpgsqlCounters
{
    class Program
    {
        const string ConnectionString = "Host=localhost; Database=tessa360; User ID=postgres; Password=Master1234; Pooling=true; MaxPoolSize=100";

        static int Counter;

        static async Task Main(string[] args)
        {
            var tasks = new List<Task>(4);
            for (var i = 0; i < 4; i++)
            {
                tasks.Add(Task.Run(Query));
            }

            _ = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);

                    var counter = Interlocked.Exchange(ref Counter, 0);
                    Console.WriteLine($"RPS: {counter}");
                }
            });

            await Task.WhenAll(tasks);
        }

        static async Task Query()
        {
            while (true)
            {
                await using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteNonQueryAsync();

                Interlocked.Increment(ref Counter);
            }
        }
    }
}
