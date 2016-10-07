using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace WF.Upgrade.Framework
{
    internal static class DbHelper
    {
        internal static int ExecuteNonQuery(SqlCommand command)
        {
            using (ConnectionScope scope = new ConnectionScope())
            {
                return scope.Current.ExecuteCommand<int>(
                    command,
                    cmd => cmd.ExecuteNonQuery()
                    );
            }
        }

        internal static DataTable ToDataTable(SqlCommand command)
        {
            using (ConnectionScope scope = new ConnectionScope())
            {
                return scope.Current.ExecuteCommand<DataTable>(
                    command,
                    cmd =>
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {

                            //fix bug,必须要创建一个DataSet并把EnforceConstraints设置为False
                            //否则在select * 连接查询时,包含两个时间戳字段将导致默认推断主键.
                            //记录重复后会引发[System.Data.ConstraintException] = {"未能启用约束。一行或多行中包含违反非空、唯一或外键约束的值。"}异常

                            DataSet ds = new DataSet();
                            ds.EnforceConstraints = false;

                            DataTable table = new DataTable("_tb");
                            ds.Tables.Add(table);

                            table.Load(reader);
                            return table;
                        }
                    }
                    );
            }
        }


        internal static DataSet ToDataSet(SqlCommand command)
        {
            using (ConnectionScope scope = new ConnectionScope())
            {
                return scope.Current.ExecuteCommand<DataSet>(
                    command,
                    cmd =>
                    {
                        DataSet ds = new DataSet();
                        SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                        adapter.Fill(ds);
                        for (int i = 0; i < ds.Tables.Count; i++)
                        {
                            ds.Tables[i].TableName = "_tb" + i.ToString();
                        }
                        return ds;
                    }
                    );
            }
        }

        internal static T ExecuteScalar<T>(SqlCommand command)
        {
            using (ConnectionScope scope = new ConnectionScope())
            {
                return scope.Current.ExecuteCommand<T>(
                    command,
                    cmd => ConvertScalar<T>(cmd.ExecuteScalar())
                );
            }
        }

        internal static T ConvertScalar<T>(object obj)
        {
            if (obj == null || DBNull.Value.Equals(obj))
                return default(T);

            if (obj is T)
                return (T)obj;

            Type targetType = typeof(T);

            if (targetType == typeof(object))
                return (T)obj;

            return (T)Convert.ChangeType(obj, targetType);
        }

        internal static List<T> ToList<T>(SqlCommand cmd) where T:class,new()
        {
            using (ConnectionScope scope = new ConnectionScope())
            {
                return scope.Current.ExecuteCommand<List<T>>(cmd, p =>
                {
                    using (SqlDataReader reader = p.ExecuteReader())
                    {
                        return ToList<T>(reader);
                    }
                });
            }
        }

        internal static List<T> ToList<T>(SqlDataReader reader) where T : class,new()
        {
            Type type = typeof(T);
            List<T> list = new List<T>();

            string[] columnNames = GetColumnNames(reader);
            while (reader.Read())
            {
                T entity = Activator.CreateInstance(type) as T;
                PropertyInfo[] pArray = type.GetProperties();
                for (int i = 0; i < columnNames.Length; i++)
                {
                    string key = columnNames[i];
                    object val = reader.GetValue(i);
                    PropertyInfo propertyInfo = GetPropertyInfo(key, pArray);
                    if (val != null && DBNull.Value.Equals(val) == false && propertyInfo!=null)
                    {
                        propertyInfo.SetValue(entity, ConvertType(val, propertyInfo.PropertyType), null);
                    }
                    
                }
                list.Add(entity);
            }
            return list;
        }
        internal static List<T> ToList<T>(DataTable dt) where T : class,new()
        {
            Type type = typeof(T);
            List<T> list = new List<T>();

            foreach (DataRow row in dt.Rows)
            {
                PropertyInfo[] pArray = type.GetProperties();
                T entity = new T();
                foreach (PropertyInfo p in pArray)
                {
                    if (row[p.Name] is Int64)
                    {
                        p.SetValue(entity, Convert.ToInt32(row[p.Name]), null);
                        continue;
                    }
                    p.SetValue(entity, row[p.Name], null);
                }
                list.Add(entity);
            }
            return list;
        }

        internal static string[] GetColumnNames(SqlDataReader reader)
        {
            int count = reader.FieldCount;
            string[] names = new string[count];
            for (int i = 0; i < count; i++)
            {
                names[i] = reader.GetName(i);
            }
            return names;
        }

        internal static object ConvertType(this object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }
            if (targetType == typeof(string))
            {
                return value.ToString();
            }
            Type conversionType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (value.GetType() == conversionType)
            {
                return value;
            }
            if ((conversionType == typeof(Guid)) && (value.GetType() == typeof(string)))
            {
                return new Guid(value.ToString());
            }
            return System.Convert.ChangeType(value, conversionType);
        }
        internal static PropertyInfo GetPropertyInfo(string Name, PropertyInfo[] pArray)
        { 
            foreach(var propertyInfo in pArray){
                if (propertyInfo.Name == Name) {
                    return propertyInfo;
                }
            }
            return null;
     
        }
    }
}
