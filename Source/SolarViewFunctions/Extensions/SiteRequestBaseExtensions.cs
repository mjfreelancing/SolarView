using System.Threading.Tasks;
using SolarViewFunctions.Dto;
using SolarViewFunctions.Entities;
using SolarViewFunctions.Exceptions;
using SolarViewFunctions.Repository.Sites;
using SolarViewFunctions.Validators;

namespace SolarViewFunctions.Extensions
{
  public static class SiteRequestBaseExtensions
  {
    public static async Task<SiteInfo> GetValidatedSiteInfo(this SiteRequestBase hydrateRequest, ISitesRepository sitesRepository)
    {
      var siteInfo = await sitesRepository.GetSiteAsync(hydrateRequest.SiteId);

      var validator = new SiteValidator(siteInfo);

      // ReSharper disable once MethodHasAsyncOverload
      var validationResult = validator.Validate(hydrateRequest);

      if (!validationResult.IsValid)
      {
        throw new PreConditionException(validationResult.Errors);
      }

      return siteInfo;
    }
  }
}