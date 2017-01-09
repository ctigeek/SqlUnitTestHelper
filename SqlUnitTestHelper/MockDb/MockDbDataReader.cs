using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;

namespace SqlUnitTestHelper.MockDb
{
    public class MockDbDataReader : Mock<DbDataReader>
    {
        public virtual string[] ColumnNames { get; set; }
        public virtual List<object[]> DataValues { get; set; }
        private int dataReaderRowNumber = -1;

        public virtual int DataReaderRowNumber
        {
            get { return dataReaderRowNumber < 0 ? 0 : dataReaderRowNumber; }
            set { dataReaderRowNumber = value; }
        }

        public MockDbDataReader(string[] columnNames, List<object[]> dataValues )
        {
            CallBase = true;
            SetupAllProperties();
            ColumnNames = columnNames;
            DataValues = dataValues;

            Setup(d => d.GetName(It.IsAny<int>()))
                .Returns<int>(i => ColumnNames[i]);
            Setup(d => d.GetValues(It.IsAny<object[]>()))
                .Throws(new NotImplementedException());
            Setup(d => d.IsDBNull(It.IsAny<int>()))
                .Returns<int>(i => DataValues[DataReaderRowNumber][i] == DBNull.Value);
            SetupGet(d => d.FieldCount).Returns(ColumnNames.Length);
            Setup(d => d[It.IsAny<string>()])
                .Returns<string>(s => DataValues[DataReaderRowNumber][GetOrdinalWrapper(s)]);
            Setup(d => d[It.IsAny<int>()])
                .Returns<int>(i => DataValues[DataReaderRowNumber][i]);
            SetupGet(d => d.HasRows).Returns(DataValues.Count > 0);
            SetupGet(d => d.IsClosed).Returns(false);
            SetupGet(d => d.RecordsAffected).Returns(DataValues.Count);
            Setup(d => d.NextResult()).Returns(false);
            Setup(d => d.Read())
                .Returns(() =>
                {
                    dataReaderRowNumber++;
                    return DataReaderRowNumber < DataValues.Count;
                });
            Setup(d => d.ReadAsync(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    dataReaderRowNumber++;
                    return Task.FromResult(DataReaderRowNumber < DataValues.Count);
                });
            SetupGet(d => d.Depth).Returns(1);
            Setup(d => d.GetOrdinal(It.IsAny<string>()))
                .Returns<string>(GetOrdinalWrapper);
            Setup(d => d.GetBoolean(It.IsAny<int>()))
                .Returns<int>(i => (bool)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetByte(It.IsAny<int>()))
                .Returns<int>(i => (byte)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetChar(It.IsAny<int>()))
                .Returns<int>(i => (char)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetGuid(It.IsAny<int>()))
                .Returns<int>(i => (Guid)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetInt16(It.IsAny<int>()))
                .Returns<int>(i => (short)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetInt32(It.IsAny<int>()))
                .Returns<int>(i => (int)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetInt64(It.IsAny<int>()))
                .Returns<int>(i => (long)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetDateTime(It.IsAny<int>()))
                .Returns<int>(i => (DateTime)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetString(It.IsAny<int>()))
                .Returns<int>(i => (string)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetDecimal(It.IsAny<int>()))
                .Returns<int>(i => (decimal)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetDouble(It.IsAny<int>()))
                .Returns<int>(i => (double)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetFloat(It.IsAny<int>()))
                .Returns<int>(i => (float)DataValues[DataReaderRowNumber][i]);
            Setup(d => d.GetDataTypeName(It.IsAny<int>()))
                .Returns<int>(i => DataValues[DataReaderRowNumber][i].GetType().ToString());
            Setup(d => d.GetFieldType(It.IsAny<int>()))
                .Returns<int>(i => DataValues[DataReaderRowNumber][i].GetType());
            Setup(d => d.GetValue(It.IsAny<int>()))
                .Returns<int>(i => DataValues[DataReaderRowNumber][i]);
        }

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
