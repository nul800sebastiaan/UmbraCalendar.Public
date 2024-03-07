using Microsoft.Azure.Cosmos;
using UmbraCalendar.Meetup.Models.Events;
using UmbraCalendar.Meetup.Models.Groups;

namespace UmbraCalendar.CosmosDb;

public interface ICosmosService
{
    Task<ItemResponse<T>> UpsertItemAsync<T>(T item, string containerId);
    Task<List<MeetupEvent>> GetUpcomingMeetupEvents();
    Task<List<MeetupGroup>> GetMeetupGroups();
    Task<List<MeetupEvent>> GetAllMeetupEvents(DateTime startDate);
}