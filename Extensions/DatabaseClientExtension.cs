using System;
namespace VSSystem.Data.AWS.Extensions
{
    class DatabaseClientExtension
    {
        static VSSystem.Data.EDbProvider _Provider;
        public static VSSystem.Data.EDbProvider Provider { get { return _Provider; } }
        public static VSSystem.Data.SqlPoolProcess SqlPoolProcess;
        static object _initializeLockObj;
        public static void Initialize(string server, string username, string password, int port, string database, string driver, int commandTimeout = 120, int numberOfConnections = 1)
        {
            if (_initializeLockObj == null)
            {
                _initializeLockObj = new object();
            }
            lock (_initializeLockObj)
            {
                SqlPoolProcess = Data.Variables.GetSqlProcess(server, username, password, port, database, driver, commandTimeout, numberOfConnections);
                _Provider = SqlPoolProcess.Provider;

                VSSystem.Data.File.Variables.Init(server, username, password, port, database, driver, commandTimeout, numberOfConnections);
            }
        }
        public static void Initialize(Func<string, string, string, string> readIniValueFunc, Func<string, string> descryptFunc, string databaseConfigSection = "database_info")
        {
            if (_initializeLockObj == null)
            {
                _initializeLockObj = new object();
            }
            lock (_initializeLockObj)
            {
                if (SqlPoolProcess == null)
                {
                    SqlPoolProcess = Data.Variables.GetSqlProcessFromIniFile(readIniValueFunc, descryptFunc, databaseConfigSection);
                    _Provider = SqlPoolProcess.Provider;
                }
                VSSystem.Data.File.Variables.InitFromIniFile(readIniValueFunc, descryptFunc, databaseConfigSection);
            }
        }
    }
}