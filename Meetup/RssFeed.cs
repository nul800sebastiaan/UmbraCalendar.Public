using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using UmbraCalendar.CosmosDb;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Meetup;

public class RssController : RenderController
{
    private readonly ICosmosService _cosmosService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    
    public RssController(ILogger<RenderController> logger, 
        ICompositeViewEngine compositeViewEngine, 
        IUmbracoContextAccessor umbracoContextAccessor,
        ICosmosService cosmosService,
        IServiceProvider serviceProvider,
        IUmbracoContextFactory umbracoContextFactory) 
        : base(logger, compositeViewEngine, umbracoContextAccessor)
    {
        _cosmosService = cosmosService;
        _serviceProvider = serviceProvider;
        _umbracoContextFactory = umbracoContextFactory;
    }
      
    //[ResponseCache(Duration = 1200)]
    [HttpGet]
    public IActionResult Index()
    {
        var blogCopyright = $"{DateTime.Now.Year} Umbracalendar";
        var blogTitle = "Umbraco events on Meetup.com";
        var blogDescription = "";
        var blogUrl = "https://umbracalendar.com/";
        var blogSlug = "/";
        var feedId = "MeetupRss";

        var feed = new SyndicationFeed(blogTitle, blogDescription, new Uri(blogUrl), feedId, DateTime.Now)
        {
            Copyright = new TextSyndicationContent(blogCopyright)
        };

        var items = new List<SyndicationItem>();

        using var _ = _umbracoContextFactory.EnsureUmbracoContext();
        using var serviceScope = _serviceProvider.CreateScope();
        var query = serviceScope.ServiceProvider.GetRequiredService<IPublishedContentQuery>();
        var rootNode = query.ContentAtRoot().FirstOrDefault();
        
        if (rootNode != null)
        {
            foreach (var umbracoEvent in rootNode.Children.OfType<CalendarEvent>().Where(x => x.DateTo >= DateTime.Today))
            {
                if (umbracoEvent.EventLink?.Url == null) continue;
                
                var convertedDateFrom = DateTime.SpecifyKind(umbracoEvent.DateFrom, DateTimeKind.Local);
                var convertedDateTo = DateTime.SpecifyKind(umbracoEvent.DateTo, DateTimeKind.Local);
                var description = $"{convertedDateFrom:yyyy-MM-dd} from {convertedDateFrom:HH:mm} to {convertedDateTo:HH:mm} - {umbracoEvent.EventLocation}";
                
                if (umbracoEvent.DateFrom.ToString("yyyy-MM-dd") != umbracoEvent.DateTo.ToString("yyyy-MM-dd"))
                {
                    description = $"{umbracoEvent.DateFrom:yyyy-MM-dd} to {umbracoEvent.DateTo:yyyy-MM-dd} - {umbracoEvent.EventLocation}";
                }
                var item = new SyndicationItem(umbracoEvent.Name, description, new Uri(umbracoEvent.EventLink.Url))
                {
                    PublishDate = convertedDateFrom,
                    Id = umbracoEvent.Id.ToString(),
                    Summary = new TextSyndicationContent(description)

                };
                
                items.Add(item);
            }
        }
        
        var upcomingEvents = _cosmosService.GetUpcomingMeetupEvents().Result;
        foreach (var meetupEvent in upcomingEvents)
        {
            var venueFormatted = string.Empty;
            if (meetupEvent.OnlineVenue != null)
            {
                venueFormatted = $" - Online on {meetupEvent.OnlineVenue.Type}";
            }
            else if(meetupEvent.Venue != null)
            {
                var array = new[] { meetupEvent.Venue.Name, meetupEvent.Venue.Address, meetupEvent.Venue.City, meetupEvent.Venue.Country.ToUpperInvariant() };
                var fullAddress = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
                venueFormatted = $" - {fullAddress}";
            }

            if (venueFormatted.Trim() == string.Empty)
            {
                venueFormatted = " - Needs a location";
            }
            var description = $"{meetupEvent.StartDateLocal} from {meetupEvent.StartTimeLocal} to {meetupEvent.EndTimeLocal}{venueFormatted}";
            var startDateTime = DateTimeOffset.Parse(meetupEvent.StartDateTime);
            var item = new SyndicationItem(meetupEvent.Title, description, new Uri(meetupEvent.EventUrl))
            {
                PublishDate = startDateTime,
                Id = meetupEvent.id,
                Summary = new TextSyndicationContent(description)
                
            };
            items.Add(item);
        }

        feed.Items = items.OrderBy(x => x.PublishDate);

        var settings = new XmlWriterSettings
        {
            Encoding = Encoding.UTF8,
            NewLineHandling = NewLineHandling.Entitize,
            NewLineOnAttributes = true,
            Indent = true
        };

        using var stream = new MemoryStream();
        using (var xmlWriter = XmlWriter.Create(stream, settings))
        {
            var rssFormatter = new Rss20FeedFormatter(feed, false);
            rssFormatter.WriteTo(xmlWriter);
            xmlWriter.Flush();
        }

        return File(stream.ToArray(), "application/rss+xml; charset=utf-8");
    }
}