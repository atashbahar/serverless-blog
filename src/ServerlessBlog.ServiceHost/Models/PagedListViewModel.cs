using System;
using System.Collections.Generic;

namespace ServerlessBlog.ServiceHost.Models
{
    public class PagedList<T>
    {
        public IEnumerable<T> Items { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }

        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);

        public int? NextPage => CurrentPage + 1 > TotalPages ? null : (int?)CurrentPage + 1;

        public int? PreviousPage => CurrentPage - 1 < 1 ? null : (int?)CurrentPage - 1;
    }
}
