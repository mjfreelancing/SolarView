using SendGrid.Helpers.Mail;
using SolarView.Common.Models;

namespace SolarViewFunctions.SendGrid
{
  // todo: add the ability to create emails based on a razor template

  public class SendGridEmailCreator : ISendGridEmailCreator
  {
    public SendGridMessage CreateMessage(ISiteDetails siteDetails, string subject, string mimeType, string content)
    {
      var email = new SendGridMessage();
      email.AddTo(siteDetails.ContactEmail);
      email.AddContent(mimeType, content);
      email.SetFrom(new EmailAddress(siteDetails.ContactEmail, siteDetails.ContactName));
      email.SetSubject(subject);

      return email;
    }
  }
}