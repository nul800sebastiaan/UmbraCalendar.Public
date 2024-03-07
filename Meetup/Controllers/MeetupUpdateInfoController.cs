using Microsoft.AspNetCore.Mvc;
using UmbraCalendar.CosmosDb;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Querying;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Web.Website.Controllers;

namespace UmbraCalendar.Meetup.Controllers;

public class MeetupUpdateInfoController : SurfaceController
{
    private readonly ICosmosService _cosmosService;
    private readonly IContentService _contentService;
    private readonly ISqlContext _sqlContext;
    private readonly IScopeProvider _scopeProvider;
    
    public MeetupUpdateInfoController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        ICosmosService cosmosService, IContentService contentService, ISqlContext sqlContext, IScopeProvider scopeProvider) 
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _cosmosService = cosmosService;
        _contentService = contentService;
        _sqlContext = sqlContext;
        _scopeProvider = scopeProvider;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditAction(MeetupGroup meetupGroup)
    {              
        using (var scope = _scopeProvider.CreateScope())
        {
            var query = new Query<IContent>(scope.SqlContext).Where(x => x.Name == "billing");
            var results =  _contentService.GetPagedDescendants(1234, 0, 100, out var totalRecords, query, null).FirstOrDefault();
            //Always complete scope
            scope.Complete();
        }
        
        var meetups = await _cosmosService.GetMeetupGroups();
        var meetup = meetups.First(x => x.id == meetupGroup.id);

        if (!string.Equals(meetup.Area, meetupGroup.Area, StringComparison.InvariantCultureIgnoreCase))
        {
            meetup.Area = meetupGroup.Area;
            var result = await _cosmosService.UpsertItemAsync(meetup, Constants.MeetupGroupsContainerId);
        }
        
        return RedirectToCurrentUmbracoPage();
    }
}