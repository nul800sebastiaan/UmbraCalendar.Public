using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models.Groups;

public class MeetupGroup
{
    // Not annotated since CosmosDb really likes lower case `id`
    public string id { get; set; }

    [JsonPropertyName("logo")]
    public Image Logo { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("urlname")]
    public string Urlname { get; set; }

    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }

    [JsonPropertyName("groupPhoto")]
    public Image GroupPhoto { get; set; }

    [JsonPropertyName("link")]
    public string Link { get; set; }

    [JsonPropertyName("groupAnalytics")]
    public GroupAnalytics GroupAnalytics { get; set; }
    
    [JsonPropertyName("area")]
    public string Area { get; set; }
}