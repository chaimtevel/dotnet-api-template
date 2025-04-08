using Microsoft.AspNetCore.Mvc;
using Services.AppServices;
using Services.HelperServices;
using Shared.Models.Responses;
using static Shared.Extensions.ReflectionExtensions;

namespace Api.Controllers;

[Route("api/[controller]"), ApiController]
public class SystemController(IGeoService geoService, ICacheService cacheService) : ControllerBase
{
    public IGeoService GeoService { get; } = geoService;
    public ICacheService CacheService { get; } = cacheService;

    [HttpGet]
    public async Task<ActionResult> GetAsync(CancellationToken token)
    {
        await Task.CompletedTask;

        var response = new
        {
            
        };

        return Ok(response.ToDictionary(lowercaseFirst: true));
    }

    [HttpGet("states")]
    public async Task<ActionResult<IEnumerable<StateResponse>>> GetStatesAsync(CancellationToken token)
    {
        var states = await GeoService.GetStatesCacheAsync(token);

        var stateResponses = states
            .Select(s => new StateResponse
            {
                ID = s.ID,
                Name = s.Name,
                StateCode = s.Code
            })
            .ToArray();

        return Ok(stateResponses);
    }
}
