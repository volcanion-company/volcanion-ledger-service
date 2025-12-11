namespace Volcanion.LedgerService.Application.Common;

/// <summary>
/// Represents a paged result set containing a subset of items and pagination metadata for navigating large collections.
/// </summary>
/// <remarks>Use this class to encapsulate both the data for a single page and information needed for paging, such
/// as total item count and navigation properties. This is commonly used in scenarios where data is retrieved in
/// segments, such as database queries or API responses.</remarks>
/// <typeparam name="T">The type of items contained in the paged result.</typeparam>
/// <param name="items">The list of items included in the current page of results.</param>
/// <param name="totalCount">The total number of items available across all pages.</param>
/// <param name="page">The current page number, starting at 1.</param>
/// <param name="pageSize">The maximum number of items included in each page.</param>
public class PagedResult<T>(List<T> items, int totalCount, int page, int pageSize)
{
    /// <summary>
    /// Gets or sets the collection of items contained in the list.
    /// </summary>
    public List<T> Items { get; set; } = items;
    /// <summary>
    /// Gets or sets the total number of items available in the collection or result set.
    /// </summary>
    public int TotalCount { get; set; } = totalCount;
    /// <summary>
    /// Gets or sets the current page number in the paginated result set.
    /// </summary>
    public int Page { get; set; } = page;
    /// <summary>
    /// Gets or sets the number of items to include on each page of results.
    /// </summary>
    /// <remarks>Set this property to control the maximum number of items returned per page when retrieving
    /// paged data. The value should be a positive integer.</remarks>
    public int PageSize { get; set; } = pageSize;
    /// <summary>
    /// Gets the total number of pages available based on the current total item count and page size.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    /// <summary>
    /// Gets a value indicating whether there is a subsequent page available in the paginated result set.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
    /// <summary>
    /// Gets a value indicating whether there is a previous page available in the paginated sequence.
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}
