using Sitecore.Data.SqlServer;
using System;
using System.Reflection;

namespace SitecoreMvcOtel;

public class OtelSqlServerDataProvider : SqlServerDataProvider
{
    public OtelSqlServerDataProvider(string connectionString) : base(connectionString) => SetPrivateFieldValue(this, "api", new OtelSqlServerDataApi(connectionString));

    private void SetPrivateFieldValue<T>(object obj, string fieldName, T val)
    {
        if (obj == null)
        {
            throw new ArgumentNullException("obj");
        }

        var type = obj.GetType();

        FieldInfo fieldInfo = null;
        
        while (fieldInfo == null && type != null)
        {
            fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            type = type.BaseType;
        }

        if (fieldInfo == null)
        {
            throw new ArgumentOutOfRangeException("propName", string.Format("Field {0} was not found in Type {1}", fieldName, obj.GetType().FullName));
        }

        fieldInfo.SetValue(obj, val);
    }
}
