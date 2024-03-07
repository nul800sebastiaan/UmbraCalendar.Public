using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using UmbraCalendar.Meetup;
using UmbraCalendar.Meetup.Models.Events;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.Cms.Core.Cache;

namespace UmbraCalendar.CosmosDb;

public class CosmosService : ICosmosService
{
    private readonly string _databaseId;
    private readonly Options _config;
    private readonly IAppPolicyCache _runtimeCache;
    private const int CacheTimespanShort = 5;
    private const int CacheTimespanMedium = 60;
    private const int CacheTimespanLong = 60 * 4;
    
    public CosmosService(
        IOptions<Options> config,
        IAppPolicyCache runtimeCache)
    {
        _runtimeCache = runtimeCache;
        _config = config.Value;
        _databaseId = "CoreCollab";
    }

    public async Task<ItemResponse<T>> UpsertItemAsync<T>(T item, string containerId)
    {
        using var client = new CosmosClient(
            _config.CosmosDbEndpoint,
            _config.CosmosDbMasterKey);

        var container = client.GetContainer(_databaseId, containerId);
        var create = await container.UpsertItemAsync(item);
        return create;
    }

    public Task<List<MeetupEvent>> GetUpcomingMeetupEvents()
    {
        const string cacheKey = "UpcomingMeetupEvents";

        return _runtimeCache.GetCacheItem(cacheKey, async () =>
        {
            using var client = new CosmosClient(
                _config.CosmosDbEndpoint,
                _config.CosmosDbMasterKey);

            var container = client.GetContainer(_databaseId, Constants.MeetupEventsContainerId);
            var startDate = DateTime.Now;
            var sql = $"SELECT * FROM c WHERE c.StartDateTime >= '{startDate:yyyy-MM-dd}'";
            var iterator = container.GetItemQueryIterator<MeetupEvent>(sql);
            var meetupEvents = new List<MeetupEvent>();
            while (iterator.HasMoreResults)
            {
                meetupEvents.AddRange(await iterator.ReadNextAsync());
            }

            return meetupEvents.ToList();
        }, TimeSpan.FromMinutes(CacheTimespanShort)) ?? Task.FromResult(new List<MeetupEvent>());
    }
    
    public Task<List<MeetupGroup>> GetMeetupGroups()
    {
        const string cacheKey = "MeetupGroups";

        return _runtimeCache.GetCacheItem(cacheKey, async () =>
        {
            using var client = new CosmosClient(
                _config.CosmosDbEndpoint,
                _config.CosmosDbMasterKey);

            var container = client.GetContainer(_databaseId, Constants.MeetupGroupsContainerId);
            const string sql = "SELECT * FROM c";
            var iterator = container.GetItemQueryIterator<MeetupGroup>(sql);
            var group = new List<MeetupGroup>();
            while (iterator.HasMoreResults)
            {
                group.AddRange(await iterator.ReadNextAsync());
            }

            return group.ToList();
        }, TimeSpan.FromMinutes(CacheTimespanShort)) ?? Task.FromResult(new List<MeetupGroup>());
    }

    public Task<List<MeetupEvent>> GetAllMeetupEvents(DateTime startDate)
    {
        var cacheKey = $"AllMeetupEvents{startDate:yyyy-MM-dd}";
        
        return _runtimeCache.GetCacheItem(cacheKey, async () =>
        {
            var allGroups = await GetMeetupGroups();
            
            using var client = new CosmosClient(
                _config.CosmosDbEndpoint,
                _config.CosmosDbMasterKey);
            
            var container = client.GetContainer(_databaseId, Constants.MeetupEventsContainerId);
            var sql = $"SELECT * FROM c WHERE c.StartDateTime >= '{startDate:yyyy-MM-dd}'";
            var iterator = container.GetItemQueryIterator<MeetupEvent>(sql);
            var meetupEvents = new List<MeetupEvent>();
            while (iterator.HasMoreResults)
            {
                meetupEvents.AddRange(await iterator.ReadNextAsync());
            }

            foreach (var meetupEvent in meetupEvents)
            {
                var group = allGroups.FirstOrDefault(x => x.Urlname.InvariantEquals(meetupEvent.Group.UrlName));
                if (group != null)
                {
                    meetupEvent.Group.Area = group.Area;
                }
                else
                {
                    meetupEvent.Group.Area = "unkown";
                }
            }
            
            return meetupEvents.ToList();
        }, TimeSpan.FromMinutes(CacheTimespanLong)) ?? Task.FromResult(new List<MeetupEvent>());
    }
}