using Microsoft.Azure.WebJobs;
using SolarViewFunctions.Entities;
using System;
using System.Threading.Tasks;

namespace SolarViewFunctions.Extensions
{
  public static class ExceptionDocumentExtensions
  {
    public static async Task AddNotificationAsync<TSource>(this IAsyncCollector<ExceptionDocument> exceptionDocuments, string siteId, Exception exception,
      object notification) where TSource : class
    {
      var exceptionDocument = new ExceptionDocument(typeof(TSource).Name, siteId, exception, notification);
      await exceptionDocuments.AddAsync(exceptionDocument);
    }
  }
}