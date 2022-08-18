using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using VSSystem.Data.AWS.Models.SQS;
using VSSystem.Data.AWS.Extensions;

namespace VSSystem.Data.AWS
{
    public class SQSClient
    {
        const int MAX_MESSAGE_SIZE = 256 * 1024;
        static Dictionary<string, string> _queueAttributes;
        static Dictionary<string, string> _largeMessageQueueMapping;
        static void _InitQueueAttribute(bool isFIFO)
        {
            if (_queueAttributes == null)
            {
                _queueAttributes = new Dictionary<string, string>
                {
                    {
                        QueueAttributeName.DelaySeconds,
                        TimeSpan.FromSeconds(5).TotalSeconds.ToString()
                    },
                    {
                        QueueAttributeName.MaximumMessageSize,
                        MAX_MESSAGE_SIZE.ToString()
                    },
                };
                if (isFIFO)
                {
                    _queueAttributes[QueueAttributeName.FifoQueue] = "True";
                }
            }
        }
        static public async Task<string> CreateQueueAsync(string queueName, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                var client = AWSClientExtension.CreateSQSClient();
                if (client != null)
                {
                    bool isFIFO = queueName.EndsWith(".fifo", StringComparison.InvariantCultureIgnoreCase);
                    var requestObj = new CreateQueueRequest(queueName);

                    _InitQueueAttribute(isFIFO);
                    requestObj.Attributes = _queueAttributes;

                    var responseObj = await client.CreateQueueAsync(requestObj, cancellationToken);

                    if (responseObj?.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        if (_largeMessageQueueMapping == null)
                        {
                            _largeMessageQueueMapping = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                        }
                        result = responseObj.QueueUrl;
                        _largeMessageQueueMapping[result] = "s3-" + queueName;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return result;
        }

        static public Task<bool> SendMessageAsync(string queueUrl, object sentObj, CancellationToken cancellationToken = default)
        {
            try
            {
                string messageBody = JsonConvert.SerializeObject(sentObj);
                return SendMessageAsync(queueUrl, messageBody, cancellationToken);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        static public async Task<bool> SendMessageAsync(string queueUrl, string messageBody, CancellationToken cancellationToken = default)
        {
            try
            {
                if (messageBody.Length < MAX_MESSAGE_SIZE)
                {
                    var client = AWSClientExtension.CreateSQSClient();
                    if (client != null)
                    {
                        string messageGuid = Guid.NewGuid().ToString("N").ToLower();
                        SendMessageRequest requestObj = new SendMessageRequest(queueUrl, messageBody)
                        {
                            MessageGroupId = messageGuid,
                            MessageDeduplicationId = messageGuid
                        };
                        var responseObj = await client.SendMessageAsync(requestObj, cancellationToken);

                        if (responseObj?.HttpStatusCode == System.Net.HttpStatusCode.OK)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (_largeMessageQueueMapping?.ContainsKey(queueUrl) ?? false)
                    {
                        string bucketName = _largeMessageQueueMapping[queueUrl];
                        if (!string.IsNullOrWhiteSpace(bucketName))
                        {
                            var objectKey = await S3Client.SendSQSMessageAsync(bucketName, messageBody, cancellationToken);
                            var newMessageObj = new MessageInfo
                            {
                                ObjectKey = objectKey,
                                BucketName = bucketName
                            };
                            return await SendMessageAsync(queueUrl, newMessageObj, cancellationToken);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return false;
        }

        async public static Task<string> ReceiveMessageAsync(string queueUrl, bool deleteMessage = true, CancellationToken cancellationToken = default)
        {
            string result = string.Empty;
            try
            {
                var client = AWSClientExtension.CreateSQSClient();
                if (client != null)
                {
                    if (!string.IsNullOrWhiteSpace(queueUrl))
                    {
                        var responseObj = await client.ReceiveMessageAsync(queueUrl, cancellationToken);

                        if (responseObj?.Messages?.Count > 0)
                        {
                            var messageObj = responseObj.Messages[0];
                            string messageBody = messageObj.Body;
                            var messageInfoObj = JsonConvert.DeserializeObject<MessageInfo>(messageBody);
                            if (!string.IsNullOrWhiteSpace(messageInfoObj?.ObjectKey) && !string.IsNullOrWhiteSpace(messageInfoObj?.BucketName))
                            {
                                messageBody = await S3Client.ReceiveSQSMessageAsync(messageInfoObj.BucketName, messageInfoObj.ObjectKey, deleteMessage, cancellationToken);
                            }
                            if (deleteMessage)
                            {
                                _ = client.DeleteMessageAsync(queueUrl, messageObj.ReceiptHandle, cancellationToken);
                            }
                            result = messageBody;
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

        async public static Task<TResult> ReceiveObjectAsync<TResult>(string queueUrl, bool deleteMessage = true, CancellationToken cancellationToken = default)
        {
            try
            {
                var messageBody = await ReceiveMessageAsync(queueUrl, deleteMessage, cancellationToken);
                var result = JsonConvert.DeserializeObject<TResult>(messageBody);
                return result;
            }
            catch { }
            return default;
        }
    }
}