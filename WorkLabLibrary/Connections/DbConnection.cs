using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;

namespace WorkLabLibrary.Connections
{
    public static class DbConnection
    {
        public static SqlConnection GetConnection()
        {
            return new(GetConnectionString());
        }

        private static string GetConnectionString()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

            return config.GetConnectionString("WorkLab");
        }
    }
}