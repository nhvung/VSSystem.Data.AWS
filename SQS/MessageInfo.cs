namespace VSSystem.Data.AWS.Models.SQS
{
    class MessageInfo
    {
        string _BucketName;
        public string BucketName { get { return _BucketName; } set { _BucketName = value; } }
        string _ObjectKey;
        public string ObjectKey { get { return _ObjectKey; } set { _ObjectKey = value; } }

    }
}