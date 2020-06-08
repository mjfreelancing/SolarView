using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace SolarViewFunctions.Extensions
{
  public static class MessageExtensions
  {
    public static TType DeserializeFromMessage<TType>(this Message message)
    {
      return JsonConvert.DeserializeObject<TType>(Encoding.UTF8.GetString(message.Body));
    }
  }
}