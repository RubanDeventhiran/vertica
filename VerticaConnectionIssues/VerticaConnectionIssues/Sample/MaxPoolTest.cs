using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Vertica.Data.VerticaClient;

namespace VerticaConnectionIssues.Sample
{
    // The connection pool for this is set to 1. Hence if multiple threads are trying to read data from the database they should not be able 
    // to do so simultaneously. To recreate this we start two threads such that one of the thread(id = 1) waits for 2 sec before it stats, meanwhile
    // the other thread(id= 2) will go ahead and get the connection and read data from it after which it will wait such that the connection is still
    // held by this thread. now thread(id =1) awakens and it is still able to get the connection to db and process it, shouldnt it have waited for 
    // thread(id = 2) to finish before it gets a hold of db.
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