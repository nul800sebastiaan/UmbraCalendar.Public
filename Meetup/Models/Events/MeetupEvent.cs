using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models.Events;

public class MeetupEvent
{
    // Not annotated since CosmosDb really likes lower case `id`
    public string id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("dateTime")]
    public string StartDateTime { get; set; }
    public string StartTimeLocal => StartDateTime.Split("T").Skip(1).First().Substring(0, 5);
    public string StartDateLocal => StartDateTime.Split("T").First();
    [JsonPropertyName("duration")]
    public string Duration { get; set; }
    [JsonPropertyName("endTime")]
    public string EndDateTime { get; set; }
    public string EndTimeLocal => EndDateTime.Split("T").Skip(1).First().Substring(0, 5);
    public string EndDateLocal => EndDateTime.Split("T").First();
    [JsonPropertyName("eventUrl")]
    public string EventUrl { get; set; }
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("going")]
    public int Going { get; set; }
    [JsonPropertyName("isOnline")]
    public bool IsOnline { get; set; }
    [JsonPropertyName("group")]
    public Group Group { get; set; }
    [JsonPropertyName("eventType")]
    // ONLINE | PHYSICAL
    public string EventType { get; set; }
    [JsonPropertyName("venue")]
    public Venue? Venue { get; set; }
    [JsonPropertyName("onlineVenue")]
    public OnlineVenue? OnlineVenue { get; set; }
    [JsonPropertyName("image")]
    public Image? Image { get; set; }
    
    [JsonPropertyName("tickets")]
    public Tickets Tickets { get; set; }
}

public class Tickets
{
    [JsonPropertyName("pageInfo")]
    public PageInfo PageInfo { get; set; }

    [JsonPropertyName("edges")]
    public List<Edge> Edges { get; set; }
}


public class Edge
{
    [JsonPropertyName("node")]
    public Node Node { get; set; }
}


public class Node
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("user")]
    public User User { get; set; }
}

public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("zip")]
    public object Zip { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("bio")]
    public string Bio { get; set; }

    [JsonPropertyName("memberPhoto")]
    public MemberPhoto MemberPhoto { get; set; }
}

public class MemberPhoto
{
    [JsonPropertyName("source")]
    public string Source { get; set; }
}