using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SqlUnitTestHelper.MockDb
{
    public class DbParameterCollectionWrapper : DbParameterCollection
    {
        public List<DbParameter> Parameters { get; set; } = new List<DbParameter>();

        public override int Add(object value)
        {
            var parameter = value as DbParameter;
            if (parameter == null)
                throw new ArgumentNullException(nameof(value));
            Parameters.Add(parameter);
            return 1;
        }

        public override bool Contains(object value)
        {
            return Parameters.Contains((DbParameter)value);
        }

        public override void Clear()
        {
            Parameters.Clear();
        }

        public override int IndexOf(object value)
        {
            return Parameters.IndexOf((DbParameter)value);
        }

        public override int IndexOf(string parameterName)
        {
            var dbParameter = Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
            return IndexOf(dbParameter);
        }

        public override void Insert(int index, object value)
        {
            Parameters.Insert(index, (DbParameter)value);
        }

        public override void Remove(object value)
        {
            Parameters.Remove((DbParameter)value);
        }

        public override void RemoveAt(int index)
        {
            Parameters.RemoveAt(index);
        }

        public override void RemoveAt(string parameterName)
        {
            var dbParameter = Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
            Parameters.Remove(dbParameter);
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            Parameters[index] = value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            var index = IndexOf(parameterName);
            Parameters[index] = value;
        }

        public override int Count => Parameters.Count;

        public override object SyncRoot { get; }

        public override IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter GetParameter(int index)
        {
            return Parameters[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return Parameters.FirstOrDefault(p => p.ParameterName == parameterName);
        }

        public override bool Contains(string value)
        {
            return Parameters.Any(p => p.ParameterName == value);
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public override void AddRange(Array values)
        {
            throw new NotImplementedException();
        }
    }
}