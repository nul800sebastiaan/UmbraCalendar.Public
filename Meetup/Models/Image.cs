using System.Text.Json.Serialization;

namespace UmbraCalendar.Meetup.Models;

public class Image
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("baseUrl")]
    public string BaseUrl { get; set; }

    public string Url => BaseUrl + Id + "/676x380.webp";
}