using SolarView.Common.Models;

namespace SolarViewBlazor.Models
{
  public class DateRangeSelectOption
  {
    public string Caption { get; set; }
    public bool Selected { get; set; }
    public bool IsCustom { get; set; }
    public DateRange DateRange { get; set; }
  }
}
