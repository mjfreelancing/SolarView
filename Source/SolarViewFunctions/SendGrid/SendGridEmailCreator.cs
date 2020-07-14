using SendGrid.Helpers.Mail;
using SolarView.Common.Models;

namespace SolarViewFunctions.SendGrid
{
  // todo: add the ability to create emails based on a razor template

  public class SendGridEmailCreator : ISendGridEmailCreator
  {
    public SendGridMessage CreateMessage(ISiteInfo siteInfo, string subject, string mimeType, string content)
    {
      var email = new SendGridMessage();
      email.AddTo(siteInfo.ContactEmail);
      email.AddContent(mimeType, content);
      email.SetFrom(new EmailAddress(siteInfo.ContactEmail, siteInfo.ContactName));
      email.SetSubject(subject);

      return email;
    }
  }
}