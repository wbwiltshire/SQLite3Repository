/******************************************************************************************************
 *This class was generated on 04/20/2014 09:21:22 using Repository Builder version 0.9. *
 *The class was generated from Database: BACS and Table: State.  *
******************************************************************************************************/
using System;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data.POCO
{
    public class State
    {
        public PrimaryKey PK { get; set; }
        public string Id
        {
            get { return (string)PK.Key; }
            set { PK.Key = (string)value; }
        }
        public string Name { get; set; }
        //Properties managed by the architecture
        public int Active { get; set; }
        public DateTime ModifiedDt { get; set; }
        public DateTime CreateDt { get; set; }
        public State()
        {
            PK = new PrimaryKey() { Key = String.Empty, IsIdentity = false };
        }
        public string ToPrint()
        {
            return String.Format("{0}|{1}|{2}|{3}|{4}", Id, Name, Active, ModifiedDt, CreateDt);
        }

    }

    public class StateMapToObject : MapToObjectBase<State>, IMapToObject<State>
    {
        public StateMapToObject(ILogger l) : base(l)
        {
        }

        public override State Execute(IDataReader reader)
        {
            State state = new State();

            state.Id = reader.GetString(reader.GetOrdinal("Id"));
            state.Name = reader.GetString(reader.GetOrdinal("Name"));
            state.Active = reader.GetInt32(reader.GetOrdinal("Active"));
            state.ModifiedDt = Constants.FromUnixTime(reader.GetInt64(reader.GetOrdinal("ModifiedDt")));
            state.CreateDt = Constants.FromUnixTime(reader.GetInt64(reader.GetOrdinal("CreateDt")));
            return state;
        }
    }

    public class StateMapFromObject : MapFromObjectBase<State>, IMapFromObject<State>
    {
        public StateMapFromObject(ILogger l) : base(l)
        {
        }

        public override void Execute(State state, SqliteCommand cmd)
        {
            cmd.Parameters.Add(new SqliteParameter("@p1", state.Name));
        }
    }
}
