/******************************************************************************************************
 *This class was generated on 04/30/2014 09:00:34 using Repository Builder version 0.9. *
 *The class was generated from Database: Customer and Table: City.  *
******************************************************************************************************/
using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data.POCO
{

    public class City
    {

        public PrimaryKey PK { get; set; }
        public int Id
        {
            get { return (int)PK.Key; }
            set { PK.Key = (int)value; }
        }
        public string Name { get; set; }
        public string StateId { get; set; }
        public int Active { get; set; }
        public DateTime ModifiedDt { get; set; }
        public DateTime CreateDt { get; set; }
        public City()
        {
            PK = new PrimaryKey() { Key = -1, IsIdentity = true };
        }
		public override string ToString()
		{
			return $"{Id}|{Name}|{StateId}|{Active}|{ModifiedDt}|{CreateDt}|";
		}

        //Relation properties
        public State State { get; set; }
    }

    public class CityMapToObject : MapToObjectBase<City>, IMapToObject<City>
    {
        public CityMapToObject(ILogger l) : base(l)
        {
        }

        public override City Execute(IDataReader reader)
        {
            City city = new City();

            city.Id = reader.GetInt32(reader.GetOrdinal("Id"));
            city.Name = reader.GetString(reader.GetOrdinal("Name"));
            city.StateId = reader.GetString(reader.GetOrdinal("StateId"));
            city.Active = reader.GetInt32(reader.GetOrdinal("Active"));
            city.ModifiedDt = Constants.FromUnixTime(reader.GetInt64(reader.GetOrdinal("ModifiedDt")));
            city.CreateDt = Constants.FromUnixTime(reader.GetInt64(reader.GetOrdinal("CreateDt")));
            return city;
        }
    }

    public class CityMapToObjectView : MapToObjectBase<City>, IMapToObject<City>
    {
        public CityMapToObjectView(ILogger l) : base(l)
        {

        }

        public override City Execute(IDataReader reader)
        {
            IMapToObject<City> map = new CityMapToObject(logger);
            City city = map.Execute(reader);

            city.State = new State
            {
                PK = new PrimaryKey { Key = city.StateId, IsIdentity = false },
                Name = reader.GetString(reader.GetOrdinal("StateName"))
            };
            return city;
        }
    }

    public class CityMapFromObject : MapFromObjectBase<City>, IMapFromObject<City>
    {
        public CityMapFromObject(ILogger l) : base(l)
        {
        }

        public override void Execute(City city, SqliteCommand cmd)
        {
            cmd.Parameters.Add(new SqliteParameter("@p1", city.Name));
            cmd.Parameters.Add(new SqliteParameter("@p2", city.StateId));
        }
    }
}
