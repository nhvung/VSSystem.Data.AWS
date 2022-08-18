using System;
using System.Collections.Generic;
using System.Text;

namespace VSSystem.Data.AWS
{
    public static class Generator
    {
        const int _MAPPING_LIMIT_COUNT = 10000;
        static Dictionary<long, bool> _mapping_int64_id;
        static DateTime _DT2010;
        public static long GenerateInt64ID()
        {
            try
            {
                if (_DT2010 == null || _DT2010 == DateTime.MinValue)
                {
                    _DT2010 = new DateTime(2010, 1, 1);
                }
                if (_mapping_int64_id == null)
                {
                    _mapping_int64_id = new Dictionary<long, bool>();
                }
                if (_mapping_int64_id.Count == _MAPPING_LIMIT_COUNT)
                {
                    _mapping_int64_id.Clear();
                }

                int randomNumber = new Random().Next(0, 99999);
                long result = (DateTime.UtcNow - _DT2010).Ticks * 10 + randomNumber;
                while (_mapping_int64_id.ContainsKey(result))
                {
                    System.Threading.Thread.Sleep(10);
                    result = (DateTime.UtcNow - _DT2010).Ticks * 10 + randomNumber;
                }
                _mapping_int64_id[result] = false;
                return result;
            }
            catch { }
            return 0;
        }
        public static long ToInt64(this DateTime dt)
        {
            if (dt > DateTime.MinValue)
            {
                long result = long.Parse(dt.ToString("yyyyMMddHHmmss"));
                return result;
            }
            return 0;
        }
    }
}
