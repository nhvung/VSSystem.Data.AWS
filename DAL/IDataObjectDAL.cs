using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSSystem.Data.DAL;
using VSSystem.Data.AWS.DTO;
using VSSystem.Data.AWS.Extensions;

namespace VSSystem.Data.AWS.DAL
{
    public class IDataObjectDAL<TDTO> : DataDAL<TDTO>
        where TDTO : DataObjectDTO
    {
        const string _CREATE_TABLE_STATEMENTS = "create table if not exists `{0}` ("
        + "`Sha1` binary(20) primary key, "
        + "`ObjectKey` varchar(50), "
        + "`ID` bigint, "
        + "`BucketName` varchar(255), "
        + "`ContentType` varchar(20), "
        + "`DataLength` bigint, "
        + "`PreSignedUrl` text, "
        + "`UploadTime` double, "
        + "`CreatedTicks` bigint, "
        + "unique index (`ID`),"
        + "unique index (`ObjectKey`)"
        + ");";
        public IDataObjectDAL(string tableName) : base(DatabaseClientExtension.SqlPoolProcess)
        {
            _TableName = tableName;
            _CreateTableStatements = _CREATE_TABLE_STATEMENTS;
            _AutoCreateTable = true;
        }
        public IDataObjectDAL(string tableName, SqlPoolProcess sqlProcess) : base(sqlProcess)
        {
            _TableName = tableName;
            _CreateTableStatements = _CREATE_TABLE_STATEMENTS;
            _AutoCreateTable = true;
        }
        public TDTO GetHashObject(byte[] sha1Key)
        {
            try
            {
                string query = string.Format("select * from {0} where Sha1 = 0x{1}", _TableName, BitConverter.ToString(sha1Key).Replace("-", ""));
                var dbObjs = ExecuteReader<TDTO>(query);
                return dbObjs?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
