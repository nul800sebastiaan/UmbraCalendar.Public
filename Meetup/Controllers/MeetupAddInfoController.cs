using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using UmbraCalendar.CosmosDb;
using UmbraCalendar.Meetup.Models.Areas;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace UmbraCalendar.Meetup.Controllers;

public class MeetupAddInfoController : RenderController
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly ServiceContext _serviceContext;
    private readonly ICosmosService _cosmosService;
    private readonly IMeetupService _meetupService;

    public MeetupAddInfoController(
        ILogger<RenderController> logger,
        ICompositeViewEngine compositeViewEngine,
        IUmbracoContextAccessor umbracoContextAccessor,
        IVariationContextAccessor variationContextAccessor,
        ServiceContext serviceContext,
        ICosmosService cosmosService,
        IMeetupService meetupService)
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _variationContextAccessor = variationContextAccessor;
        _serviceContext = serviceContext;
        _cosmosService = cosmosService;
        _meetupService = meetupService;
    }

    public async Task<IActionResult> MeetupAddInfoOverview()
    {
        var publishedValueFallback = new PublishedValueFallback(_serviceContext, _variationContextAccessor);
        var availableAreas = await _meetupService.GetAvailableAreas();
        var meetupGroups = await _cosmosService.GetMeetupGroups();
        var addInfoModel = new AddInfoOverviewModel(CurrentPage, publishedValueFallback)
        {
            MeetupGroups = meetupGroups,
            MeetupAreas = availableAreas ?? new List<MeetupArea>()
        };

        return CurrentTemplate(addInfoModel);
    }

    public async Task<IActionResult> MeetupAddInfo([FromQuery] string id)
    {
        var publishedValueFallback = new PublishedValueFallback(_serviceContext, _variationContextAccessor);

        var availableAreas = await _meetupService.GetAvailableAreas();
        var meetupGroups = await _cosmosService.GetMeetupGroups();
        var group = meetupGroups.First(x => x.id == id);
        var addInfoModel = new AddInfoModel(CurrentPage, publishedValueFallback)
        {
            Group = group,
            MeetupAreas = availableAreas ?? new List<MeetupArea>()
        };

        return CurrentTemplate(addInfoModel);
    }
}