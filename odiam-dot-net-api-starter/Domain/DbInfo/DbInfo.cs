using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DbInfo
{
    public class DbInfo : IDbInfo
    {
        public DbInfo(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string ConnectionString { get; set; }
    }
}
