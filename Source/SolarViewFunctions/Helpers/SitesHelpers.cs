using Microsoft.Azure.Cosmos.Table;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Extensions;
using System;
using System.Collections.Generic;

namespace SolarViewFunctions.Helpers
{
  public static class SitesHelpers
  {
    public static IReadOnlyList<SiteInfo> GetSites(CloudTable sitesTable, Func<SiteInfo, bool> predicate)
    {
      if (sitesTable.Name != $"{Constants.Table.Sites}")
      {
        throw new ArgumentException("The table is not bound to the expected table", nameof(sitesTable));
      }

      return sitesTable.GetPartitionItems<SiteInfo>("SiteId", predicate);
    }
  }
}