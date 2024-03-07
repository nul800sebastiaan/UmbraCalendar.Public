using UmbraCalendar.Meetup.Models.Areas;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Meetup.Models.Groups;

public class AddInfoOverviewModel : MeetupAddInfo
{
    public AddInfoOverviewModel(
        IPublishedContent? content,
        IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    { }

    public List<MeetupGroup>? MeetupGroups { get; set; }
    
    public List<MeetupArea>? MeetupAreas { get; set; }
}