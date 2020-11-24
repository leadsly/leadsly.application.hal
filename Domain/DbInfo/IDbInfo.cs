using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.DbInfo
{
    public interface IDbInfo
    {
        string ConnectionString { get; set; }
    }
}
