using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SqlClientCounters
{
    class Program
    {
        const string ConnectionString = "Server=.; Database=master; Integrated Security=false; User ID=sa; Password=Master1234; Connect Timeout=200; pooling='true'; Max Pool Size=200;";

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
                await using var conn = new SqlConnection(ConnectionString);
                await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                await cmd.ExecuteNonQueryAsync();

                Interlocked.Increment(ref Counter);
            }
        }
    }
}
