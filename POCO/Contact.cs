using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using SQLite3Repository.Data.Interfaces;

namespace SQLite3Repository.Data.POCO
{
    public class Contact
    {

        public PrimaryKey PK { get; set; }
        public int Id
        {
            get { return (int)PK.Key; }
            set { PK.Key = (int)value; }
        }
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Display(Name = "Address 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address 2")]
        public string Address2 { get; set; }
        [Display(Name = "Notes")]
        public string Notes { get; set; }
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; }
        [Display(Name = "Home Phone")]
        public string HomePhone { get; set; }
        [Display(Name = "Work Phone")]
        public string WorkPhone { get; set; }
        [Display(Name = "Cell Phone")]
        public string CellPhone { get; set; }
        [Display(Name = "E-mail")]
        public string EMail { get; set; }
        [Display(Name = "City Id")]
        public int CityId { get; set; }
        [Display(Name = "Active")]
        public int Active { get; set; }
        [Display(Name = "Modified Date")]
        public DateTime ModifiedDt { get; set; }
        [Display(Name = "Create Date")]
        public DateTime CreateDt { get; set; }
        public Contact()
        {
            PK = new PrimaryKey() { Key = -1, IsIdentity = true };
        }
        public override string ToString()
        {
            return $"{Id}|{FirstName}|{LastName}|{Address1}|{Address2}|{Notes}|{ZipCode}|{HomePhone}|{WorkPhone}|{CellPhone}|{EMail}|{CityId}|{Active}|{ModifiedDt}|{CreateDt}|";
        }

        //Relation properties
        public City City { get; set; }

    }

    public class ContactMapToObject : MapToObjectBase<Contact>, IMapToObject<Contact>
	{
        public ContactMapToObject(ILogger l) : base(l)
        {

        }

        public override Contact Execute(IDataReader reader)
		{
            Contact contact = new Contact();

            contact.Id = reader.GetInt32(reader.GetOrdinal("Id"));
            contact.FirstName = reader.GetString(reader.GetOrdinal("FirstName"));
            contact.LastName = reader.GetString(reader.GetOrdinal("LastName"));
            contact.Address1 = GetNullableString(reader, "Address1");
            contact.Address2 = GetNullableString(reader, "Address2");
            contact.Notes = GetNullableString(reader, "Notes");
            contact.ZipCode = GetNullableString(reader, "ZipCode");
            contact.HomePhone = GetNullableString(reader, "HomePhone");
            contact.WorkPhone = GetNullableString(reader, "WorkPhone");
            contact.CellPhone = GetNullableString(reader, "CellPhone");
            contact.EMail = GetNullableString(reader, "EMail");
            contact.CityId = reader.GetInt32(reader.GetOrdinal("CityId"));
            contact.Active = reader.GetInt32(reader.GetOrdinal("Active"));
            contact.ModifiedDt = Constants.FromUnixTime(reader.GetInt64(reader.GetOrdinal("ModifiedDt")));
            contact.CreateDt = Constants.FromUnixTime(reader.GetInt64(reader.GetOrdinal("CreateDt")));
            return contact;
        }

        private static string GetNullableString(IDataReader reader, string column)
        {
            int ordinal = reader.GetOrdinal(column);
            return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
        }
    }

    public class ContactMapToObjectView : MapToObjectBase<Contact>, IMapToObject<Contact>
    {
        public ContactMapToObjectView(ILogger l) : base(l)
        {
        }

        public override Contact Execute(IDataReader reader)
        {
            IMapToObject<Contact> map = new ContactMapToObject(logger);
            Contact contact = map.Execute(reader);
            string stateId = reader.GetString(reader.GetOrdinal("StateId"));

            contact.City = new City
            {
                PK = new PrimaryKey { Key = contact.CityId, IsIdentity = true },
                Name = reader.GetString(reader.GetOrdinal("CityName")),
                StateId = stateId,
                State = new State { PK = new PrimaryKey { Key = stateId, IsIdentity = false }, Name = reader.GetString(reader.GetOrdinal("StateName")) }
            };
            return contact;
        }
    }

    public class ContactMapFromObject : MapFromObjectBase<Contact>, IMapFromObject<Contact>
    {
        public ContactMapFromObject(ILogger l) : base(l)
        {
        }

        public override void Execute(Contact contact, SqliteCommand cmd)
        {
            //Microsoft.Data.Sqlite requires DBNull.Value, rather than null, for NULL columns
            cmd.Parameters.Add(new SqliteParameter("@p1", contact.FirstName));
            cmd.Parameters.Add(new SqliteParameter("@p2", contact.LastName));
            cmd.Parameters.Add(new SqliteParameter("@p3", (object)contact.Address1 ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p4", (object)contact.Address2 ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p5", (object)contact.Notes ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p6", (object)contact.ZipCode ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p7", (object)contact.HomePhone ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p8", (object)contact.WorkPhone ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p9", (object)contact.CellPhone ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p10", (object)contact.EMail ?? DBNull.Value));
            cmd.Parameters.Add(new SqliteParameter("@p11", contact.CityId));
        }
    }
}
