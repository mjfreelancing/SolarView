using AllOverIt.Helpers;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SolarViewFunctions.Helpers
{
  public class DurableHttpRequestBuilder
  {
    private readonly string _fullUri;
    private readonly StringBuilder _queryBuilder = new StringBuilder();
    private readonly IDictionary<string, StringValues> _headers = new Dictionary<string, StringValues>();

    public static DurableHttpRequestBuilder CreateUri(string baseUri, string route)
    {
      return new DurableHttpRequestBuilder(baseUri, route);
    }

    public DurableHttpRequestBuilder AddParameter(string name, string value)
    {
      _queryBuilder.Append(_queryBuilder.Length == 0 ? "?" : "&");
      _queryBuilder.Append($"{name}={value}");

      return this;
    }

    public DurableHttpRequestBuilder AddHeader(string name, StringValues values)
    {
      _headers.Add(name, values);

      return this;
    }

    public DurableHttpRequest Build()
    {
      return new DurableHttpRequest(HttpMethod.Get, new Uri($"{_fullUri}{_queryBuilder}"), _headers);
    }

    private DurableHttpRequestBuilder(string baseUri, string route)
    {
      _ = baseUri.WhenNotNullOrEmpty(nameof(baseUri));
      _ = route.WhenNotNullOrEmpty(nameof(route));

      _fullUri = baseUri.Substring(baseUri.Length - 1) == "/" 
        ? $"{baseUri}{route}" 
        : $"{baseUri}/{route}";
    }
  }
}