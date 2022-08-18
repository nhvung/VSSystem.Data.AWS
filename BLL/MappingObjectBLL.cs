using System;
using System.Collections.Generic;
using System.Text;
using VSSystem.Data.BLL;
using VSSystem.Data.File.DTO;
using VSSystem.Data.AWS.DAL;
using VSSystem.Data.AWS.DTO;

namespace VSSystem.Data.AWS.BLL
{
    public class MappingObjectBLL<TDAL, TDTO> : File.BLL.MappingObjectBLL<TDAL, TDTO>
         where TDAL : IMappingObjectDAL<TDTO>
         where TDTO : MappingObjectDTO
    {
        static public DataObjectUrlDTO GetObjectUrl(string tableName, long id, string awsTableName)
        {
            return GetDAL(tableName).GetObjectUrl(id, awsTableName);
        }
    }
    public class MappingObjectBLL<TMappingObjectDAL> : MappingObjectBLL<TMappingObjectDAL, MappingObjectDTO>
        where TMappingObjectDAL : IMappingObjectDAL<MappingObjectDTO>
    {

    }
    public class MappingObjectBLL : MappingObjectBLL<IMappingObjectDAL<MappingObjectDTO>>
    {

    }
}
