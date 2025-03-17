using Sitecore.Data.DataProviders.Sql;
using Sitecore.Data.SqlServer;
using System.Diagnostics;
using System.Linq;

namespace SitecoreMvcOtel;

public class OtelSqlServerDataApi : SqlServerDataApi
{
    public OtelSqlServerDataApi(string connectionString) : base(connectionString)
    {

    }

    public override DataProviderReader CreateReader(string sql, params object[] parameters)
    {
        using var source = Activity.Current?.Source.StartActivity("SitecoreSqlServerDataApi");

        if (source != null)
        {
            source.AddTag("sqlserver.dataapi.method", "CreateReader");
            source.AddTag("sqlserver.dataapi.sql", sql);
            source.AddTag("sqlserver.dataapi.parameters", string.Join(",", [.. parameters.Select(p => p.ToString())]));
        }

        return base.CreateReader(sql, parameters);

    }
}
