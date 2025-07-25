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

        // Add event namespace for RSS event module support
        feed.AttributeExtensions.Add(new XmlQualifiedName("ev", "http://www.w3.org/2000/xmlns/"), 
                                   "http://purl.org/rss/1.0/modules/event/");

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
                
                // Add RSS event module extensions
                AddEventExtensions(item, convertedDateFrom, convertedDateTo, umbracoEvent.EventLocation, "Conference", umbracoEvent.EventHost);
                
                items.Add(item);
            }
        }
        
        var upcomingEvents = _cosmosService.GetUpcomingMeetupEvents().Result;
        foreach (var meetupEvent in upcomingEvents)
        {
            var venueFormatted = string.Empty;
            var eventLocation = string.Empty;
            var eventType = "Meetup";
            
            if (meetupEvent.OnlineVenue != null)
            {
                venueFormatted = $" - Online on {meetupEvent.OnlineVenue.Type}";
                eventLocation = $"Online ({meetupEvent.OnlineVenue.Type})";
                eventType = "Online Meetup";
            }
            else if(meetupEvent.Venue != null)
            {
                var array = new[] { meetupEvent.Venue.Name, meetupEvent.Venue.Address, meetupEvent.Venue.City, meetupEvent.Venue.Country.ToUpperInvariant() };
                var fullAddress = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
                venueFormatted = $" - {fullAddress}";
                eventLocation = fullAddress;
            }

            if (venueFormatted.Trim() == string.Empty)
            {
                venueFormatted = " - Needs a location";
                eventLocation = "TBD";
            }
            
            var description = $"{meetupEvent.StartDateLocal} from {meetupEvent.StartTimeLocal} to {meetupEvent.EndTimeLocal}{venueFormatted}";
            var startDateTime = DateTimeOffset.Parse(meetupEvent.StartDateTime);
            var endDateTime = !string.IsNullOrEmpty(meetupEvent.EndDateTime) 
                ? DateTimeOffset.Parse(meetupEvent.EndDateTime) 
                : startDateTime.AddHours(2); // Default 2-hour duration if end time not specified

            var item = new SyndicationItem(meetupEvent.Title, description, new Uri(meetupEvent.EventUrl))
            {
                PublishDate = startDateTime,
                Id = meetupEvent.id,
                Summary = new TextSyndicationContent(description)
            };
            
            // Add RSS event module extensions
            AddEventExtensions(item, startDateTime.DateTime, endDateTime.DateTime, eventLocation, eventType, meetupEvent.Group.Name);
            
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

    private void AddEventExtensions(SyndicationItem item, DateTime startDate, DateTime endDate, string? location, string eventType, string? organizer = null)
    {
        // Add event start date (ev:startdate)
        AddEventElement(item, "startdate", startDate.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        
        // Add event end date (ev:enddate)
        AddEventElement(item, "enddate", endDate.ToString("yyyy-MM-ddTHH:mm:sszzz"));
        
        // Add event location (ev:location)
        if (!string.IsNullOrEmpty(location))
        {
            AddEventElement(item, "location", location);
        }
        
        // Add event type (ev:type)
        if (!string.IsNullOrEmpty(eventType))
        {
            AddEventElement(item, "type", eventType);
        }
        
        // Organizer information (ev:organizer)
        var eventOrganizer = !string.IsNullOrEmpty(organizer) ? organizer : "Umbraco Community";
        AddEventElement(item, "organizer", eventOrganizer);
    }

    private void AddEventElement(SyndicationItem item, string elementName, string value)
    {
        const string eventNamespace = "http://purl.org/rss/1.0/modules/event/";
        var doc = new XmlDocument();
        var element = doc.CreateElement("ev", elementName, eventNamespace);
        element.InnerText = value;
        item.ElementExtensions.Add(element);
    }
}