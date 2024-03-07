using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models;

public class PageInfo
{
    [JsonPropertyName("endCursor")]
    public string EndCursor { get; set; }
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }
}
