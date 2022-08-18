using Amazon;
using System;
namespace VSSystem.Data.AWS.Extensions
{
    class AWSClientExtension
    {
        static string _AccessKey, _SecretAccessKey;
        static Amazon.RegionEndpoint _Region;
        static public Amazon.RegionEndpoint Region { get { return _Region; } }
        static bool _AuthenicationInitialized;
        public static bool AuthenicationInitialized { get { return _AuthenicationInitialized; } }
        static object _initializeLockObj;
        public static void Initialize(string accessKey, string secretAccessKey, string regionName = "")
        {
            if (_initializeLockObj == null)
            {
                _initializeLockObj = new object();
            }
            lock (_initializeLockObj)
            {
                try
                {
                    _AccessKey = accessKey;
                    _SecretAccessKey = secretAccessKey;
                    _AuthenicationInitialized = !string.IsNullOrWhiteSpace(_AccessKey) && !string.IsNullOrWhiteSpace(_SecretAccessKey);
                    _Region = RegionEndpoint.USEast1;
                    if (!string.IsNullOrWhiteSpace(regionName))
                    {
                        foreach (var regionObj in Amazon.RegionEndpoint.EnumerableAllRegions)
                        {
                            if (regionObj.SystemName.Equals(regionName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                _Region = regionObj;
                                break;
                            }
                        }
                    }
                }
                catch { }
            }

        }
        public static void Initialize(Func<string, string, string, string> readIniValueFunc, Func<string, string> descryptFunc, string awsAuthenticationConfigSection = "aws_auth")
        {
            if (_initializeLockObj == null)
            {
                _initializeLockObj = new object();
            }
            lock (_initializeLockObj)
            {
                try
                {
                    _AccessKey = readIniValueFunc?.Invoke(awsAuthenticationConfigSection, "access_key", "");
                    _SecretAccessKey = readIniValueFunc?.Invoke(awsAuthenticationConfigSection, "secret_access_key", "");
                    _SecretAccessKey = descryptFunc?.Invoke(_SecretAccessKey);
                    _AuthenicationInitialized = !string.IsNullOrWhiteSpace(_AccessKey) && !string.IsNullOrWhiteSpace(_SecretAccessKey);
                    string regionName = readIniValueFunc?.Invoke(awsAuthenticationConfigSection, "region", "us-east-1");
                    _Region = RegionEndpoint.USEast1;
                    if (!string.IsNullOrWhiteSpace(regionName))
                    {
                        foreach (var regionObj in Amazon.RegionEndpoint.EnumerableAllRegions)
                        {
                            if (regionObj.SystemName.Equals(regionName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                _Region = regionObj;
                                break;
                            }
                        }
                    }

                }
                catch { }
            }
        }
        public static Amazon.S3.AmazonS3Client CreateS3Client()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_AccessKey) && !string.IsNullOrWhiteSpace(_SecretAccessKey))
                {
                    Amazon.S3.AmazonS3Client client = new Amazon.S3.AmazonS3Client(_AccessKey, _SecretAccessKey, _Region);
                    return client;
                }
            }
            catch { }
            return null;
        }
        public static Amazon.SQS.AmazonSQSClient CreateSQSClient()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_AccessKey) && !string.IsNullOrWhiteSpace(_SecretAccessKey))
                {
                    Amazon.SQS.AmazonSQSClient client = new Amazon.SQS.AmazonSQSClient(_AccessKey, _SecretAccessKey, _Region);
                    return client;
                }
            }
            catch { }
            return null;
        }
    }
}