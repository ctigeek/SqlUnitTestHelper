using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SqlUnitTestHelper.MockDb
{
    public abstract class DbDataReaderWrapper : DbDataReader
    {
        public virtual string[] ColumnNames { get; set; }
        public virtual List<object[]> DataValues { get; set; }
        public virtual int DataReaderRowNumber { get; set; }

        public virtual int GetOrdinalWrapper(string name)
        {
            for (int i = 0; i < ColumnNames.Length; i++)
            {
                if (ColumnNames[i] == name)
                    return i;
            }
            return -1;
        }
    }
}