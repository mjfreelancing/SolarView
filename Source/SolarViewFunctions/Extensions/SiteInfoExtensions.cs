using AllOverIt.Extensions;
using SolarView.Common.Extensions;
using SolarView.Common.Models;
using System;

namespace SolarViewFunctions.Extensions
{
  public static class SiteInfoExtensions
  {
    public static DateTime GetLastAggregationDate(this ISiteDetails siteDetails)
    {
      // returns in site's local date
      return siteDetails.LastAggregationDate.IsNullOrEmpty()
        ? siteDetails.StartDate.ParseSolarDate().Date
        : siteDetails.LastAggregationDate.ParseSolarDate();
    }

    public static DateTime GetLastRefreshDateTime(this ISiteDetails siteDetails)
    {
      // returns in site's local date
      return siteDetails.LastRefreshDateTime.IsNullOrEmpty()
        ? siteDetails.StartDate.ParseSolarDate().Date
        : siteDetails.LastRefreshDateTime.ParseSolarDateTime().TrimToHour();
    }

    public static DateTime GetLastSummaryDate(this ISiteDetails siteDetails)
    {
      // returns in site's local date
      return siteDetails.LastSummaryDate.IsNullOrEmpty() 
        ? siteDetails.StartDate.ParseSolarDate()
        : siteDetails.LastSummaryDate.ParseSolarDate();
    }
  }
}