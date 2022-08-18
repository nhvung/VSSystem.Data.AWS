using System;
using System.Collections.Generic;
using System.Text;
using VSSystem.Data.BLL;
using VSSystem.Data.File.DTO;
using VSSystem.Data.AWS.DAL;
using VSSystem.Data.AWS.DTO;

namespace VSSystem.Data.AWS.BLL
{
    public class NonClusterMappingObjectBLL<TDAL, TDTO> : File.BLL.NonClusteredMappingObjectBLL<TDAL, TDTO>
         where TDAL : INonClusterMappingObjectDAL<TDTO>
         where TDTO : MappingObjectDTO
    {
        static public DataObjectUrlDTO GetObjectUrl(string tableName, long id, string awsTableName)
        {
            return GetDAL(tableName).GetObjectUrl(id, awsTableName);
        }
    }
    public class NonClusterMappingObjectBLL<TMappingObjectDAL> : NonClusterMappingObjectBLL<TMappingObjectDAL, MappingObjectDTO>
        where TMappingObjectDAL : INonClusterMappingObjectDAL<MappingObjectDTO>
    {

    }
    public class NonClusterMappingObjectBLL : NonClusterMappingObjectBLL<INonClusterMappingObjectDAL<MappingObjectDTO>>
    {

    }
}
