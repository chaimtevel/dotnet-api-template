using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.AppServices;
using Services.HelperServices;
using static Shared.Extensions.ReflectionExtensions;

namespace Api.Controllers;

[Route("api/[controller]"), ApiController, Authorize]
public class SystemController : ControllerBase
{
    public SystemController(IGeoService geoService, ICachingService cachingService)
    {
        GeoService = geoService;
        CachingService = cachingService;
    }

    public IGeoService GeoService { get; }
    public ICachingService CachingService { get; }

    [HttpGet]
    public async Task<ActionResult> GetAsync(CancellationToken token)
    {
        var response = new
        {
            States = await GeoService.GetStatesCacheAsync(token)
        };

        return Ok(response.ToDictionary(lowercaseFirst: true));
    }
}
