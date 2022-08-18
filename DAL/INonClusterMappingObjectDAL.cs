using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VSSystem.Data.File.DTO;
using VSSystem.Data.AWS.DTO;
using VSSystem.Data.AWS.Extensions;

namespace VSSystem.Data.AWS.DAL
{
    public class INonClusterMappingObjectDAL<TDTO> : File.DAL.INonClusterMappingObjectDAL<TDTO>
         where TDTO : MappingObjectDTO
    {
        public INonClusterMappingObjectDAL(string tableName)
           : base(tableName, "Image_ID", DatabaseClientExtension.SqlPoolProcess)
        {
        }
        public INonClusterMappingObjectDAL(string tableName, SqlPoolProcess sqlPoolProcess)
            : base(tableName, "Image_ID", sqlPoolProcess)
        {
        }

        public DataObjectUrlDTO GetObjectUrl(long id, string awsTableName)
        {

            try
            {
                string query = string.Format("select m.ID, aws.PreSignedUrl as `Url` from {0} m left join {1} aws on aws.ID = m.Base_ID where m.Image_ID = {2}", _TableName, awsTableName, id);
                var dbObjs = ExecuteReader<DataObjectUrlDTO>(query);
                return dbObjs.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
