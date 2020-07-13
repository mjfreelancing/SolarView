using SolarView.Common.Models;

namespace SolarView.Client.Common.Models
{
  public class DateRangeSelectOption
  {
    public string Caption { get; set; }
    public string Option { get; set; }     // string version of DateRangeOption
    public DateRange DateRange { get; set; }
  }
}
