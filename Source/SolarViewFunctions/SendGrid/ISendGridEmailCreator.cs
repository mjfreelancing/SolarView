using SendGrid.Helpers.Mail;
using SolarView.Common.Models;

namespace SolarViewFunctions.SendGrid
{
  public interface ISendGridEmailCreator
  {
    SendGridMessage CreateMessage(ISiteInfo siteInfo, string subject, string mimeType, string content);
  }
}