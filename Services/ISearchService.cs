using System.Collections.Generic;
using System.Threading.Tasks;
using PharmaDNA.Models;

namespace PharmaDNA.Services
{
    public interface ISearchService
    {
        Task<SearchResult<NFT>> SearchNFTsAsync(SearchFilter filter);
        Task<SearchResult<TransferRequest>> SearchTransfersAsync(SearchFilter filter);
        Task<List<string>> GetSearchSuggestionsAsync(string term);
        Task<Dictionary<string, int>> GetFilterOptionsAsync();
    }
}
