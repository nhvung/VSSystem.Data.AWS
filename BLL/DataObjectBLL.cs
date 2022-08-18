using System;
using System.IO;
using VSSystem.Data.AWS.DAL;
using VSSystem.Data.AWS.DTO;
using VSSystem.Data.BLL;
using System.Threading;
using System.Threading.Tasks;
using VSSystem.Data.AWS.Extensions;
using Newtonsoft.Json;
using System.Text;

namespace VSSystem.Data.AWS.BLL
{
    public class DataObjectBLL<TDAL, TDTO> : DataBLL<TDAL, TDTO>
        where TDAL : IDataObjectDAL<TDTO>
        where TDTO : DataObjectDTO
    {
        // static object _lockObj;
        static TDTO GetHashObject(string tableName, byte[] sha1Key)
        {
            return GetDAL(tableName).GetHashObject(sha1Key);
        }

        public static async Task<TDTO> PutObjectAsync(string tableName, string objectKey, long id, string bucketName, byte[] input, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            TDTO pHashObj = null;
            try
            {
                using (var stream = new MemoryStream(input))
                {
                    pHashObj = await PutObjectAsync(tableName, objectKey, id, bucketName, stream, contentType, isPublic, cancellationToken);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pHashObj;
        }
        public static async Task<TDTO> PutObjectAsync(string tableName, string objectKey, long id, string bucketName, object input, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            TDTO pHashObj = null;
            try
            {
                string jsonObject = JsonConvert.SerializeObject(input);
                byte[] objectBytes = Encoding.UTF8.GetBytes(jsonObject);
                using (var stream = new MemoryStream(objectBytes))
                {
                    pHashObj = await PutObjectAsync(tableName, objectKey, id, bucketName, stream, contentType, isPublic, cancellationToken);
                    stream.Close();
                    stream.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return pHashObj;
        }
        public static async Task<TDTO> PutObjectAsync(string tableName, string objectKey, long id, string bucketName, Stream stream, string contentType, bool isPublic = false, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!AWSClientExtension.AuthenicationInitialized)
                {
                    throw new Exception("AWS Client not init");
                }
                DateTime utcNow = DateTime.UtcNow;
                long lUtcNow = utcNow.ToInt64();

                var sha1Key = stream.GetSha1Hash();

                TDTO pHashObj = null;

                pHashObj = GetHashObject(tableName, sha1Key);

            STEP1:
                if (pHashObj == null)
                {
                    if (id <= 0)
                    {
                        id = Generator.GenerateInt64ID();
                    }

                    pHashObj = Activator.CreateInstance<TDTO>();
                    pHashObj.Sha1 = sha1Key;
                    pHashObj.ID = id;
                    pHashObj.ContentType = contentType;
                    pHashObj.BucketName = bucketName;
                    pHashObj.CreatedDateTime = lUtcNow;
                    pHashObj.DataLength = stream.Length;

                    try
                    {
                        try
                        {
                            DateTime putBeginTime = DateTime.Now;
                            pHashObj.ObjectKey = await S3Client.PutObjectAsync(bucketName, objectKey, stream, contentType, isPublic, cancellationToken);
                            DateTime putEndTime = DateTime.Now;
                            if (!string.IsNullOrWhiteSpace(objectKey))
                            {
                                pHashObj.PreSignedUrl = $"https://{bucketName}.s3.{AWSClientExtension.Region.SystemName}.amazonaws.com/{objectKey}";
                            }
                            pHashObj.UploadTime = Convert.ToInt64(Math.Floor((putEndTime - putBeginTime).TotalMilliseconds));

                            try
                            {
                                Insert(tableName, pHashObj);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception("Insert pHashObj", ex);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message.IndexOf("Duplicate entry", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                Thread.Sleep(500);
                                pHashObj = GetHashObject(tableName, sha1Key);
                                if (pHashObj == null)
                                {
                                    goto STEP1;
                                }
                            }
                            else
                            {
                                throw new Exception("PutObjectAsync", ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error ID: " + id, ex);
                    }

                }
                return pHashObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }

    public class DataObjectBLL<TDAL> : DataObjectBLL<TDAL, DataObjectDTO>
        where TDAL : IDataObjectDAL<DataObjectDTO>
    {

    }
    public class DataObjectBLL : DataObjectBLL<IDataObjectDAL<DataObjectDTO>>
    {

    }
}
