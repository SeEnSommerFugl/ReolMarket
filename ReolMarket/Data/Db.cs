using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace ReolMarket.Data
{
    internal class Db
    {
        /// <summary>
        /// Config builder for connection string, gets the "appsettings.json", with optional to false; it must be provided.
        /// "reloadOnChange" to false, means, no hot reload when changing the connection string.
        /// </summary>
        private static readonly IConfigurationRoot _config =
            new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();
        /// <summary>
        /// Encapsulaped connection string, for privacy concerns
        /// Gets the connection string for the default database connection.
        /// </summary>
        protected static string ConnectionString =>

            _config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing 'DefaultConnection'.");

        /// <summary>
        /// Opens an connection with the given connection string <see cref="SqlConnection()"/>, 
        /// which uses <param name="ConnectionString"></param>
        /// as a parameter input.
        /// </summary>
        /// 
        /// <returns>
        /// Returns the open connection with the connection string attached, to unsure proper encapsulation and privacy.
        /// </returns>
        internal static SqlConnection OpenConnection()
        {
            var con = new SqlConnection(ConnectionString);
            con.Open();
            return con;
        }
    }
}
