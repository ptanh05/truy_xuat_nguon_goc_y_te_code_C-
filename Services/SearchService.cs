using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PharmaDNA.Data;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public class SearchService : ISearchService
    {
        private readonly PharmaDNAContext _context;

        public SearchService(PharmaDNAContext context)
        {
            _context = context;
        }

        public async Task<SearchResult<NFT>> SearchNFTsAsync(SearchFilter filter)
        {
            var query = _context.NFTs.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(n => 
                    n.ProductName.Contains(filter.SearchTerm) ||
                    n.ProductCode.Contains(filter.SearchTerm) ||
                    n.Manufacturer.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.BatchId))
                query = query.Where(n => n.BatchId.Contains(filter.BatchId));

            if (!string.IsNullOrEmpty(filter.ProductCode))
                query = query.Where(n => n.ProductCode.Contains(filter.ProductCode));

            if (!string.IsNullOrEmpty(filter.ManufacturerName))
                query = query.Where(n => n.Manufacturer.Contains(filter.ManufacturerName));

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(n => n.Status == filter.Status);

            if (filter.StartDate.HasValue)
                query = query.Where(n => n.CreatedDate >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(n => n.CreatedDate <= filter.EndDate);

            if (!string.IsNullOrEmpty(filter.ProductType))
                query = query.Where(n => n.ProductType == filter.ProductType);

            query = filter.SortBy switch
            {
                "ProductName" => filter.SortDescending ? query.OrderByDescending(n => n.ProductName) : query.OrderBy(n => n.ProductName),
                "CreatedDate" => filter.SortDescending ? query.OrderByDescending(n => n.CreatedDate) : query.OrderBy(n => n.CreatedDate),
                "Manufacturer" => filter.SortDescending ? query.OrderByDescending(n => n.Manufacturer) : query.OrderBy(n => n.Manufacturer),
                _ => query.OrderByDescending(n => n.CreatedDate)
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new SearchResult<NFT>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<SearchResult<TransferRequest>> SearchTransfersAsync(SearchFilter filter)
        {
            var query = _context.TransferRequests.AsQueryable();

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                query = query.Where(t => 
                    t.NFT.ProductName.Contains(filter.SearchTerm) ||
                    t.FromAddress.Contains(filter.SearchTerm) ||
                    t.ToAddress.Contains(filter.SearchTerm));
            }

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(t => t.Status == filter.Status);

            if (filter.StartDate.HasValue)
                query = query.Where(t => t.RequestDate >= filter.StartDate);

            if (filter.EndDate.HasValue)
                query = query.Where(t => t.RequestDate <= filter.EndDate);

            query = filter.SortDescending 
                ? query.OrderByDescending(t => t.RequestDate) 
                : query.OrderBy(t => t.RequestDate);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Include(t => t.NFT)
                .ToListAsync();

            return new SearchResult<TransferRequest>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }

        public async Task<List<string>> GetSearchSuggestionsAsync(string term)
        {
            var suggestions = new List<string>();

            if (string.IsNullOrEmpty(term) || term.Length < 2)
                return suggestions;

            var productNames = await _context.NFTs
                .Where(n => n.ProductName.Contains(term))
                .Select(n => n.ProductName)
                .Distinct()
                .Take(5)
                .ToListAsync();

            var manufacturers = await _context.NFTs
                .Where(n => n.Manufacturer.Contains(term))
                .Select(n => n.Manufacturer)
                .Distinct()
                .Take(5)
                .ToListAsync();

            suggestions.AddRange(productNames);
            suggestions.AddRange(manufacturers);

            return suggestions.Distinct().ToList();
        }

        public async Task<Dictionary<string, int>> GetFilterOptionsAsync()
        {
            var options = new Dictionary<string, int>();

            var statuses = await _context.NFTs
                .GroupBy(n => n.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var status in statuses)
            {
                options[$"Status_{status.Status}"] = status.Count;
            }

            return options;
        }
    }
}
