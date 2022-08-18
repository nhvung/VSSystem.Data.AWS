using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using VSSystem.Data.AWS.Extensions;

namespace VSSystem.Data.AWS
{
    public class S3Client
    {
        const int DEFAULT_BUFFER_SIZE = 10 * 1024 * 1024; // 10 Mb

        async public static Task<bool> CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    PutBucketRequest requestObj = new PutBucketRequest()
                    {
                        BucketName = bucketName,
                    };

                    var responseObj = await client.PutBucketAsync(requestObj, cancellationToken);

                    if (responseObj?.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        async public static Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    DeleteBucketRequest request = new DeleteBucketRequest();
                    request.BucketName = bucketName;
                    var responseObj = await client.DeleteBucketAsync(request, cancellationToken);
                    if (responseObj?.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        async public static Task<string> PutObjectAsync(string bucketName, string objectKey, Stream input, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    if (string.IsNullOrWhiteSpace(objectKey))
                    {
                        objectKey = Guid.NewGuid().ToString().ToLower();
                    }
                    DateTime putBeginTime = DateTime.Now;
                    PutObjectRequest requestObj = new PutObjectRequest();
                    requestObj.InputStream = input;
                    requestObj.ContentType = contentType;
                    requestObj.BucketName = bucketName;
                    requestObj.Key = objectKey;
                    requestObj.BucketKeyEnabled = true;
                    requestObj.CannedACL = S3CannedACL.AuthenticatedRead;
                    if (isPublic)
                    {
                        requestObj.CannedACL = S3CannedACL.PublicRead;
                    }
                    requestObj.StorageClass = S3StorageClass.Standard;
                    requestObj.AutoCloseStream = false;
                    requestObj.AutoResetStreamPosition = true;

                    DateTime putEndTime = DateTime.Now;
                RETRY:
                    try
                    {
                        var responseObj = await client.PutObjectAsync(requestObj, cancellationToken);
                        if (responseObj.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            result = objectKey;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("The specified bucket does not exist", StringComparison.InvariantCultureIgnoreCase) >= 0)
                        {
                            var createBucketResult = await CreateBucketAsync(bucketName, cancellationToken);
                            if (createBucketResult)
                            {
                                goto RETRY;
                            }
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<string> PutObjectAsync(string bucketName, string objectKey, byte[] input, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                using (var stream = new MemoryStream(input))
                {
                    try
                    {
                        result = await PutObjectAsync(bucketName, objectKey, stream, contentType, isPublic, cancellationToken);
                    }
                    finally
                    {
                        stream.Close();
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }

        async public static Task<string> PutObjectAsync(string bucketName, string objectKey, string input, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                byte[] objectBytes = Encoding.UTF8.GetBytes(input);
                result = await PutObjectAsync(bucketName, objectKey, objectBytes, contentType, isPublic, cancellationToken);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<string> PutObjectAsync(string bucketName, string objectKey, object input, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                byte[] objectBytes = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(input));
                result = await PutObjectAsync(bucketName, objectKey, objectBytes, contentType, isPublic, cancellationToken);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<bool> DeleteObjectAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    DateTime putBeginTime = DateTime.Now;
                    DeleteObjectRequest requestObj = new DeleteObjectRequest();
                    requestObj.BucketName = bucketName;
                    requestObj.Key = objectKey;
                    DateTime putEndTime = DateTime.Now;

                    var responseObj = await client.DeleteObjectAsync(requestObj, cancellationToken);
                    if (responseObj.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        async public static Task<string> GetStringAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    GetObjectRequest requestObj = new GetObjectRequest();
                    requestObj.BucketName = bucketName;
                    requestObj.Key = objectKey;
                    var responseObj = await client.GetObjectAsync(bucketName, objectKey, cancellationToken);
                    if (responseObj != null)
                    {
                        if (responseObj.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            if (responseObj.ContentLength <= int.MaxValue)
                            {
                                using (var sr = new StreamReader(responseObj.ResponseStream, Encoding.UTF8))
                                {
                                    result = await sr.ReadToEndAsync();
                                    sr.Close();
                                    sr.Dispose();
                                }
                            }
                            else
                            {
                                throw new Exception("Not enough memory. Please try WriteToFileAsync method.");
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<Stream> GetStreamAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            Stream result = new Models.MemoryStream64();
            try
            {

                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    GetObjectRequest requestObj = new GetObjectRequest();
                    requestObj.BucketName = bucketName;
                    requestObj.Key = objectKey;
                    var responseObj = await client.GetObjectAsync(bucketName, objectKey, cancellationToken);
                    if (responseObj != null)
                    {
                        if (responseObj.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            if (responseObj.ContentLength <= int.MaxValue)
                            {
                                await responseObj.ResponseStream.CopyToAsync(result);
                            }
                            else
                            {
                                throw new Exception("Not enough memory. Please try WriteToFileAsync method.");
                            }
                            result.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<byte[]> GetBytesAsync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            byte[] result = null;
            try
            {
                var stream = new MemoryStream();
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    GetObjectRequest requestObj = new GetObjectRequest();
                    requestObj.BucketName = bucketName;
                    requestObj.Key = objectKey;
                    var responseObj = await client.GetObjectAsync(bucketName, objectKey, cancellationToken);
                    if (responseObj != null)
                    {
                        if (responseObj.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            if (responseObj.ContentLength <= int.MaxValue)
                            {
                                await responseObj.ResponseStream.CopyToAsync(stream);
                                result = stream.ToArray();
                            }
                            else
                            {
                                throw new Exception("Not enough memory. Please try WriteToFileAsync method.");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<string> WriteToFileAsync(string bucketName, string objectKey, string filePath, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                var client = AWSClientExtension.CreateS3Client();
                if (client != null)
                {
                    GetObjectRequest requestObj = new GetObjectRequest();
                    requestObj.BucketName = bucketName;
                    requestObj.Key = objectKey;
                    var responseObj = await client.GetObjectAsync(bucketName, objectKey, cancellationToken);
                    if (responseObj != null)
                    {
                        if (responseObj.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            FileInfo file = new FileInfo(filePath);
                            if (!file.Directory.Exists)
                            {
                                file.Directory.Create();
                            }
                            using (var fs = file.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                            {
                                await responseObj.ResponseStream.CopyToAsync(fs, DEFAULT_BUFFER_SIZE, cancellationToken);
                                fs.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return result;
        }
        async public static Task<string> SendSQSMessageAsync(string bucketName, string messageBody, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
            RETRY:
                try
                {
                    string objectKey = Guid.NewGuid().ToString().ToLower();
                    byte[] objectBytes = Encoding.UTF8.GetBytes(messageBody);
                    result = await PutObjectAsync(bucketName, objectKey, objectBytes, "application/json", false, cancellationToken);
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("The specified bucket does not exist", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        var createBucketResult = await CreateBucketAsync(bucketName, cancellationToken);
                        if (createBucketResult)
                        {
                            goto RETRY;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }
        async public static Task<string> ReceiveSQSMessageAsync(string bucketName, string objectKey, bool deleteMessage = true, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                result = await GetStringAsync(bucketName, objectKey, cancellationToken);
                if (deleteMessage)
                {
                    _ = DeleteObjectAsync(bucketName, objectKey, cancellationToken);
                }
            }
            catch { }
            return result;
        }

    }
}