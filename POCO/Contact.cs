using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Text;
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
            int ordinal = 0;
            try
            {
                ordinal = reader.GetOrdinal("Id");
                contact.Id = reader.GetInt32(ordinal);
                ordinal = reader.GetOrdinal("FirstName");
                contact.FirstName = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("LastName");
                contact.LastName = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("Address1");
                if (reader.IsDBNull(ordinal) == false)
                    contact.Address1 = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("Address2");
                if (reader.IsDBNull(ordinal) == false)
                    contact.Address2 = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("Notes");
                if (reader.IsDBNull(ordinal) == false)
                    contact.Notes = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("ZipCode");
                if (reader.IsDBNull(ordinal) == false)
                    contact.ZipCode = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("HomePhone");
                if (reader.IsDBNull(ordinal) == false)
                    contact.HomePhone = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("WorkPhone");
                if (reader.IsDBNull(ordinal) == false)
                    contact.WorkPhone = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("CellPhone");
                if (reader.IsDBNull(ordinal) == false)
                    contact.CellPhone = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("EMail");
                if (reader.IsDBNull(ordinal) == false)
                    contact.EMail = reader.GetString(ordinal);
                ordinal = reader.GetOrdinal("CityId");
                contact.CityId = reader.GetInt32(ordinal);
                ordinal = reader.GetOrdinal("Active");
                contact.Active = reader.GetInt32(ordinal);
                ordinal = reader.GetOrdinal("ModifiedDt");
                contact.ModifiedDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(ordinal));
                ordinal = reader.GetOrdinal("CreateDt");
                contact.CreateDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(ordinal));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
            return contact;
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

            try
            {
                contact.City = new City
                {
                    PK = new PrimaryKey { Key = contact.CityId, IsIdentity = true },
                    Name = reader.GetString(reader.GetOrdinal("CityName")),
                    StateId = reader.GetString(reader.GetOrdinal("StateId")),
                    State = new State { PK = new PrimaryKey { Key = reader.GetString(13), IsIdentity = false }, Name = reader.GetString(reader.GetOrdinal("StateName")) }
                };
                //contact.Active = reader.GetInt32(15);
                //contact.ModifiedDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(16));
                //contact.CreateDt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.GetInt64(17));
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
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
            SqliteParameter parm;

            try
            {
                parm = new SqliteParameter("@p1", contact.FirstName);
                cmd.Parameters.Add(parm);
                parm = new SqliteParameter("@p2", contact.LastName);
                cmd.Parameters.Add(parm);
                if (contact.Address1 == null)
                    parm = new SqliteParameter("@p3", DBNull.Value);
                else
                    parm = new SqliteParameter("@p3", contact.Address1);
                cmd.Parameters.Add(parm);
                if (contact.Address2 == null)
                    parm = new SqliteParameter("@p4", DBNull.Value);
                else
                    parm = new SqliteParameter("@p4", contact.Address2);
                cmd.Parameters.Add(parm);
                if (contact.Notes == null)
                    parm = new SqliteParameter("@p5", DBNull.Value);
                else
                    parm = new SqliteParameter("@p5", contact.Notes);
                cmd.Parameters.Add(parm);
                if (contact.ZipCode == null)
                    parm = new SqliteParameter("@p6", DBNull.Value);
                else
                    parm = new SqliteParameter("@p6", contact.ZipCode);
                cmd.Parameters.Add(parm);
                if (contact.HomePhone == null)
                    parm = new SqliteParameter("@p7", DBNull.Value);
                else
                    parm = new SqliteParameter("@p7", contact.HomePhone);
                cmd.Parameters.Add(parm);
                if (contact.WorkPhone == null)
                    parm = new SqliteParameter("@p8", DBNull.Value);
                else
                    parm = new SqliteParameter("@p8", contact.WorkPhone);
                cmd.Parameters.Add(parm);
                if (contact.CellPhone == null)
                    parm = new SqliteParameter("@p9", DBNull.Value);
                else
                    parm = new SqliteParameter("@p9", contact.CellPhone);
                cmd.Parameters.Add(parm);
                if (contact.EMail == null)
                    parm = new SqliteParameter("@p10", DBNull.Value);
                else
                    parm = new SqliteParameter("@p10", contact.EMail);
                cmd.Parameters.Add(parm);
                parm = new SqliteParameter("@p11", contact.CityId);
                cmd.Parameters.Add(parm);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
