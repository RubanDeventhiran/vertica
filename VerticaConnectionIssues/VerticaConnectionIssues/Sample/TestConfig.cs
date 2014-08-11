using System.Configuration;

namespace VerticaConnectionIssues.Sample
{
    class TestConfig
    {
        public static readonly string SchemaName = ConfigurationManager.AppSettings["schemaName"];
        public static readonly string Table1 = ConfigurationManager.AppSettings["table1"];
        public static readonly string Table2 = ConfigurationManager.AppSettings["table2"];
        public static readonly string LargeTable = ConfigurationManager.AppSettings["largetable"];
        public static readonly string ConnectionString = ConfigurationManager.AppSettings["connectionString"];
    }
}
