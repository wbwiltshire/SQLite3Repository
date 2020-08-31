using System;
using System.Collections.Generic;

namespace SQLite3Repository.Data.POCO
{
    public class AppSettingsConfiguration
    {
        public Database Database { get; set; }
        public Serilog Serilog { get; set; }
    }
    
    //Logging Objects
    public class Serilog
    {
        public string[] Using { get; set; }
        public string MinimumLevel { get; set; }
        public WriteTo[] WriteTo { get; set; }
    }

    public class WriteTo
    {
        public string[] Options { get; set; }
    }

    //Database Objects
    public class Database
    {
        public string Server { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string[] StoredProcedures { get; set; }
    }

}
