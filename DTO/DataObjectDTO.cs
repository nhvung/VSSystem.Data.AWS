namespace VSSystem.Data.AWS.DTO
{
    public class DataObjectDTO : Data.DTO.DataDTO
    {
        byte[] _Sha1;
        public byte[] Sha1 { get { return _Sha1; } set { _Sha1 = value; } }
        long _ID;
        public long ID { get { return _ID; } set { _ID = value; } }
        string _ObjectKey;
        public string ObjectKey { get { return _ObjectKey; } set { _ObjectKey = value; } }
        string _BucketName;
        public string BucketName { get { return _BucketName; } set { _BucketName = value; } }
        string _ContentType;
        public string ContentType { get { return _ContentType; } set { _ContentType = value; } }
        long _DataLength;
        public long DataLength { get { return _DataLength; } set { _DataLength = value; } }
        string _PreSignedUrl;
        public string PreSignedUrl { get { return _PreSignedUrl; } set { _PreSignedUrl = value; } }
        long _CreatedDateTime;
        public long CreatedDateTime { get { return _CreatedDateTime; } set { _CreatedDateTime = value; } }
        double _UploadTime;
        public double UploadTime { get { return _UploadTime; } set { _UploadTime = value; } }
    }
}
