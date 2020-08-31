/******************************************************************************************************
 *This class was generated on 04/30/2014 09:00:34 using Repository Builder version 0.9. *
 *The class was generated from Database: Customer and Table: City.  *
******************************************************************************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Text;
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
            int ordinal = 0;

            try
            {
				ordinal = reader.GetOrdinal("Id");
				city.Id = reader.GetInt32(ordinal);
				ordinal = reader.GetOrdinal("Name");
				city.Name = reader.GetString(ordinal);
				ordinal = reader.GetOrdinal("StateId");
				city.StateId = reader.GetString(ordinal);
				ordinal = reader.GetOrdinal("Active");
				city.Active = reader.GetInt32(ordinal);
				ordinal = reader.GetOrdinal("ModifiedDt");
				city.ModifiedDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(ordinal));
                ordinal = reader.GetOrdinal("CreateDt");
				city.CreateDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(ordinal));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
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

            try
            {
                city.State = new State
                {
                    PK = new PrimaryKey { Key = city.StateId, IsIdentity = true },
                    Name = reader.GetString(reader.GetOrdinal("StateName"))
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
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
            SqliteParameter parm;

            try
            {
                parm = new SqliteParameter("@p1", city.Name);
                cmd.Parameters.Add(parm);
                parm = new SqliteParameter("@p2", city.StateId);
                cmd.Parameters.Add(parm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}