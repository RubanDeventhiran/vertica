using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Vertica.Data.VerticaClient;

namespace VerticaConnectionIssues.Sample
{
    [TestFixture]
    internal class MaxPoolTest
    {
        public static Boolean ThreadSleeping = false;

        [Test]
        public void ShouldAllowOnlyOneConnectionToDbWhenMaxPoolSizeIs_1()
        {
            Parallel.Invoke(
                () => Connection.Get(2),
                () =>
                {
                    //wait for other thread to get connection
                    Thread.Sleep(1000*2);
                    Connection.Get(1);
                }
                );
            Console.WriteLine("Done");
        }
    }

    internal static class Connection
    {
        public static DataTable Get(int id)
        {
            var dataTable = new DataTable();
            try
            {
                Console.WriteLine("[ID:{0}] using connection string :{1}", id, TestConfig.ConnectionString);
                using (var conn = new VerticaConnection(TestConfig.ConnectionString))
                {
                    conn.Open();
                    using (var tx = conn.BeginTransaction())
                    using (var command = conn.CreateCommand())
                    {
                        Console.WriteLine("[ID:{0}] thread id is :{1}", id,
                            Thread.CurrentThread.ManagedThreadId);

                        command.Transaction = tx;
                        command.CommandText = String.Format("Select * from dual;");
                        var reader = command.ExecuteReader();
                        Console.WriteLine("[ID:{0}] Start reading ", id);
                        dataTable.Load(reader);
                        Console.WriteLine("[ID:{0}] Finished reading", id);
                        if (id%2 == 0)
                        {
                            Console.WriteLine("[ID:{0}] going to sleep", id);
                            Thread.Sleep(1000*10);
                            Console.WriteLine("[ID:{0}] Waking Up", id);
                        }
                        Console.WriteLine("[ID:{0}] Finished executing", id);
                        tx.Commit();
                    }
                }
                return dataTable;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                throw;
            }
        }
    }
}