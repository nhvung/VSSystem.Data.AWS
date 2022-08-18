using System;
using System.Collections.Generic;
using System.Text;

namespace VSSystem.Data.AWS.DTO
{
    public class DataObjectUrlDTO : Data.DTO.DataDTO
    {

        long _ID;
        public long ID { get { return _ID; } set { _ID = value; } }

        string _Url;
        public string Url { get { return _Url; } set { _Url = value; } }

    }
}
