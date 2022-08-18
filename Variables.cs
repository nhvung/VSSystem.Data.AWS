using System;
using System.Collections.Generic;
using VSSystem.Data.AWS.Extensions;

namespace VSSystem.Data.AWS
{
    public class Variables
    {
        public static void Initialize(Func<string, string, string, string> readIniValueFunc, Func<string, string> descryptFunc
        , string databaseConfigSection = "database_info", string awsAuthenticationConfigSection = "aws_auth")
        {
            DatabaseClientExtension.Initialize(readIniValueFunc, descryptFunc, databaseConfigSection);
            AWSClientExtension.Initialize(readIniValueFunc, descryptFunc, awsAuthenticationConfigSection);
        }
        public static void InitializeDatabase(string server, string username, string password, int port, string database, string driver, int commandTimeout = 120, int numberOfConnections = 1)
        {
            DatabaseClientExtension.Initialize(server, username, password, port, database, driver, commandTimeout, numberOfConnections);
        }
        public static void InitializeAWSAuthentication(string accessKey, string secretAccessKey, string regionName = "")
        {
            AWSClientExtension.Initialize(accessKey, secretAccessKey, regionName);
        }
        public static bool AWSAuthenicationInitialized
        {
            get
            {
                return AWSClientExtension.AuthenicationInitialized;
            }
        }
    }
}
