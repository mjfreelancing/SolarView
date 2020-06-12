using SendGrid.Helpers.Mail;
using SolarViewFunctions.Entities;

namespace SolarViewFunctions.SendGrid
{
  public interface ISendGridEmailCreator
  {
    SendGridMessage CreateMessage(SiteInfo siteInfo, string subject, string mimeType, string content);
  }
}