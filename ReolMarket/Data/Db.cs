using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ReolMarket.Data
{
    internal class Db
    {

        private static readonly IConfigurationRoot _config =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false) // 👈 ingen reload
                .Build();

        // Slå strengen op når vi skal bruge den (enkelt og robust)
        internal static string ConnectionString =>
            _config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing 'DefaultConnection'.");

        // Praktisk helper
        internal static SqlConnection OpenConnection()
        {
            var con = new SqlConnection(ConnectionString);
            con.Open();
            return con;
        }
    }
}
