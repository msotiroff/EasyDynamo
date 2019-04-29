using System.Collections.Generic;

namespace EasyDynamo.Core
{
    public class PaginationResponse<TEntity> where TEntity : class, new()
    {
        /// <summary>
        /// Gets the next set of items.
        /// </summary>
        public IEnumerable<TEntity> NextResultSet { get; internal set; }

        /// <summary>
        /// Gets the current pagination token. 
        /// Return it to the next call to retrieve the next set of items.
        /// </summary>
        public string PaginationToken { get; internal set; }
    }
}
