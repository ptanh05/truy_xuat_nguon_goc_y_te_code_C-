using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PharmaDNA.Models;
using PharmaDNA.Services;

namespace PharmaDNA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;

        public SearchController(ISearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpPost("nfts")]
        public async Task<IActionResult> SearchNFTs([FromBody] SearchFilter filter)
        {
            var result = await _searchService.SearchNFTsAsync(filter);
            return Ok(result);
        }

        [HttpPost("transfers")]
        public async Task<IActionResult> SearchTransfers([FromBody] SearchFilter filter)
        {
            var result = await _searchService.SearchTransfersAsync(filter);
            return Ok(result);
        }

        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] string term)
        {
            var suggestions = await _searchService.GetSearchSuggestionsAsync(term);
            return Ok(suggestions);
        }

        [HttpGet("filter-options")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var options = await _searchService.GetFilterOptionsAsync();
            return Ok(options);
        }
    }
}
