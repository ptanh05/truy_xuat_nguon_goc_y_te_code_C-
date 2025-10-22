using System;
using System.Collections.Generic;

namespace PharmaDNA.Models
{
    public class SearchFilter
    {
        public string SearchTerm { get; set; }
        public string BatchId { get; set; }
        public string ProductCode { get; set; }
        public string ManufacturerName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ProductType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedDate";
        public bool SortDescending { get; set; } = true;
    }

    public class SearchResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }
}
