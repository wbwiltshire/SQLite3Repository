using System;
using System.Collections.Generic;

namespace SQLite3Repository.Data
{
    public interface IPager<TEntity>
        where TEntity : class
    {
        int PageNbr { get; set; }
        int PageSize { get; set; }
        string SortColumn { get; set; }
        SQLOrderBy Direction { get; set; }
        int RowCount { get; set; }
        ICollection<TEntity> Entities { get; set; }
    }

    public class Pager<TEntity> : IPager<TEntity>
        where TEntity : class
    {
        public int PageNbr { get; set; }
        public int PageSize { get; set; }
        public string SortColumn { get; set; }
        public SQLOrderBy Direction { get; set; }
        public int RowCount { get; set; }
        public ICollection<TEntity> Entities { get; set; }

        public Pager()
        {
            PageNbr = 0;                        // Zero-based page number
            PageSize = 20;
            SortColumn = "Id";                  // Default to Id column
            Direction = SQLOrderBy.ASC;         // Default to ASC
        }
    }
}
