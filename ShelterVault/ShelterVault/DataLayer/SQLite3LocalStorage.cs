using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShelterVault.DataLayer
{
    public abstract class SQLite3LocalStorage
    {
        public string DefaultShelterVaultPath => Path.Combine(UserPath, ".sheltervault");
        public string UserPath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public string DbName { get; private set; }
        public string DbPath => Path.Combine(DefaultShelterVaultPath, string.Concat(DbName, ".db"));
        public string DbConnectionString => $"Data Source={DbPath}";

        protected virtual void SetDbName(string dbName)
        {
            DbName = dbName;
        }

        protected virtual void ExecuteQueries(string[] queries)
        {
            //sqliteConnection.Query(query, param);
        }
    }
}
