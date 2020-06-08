using AllOverIt.Extensions;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;

namespace SolarViewFunctions.Helpers
{
  public static class MessageHelpers
  {
    // all values of each property must be convertible to a string
    public static Message SerializeToMessage(object @object, object properties = null)
    {
      var message = new Message(GetBytes(@object))
      {
        ContentType = "application/json;charset=utf-8"
      };

      if (properties != null)
      {
        var propertyValues = properties.ToPropertyDictionary();

        foreach (var (key, value) in propertyValues)
        {
          message.UserProperties.Add(key, $"{value}");
        }
      }

      return message;
    }

    private static byte[] GetBytes(object updatedMessage)
    {
      return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(updatedMessage));
    }
  }
}