using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StockWise.Data;

namespace StockWise.Pages.History
{
    /// <summary>
    /// Page model for browsing the history log.
    /// </summary>
    /// <param name="db">The database context.</param>
    public class IndexModel(StockWiseDbContext db) : PageModel
    {
        /// <summary>
        /// The logged history events, sorted by the requested column.
        /// </summary>
        public List<StockWise.Models.History> Events { get; set; } = [];

        /// <summary>
        /// The column to sort the history table by.
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Sort { get; set; } = "timestamp";

        /// <summary>
        /// The sort direction: "asc" or "desc".
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "desc";

        /// <summary>
        /// Loads the history events.
        /// </summary>
        public async Task OnGetAsync()
        {
            Events = await ApplySort(db.History, Sort, Direction == "desc").ToListAsync();
        }

        /// <summary>
        /// Applies the requested sort to the history query.
        /// </summary>
        /// <param name="query">The history query to sort.</param>
        /// <param name="sort">The column to sort by.</param>
        /// <param name="descending">Whether to sort in descending order.</param>
        private static IOrderedQueryable<StockWise.Models.History> ApplySort(IQueryable<StockWise.Models.History> query, string sort, bool descending)
        {
            return sort switch
            {
                "message" => descending ? query.OrderByDescending(x => x.Message) : query.OrderBy(x => x.Message),
                _ => descending ? query.OrderByDescending(x => x.Timestamp) : query.OrderBy(x => x.Timestamp),
            };
        }
    }
}