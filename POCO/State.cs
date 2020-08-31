/******************************************************************************************************
 *This class was generated on 04/20/2014 09:21:22 using Repository Builder version 0.9. *
 *The class was generated from Database: BACS and Table: State.  *
******************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Text;
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
            int ordinal = 0;
            try
            {
                ordinal = reader.GetOrdinal("Id");
                state.Id = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("Name");
                state.Name = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("Active");
                state.Active = reader.GetInt32(ordinal);
                ordinal = reader.GetOrdinal("ModifiedDt");
                state.ModifiedDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(ordinal));
                ordinal = reader.GetOrdinal("CreateDt");
                state.CreateDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(ordinal));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            return state;
        }
    }
    public class StateMapToObjectView : MapToObjectBase<State>, IMapToObject<State>
    {
        public StateMapToObjectView(ILogger l) : base(l)
        {
        }

        public override State Execute(IDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
    public class StateMapFromObject : MapFromObjectBase<State>, IMapFromObject<State>
    {
        public StateMapFromObject(ILogger l) : base(l)
        {
        }

        public override void Execute(State state, SqliteCommand cmd)
        {
            SqliteParameter parm;

            try
            {
                parm = new SqliteParameter("@p1", state.Name);
                cmd.Parameters.Add(parm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}