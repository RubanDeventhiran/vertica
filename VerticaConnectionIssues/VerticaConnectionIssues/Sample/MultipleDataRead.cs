using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using NUnit.Framework;
using Vertica.Data.VerticaClient;

namespace VerticaConnectionIssues.Sample
{
    // This tests that on a single connection if we run multiple queries such that datareader is used to read data for each of those queries
    // then there should be error reported as per Vertica  Support. But this is not the case.

    [TestFixture]
    class MultipleDataRead
    {
        [Test]
        public void FromSameConnectionSequentially()
        {
            IEnumerable<Tuple<DataTable, Func<string, string>>> tuples = new List<Tuple<DataTable, Func<string, string>>>
            {
                new Tuple<DataTable,  Func<string, string>>(new DataTable(), schemaName => String.Format("select * from {0}.{1};", schemaName,TestConfig.Table1)),
                new Tuple<DataTable,  Func<string, string>>(new DataTable(), schemaName => String.Format( "select * from {0}.{1};", schemaName, TestConfig.Table2)),
            };

            using (var conn = new VerticaConnection(TestConfig.ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    foreach (var tuple in tuples)
                    {
                        using (var command = conn.CreateCommand())
                        {
                            command.CommandText = tuple.Item2(TestConfig.SchemaName);
                            var verticaDataReader = command.ExecuteReader();
                            tuple.Item1.Load(verticaDataReader);
                        }
                    }
                    tx.Commit();
                }
            }

            Assert.True(tuples.All(t => t.Item1.Rows.Count > 0), "Data should be loaded from all rows");
        }
    }
}
