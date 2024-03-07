using UmbraCalendar.Meetup.Models.Areas;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace UmbraCalendar.Meetup.Models.Groups;

public class AddInfoModel : MeetupAddInfo
{
    public AddInfoModel(
        IPublishedContent? content,
        IPublishedValueFallback publishedValueFallback)
        : base(content, publishedValueFallback)
    { }

    public MeetupGroup Group { get; set; }
    
    public List<MeetupArea> MeetupAreas { get; set; } 
}