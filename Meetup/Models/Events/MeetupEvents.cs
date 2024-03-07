using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models.Events;

public class MeetupEvents
{
    [JsonPropertyName("data")]
    public Data Data { get; set; }
}

public class Data
{
    [JsonPropertyName("proNetworkByUrlname")]
    public ProNetworkByUrlname MeetupNetwork { get; set; }
}

public class ProNetworkByUrlname
{
    [JsonPropertyName("eventsSearch")]
    public EventsSearch EventsSearch { get; set; }
}

public class EventsSearch
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; }
    [JsonPropertyName("edges")]
    public Edges[] Edges { get; set; }
}

public class Edges
{
    [JsonPropertyName("node")]
    public MeetupEvent MeetupEvent { get; set; }
}

public class Group
{
    [JsonPropertyName("groupPhoto")]
    public Image Image { get; set; }
    [JsonPropertyName("country")]
    public string Country { get; set; }
    [JsonPropertyName("city")]
    public string City { get; set; }
    [JsonPropertyName("urlname")]
    public string UrlName { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("timezone")]
    public string Timezone { get; set; }
    public string Area { get; set; }
}


public class OnlineVenue
{
    [JsonPropertyName("type")]
    // GOOGLEHANGOUT | ZOOM | SKYPE | OTHER
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}

public class Venue
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("address")]
    public string Address { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("postalCode")]
    public string PostalCode { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("lat")]
    public double Latitude { get; set; }

    [JsonPropertyName("lng")]
    public double Longitude { get; set; }
}
