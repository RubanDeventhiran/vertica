using System.Configuration;

namespace VerticaConnectionIssues.Sample
{
    class TestConfig
    {
        public static readonly string SchemaName = ConfigurationManager.AppSettings["schemaName"];

        public static readonly string ConnectionString = ConfigurationManager.AppSettings["connectionString"];
    }
}
