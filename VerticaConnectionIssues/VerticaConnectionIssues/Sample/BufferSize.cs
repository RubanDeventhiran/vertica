using System;
using System.Data;
using NUnit.Framework;
using Vertica.Data.VerticaClient;

namespace VerticaConnectionIssues.Sample
{
    // The ResultBufferSize should always be greater than the result returned from the db for a query according to Vertica Support. 
    // here the resultBufferSize for the connection is 512. The result set returned from the below query is around 1.5MB so how is this working?
    [TestFixture]
    class BufferSize
    {
        [Test]
        public void ResultLargerThanBufferWhenReadShouldFail()
        {
            var result = new DataTable();
            using (var conn = new VerticaConnection(TestConfig.ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                using(var command  = conn.CreateCommand())
                {
                    command.CommandText = String.Format("Select * from {0}.{1} limit 2000;",
                        TestConfig.SchemaName, TestConfig.LargeTable);

                    var verticaDataReader = command.ExecuteReader();
                    result.Load(verticaDataReader);
                    tx.Commit();
                }
            }

            Assert.That(result.Rows.Count, Is.EqualTo(2000));
        }
    }
}
