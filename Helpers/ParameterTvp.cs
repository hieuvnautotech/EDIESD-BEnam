using Dapper;
using Microsoft.SqlServer.Server;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ESD_EDI_BE.Helpers
{
    public static class ParameterTvp
    {
        //public static DataTable ToDataTable<T>(IEnumerable<T> items)
        //{
        //    var tb = new DataTable(typeof(T).Name);

        //    PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        //    foreach (var prop in props)
        //    {
        //        tb.Columns.Add(prop.Name, prop.PropertyType);
        //    }

        //    foreach (var item in items)
        //    {
        //        var values = new object[props.Length];
        //        for (var i = 0; i < props.Length; i++)
        //        {
        //            values[i] = props[i].GetValue(item, null);
        //        }

        //        tb.Rows.Add(values);
        //    }

        //    return tb;
        //}

        private static DataTable ConvertToDataTable<T>(IEnumerable<T> objects)
        {
            DataTable dataTable = new DataTable();

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                dataTable.Columns.Add(property.Name, Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType);
            }

            foreach (T obj in objects)
            {
                DataRow dataRow = dataTable.NewRow();

                foreach (PropertyInfo property in properties)
                {
                    object value = property.GetValue(obj);
                    if (value is string stringValue)
                    {
                        value = stringValue.Trim();
                    }
                    dataRow[property.Name] = value ?? DBNull.Value;
                }

                dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        private static IEnumerable<SqlDataRecord> CreateIntDataRecord(IEnumerable<int> list)
        {
            var metaData = new SqlMetaData("Value", SqlDbType.Int);
            var record = new SqlDataRecord(metaData);
            foreach (var item in list)
            {
                record.SetInt32(0, item);
                yield return record;
            }
        }

        private static IEnumerable<SqlDataRecord> CreateBigIntDataRecord(IEnumerable<long> list)
        {
            var metaData = new SqlMetaData("Value", SqlDbType.BigInt);
            var record = new SqlDataRecord(metaData);
            foreach (var item in list)
            {
                record.SetInt64(0, item);
                yield return record;
            }
        }

        private static IEnumerable<SqlDataRecord> CreateNVarcharDataRecord(IEnumerable<string> list)
        {
            var metaData = new SqlMetaData("Value", SqlDbType.NVarChar, SqlMetaData.Max);
            var record = new SqlDataRecord(metaData);
            foreach (var item in list)
            {
                record.SetString(0, item);
                yield return record;
            }
        }
        private static IEnumerable<SqlDataRecord> CreateDecimalDataRecord(IEnumerable<decimal> list)
        {
            var metaData = new SqlMetaData("Value", SqlDbType.Decimal, 18,0);
            var record = new SqlDataRecord(metaData);
            foreach (var item in list)
            {
                record.SetDecimal(0, item);
                yield return record;
            }
        }
        public static SqlMapper.ICustomQueryParameter GetTableValuedParameter_Int(IEnumerable<int> list)
        {
            return CreateIntDataRecord(list).AsTableValuedParameter("TVP_Int");
        }

        public static SqlMapper.ICustomQueryParameter GetTableValuedParameter_BigInt(IEnumerable<long> list)
        {
            return CreateBigIntDataRecord(list).AsTableValuedParameter("TVP_BigInt");
        }

        public static SqlMapper.ICustomQueryParameter GetTableValuedParameter_NVarchar(IEnumerable<string> list)
        {
            return CreateNVarcharDataRecord(list).AsTableValuedParameter("TVP_NVarchar");
        }
        public static SqlMapper.ICustomQueryParameter GetTableValuedParameter_Decimal(IEnumerable<decimal> list)
        {
            return CreateDecimalDataRecord(list).AsTableValuedParameter("TVP_Numeric");
        }
        public static SqlMapper.ICustomQueryParameter GetTableValuedParameter<T>(IEnumerable<T> objects, string parameterName)
        {
            DataTable dataTable = ConvertToDataTable(objects);
            SqlMapper.ICustomQueryParameter tableValuedParameter = dataTable.AsTableValuedParameter(parameterName);

            return tableValuedParameter;
        }
    }
}
