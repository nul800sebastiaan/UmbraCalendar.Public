using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire.Console;
using Hangfire.Server;
using UmbraCalendar.CosmosDb;
using UmbraCalendar.Meetup.Models.Areas;
using UmbraCalendar.Meetup.Models.Events;
using UmbraCalendar.Meetup.Models.Groups;
using Umbraco.AuthorizedServices.Services;

namespace UmbraCalendar.Meetup;

public class MeetupService : IMeetupService
{
    private readonly IAuthorizedServiceCaller _authorizedServiceCaller;
    private readonly ICosmosService _cosmosService;
    private readonly IWebHostEnvironment _hostingEnvironment;

    public MeetupService(IAuthorizedServiceCaller authorizedServiceCaller,
	    ICosmosService cosmosService,
	    IWebHostEnvironment hostingEnvironment)
    {
	    _authorizedServiceCaller = authorizedServiceCaller;
	    _cosmosService = cosmosService;
	    _hostingEnvironment = hostingEnvironment;
    }

    public async void GetUpcomingMeetupEvents(PerformContext context)
    {
	    var events = await _cosmosService.GetUpcomingMeetupEvents();
	    var test = events;
    }
    
    public async void GetMeetupGroups(PerformContext context)
    {
	    var groups = await _cosmosService.GetMeetupGroups();
	    var test = groups;
    }
    
    public void ImportUpcomingMeetupEvents(PerformContext context)
    {
        context.WriteLine("Starting");

        const string query = """
                             query($urlname: String!) {
                             	 proNetworkByUrlname(urlname: $urlname) {
                             	   eventsSearch(filter: { status: UPCOMING }, input: { first: 100 }) {
                             		 count
                             		 pageInfo { endCursor hasNextPage }
                             		 edges {
                             		   node {
                             		     id
                             			 title
                             			 dateTime
                             			 duration
                             			 endTime
                             			 eventUrl
                             			 description
                             			 going
                             			 isOnline
                             			 eventType
                             			 venue { name address city state postalCode country lat lng }
                             			 onlineVenue { type url }
                             			 group { groupPhoto { id baseUrl } country city urlname name timezone }
                             			 image { id baseUrl }
                             			 tickets {
                             			   pageInfo {
                             			     hasNextPage
                             			     endCursor
                             			   }
                             			   edges {
                             			     node {
                             			       id
                             			       user {
                             			         id
                             			         name
                             			         city
                             			         state
                             			         zip
                             			         country
                             			         bio
                             			         memberPhoto {
                             			           source
                             			         }
                                               }
                             			     }
                             			   }
                             			 }			 
                             		   }
                             		 }
                             	   }
                             	 }
                             }
                             """;

        const string variables = """
                                 {
                                   "urlname": "umbraco"
                                 }
                                 """;

        var requestContent = new MeetupRequest
        {
            Query = query,
            Variables = variables
        };
        
        // var responseRaw = _authorizedServiceCaller.SendRequestRawAsync(
	       //  "meetup",
	       //  "/gql",
	       //  HttpMethod.Post,
	       //  requestContent
        // ).Result;
        //
        // var res = responseRaw;

        // Can't await this one because the context.Writeline doesn't work then
        var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, MeetupEvents>(
	        "meetup",
	        "/gql",
	        HttpMethod.Post,
	        requestContent
        ).Result;

        if (response.Success && response.Result != null)
        {
	        foreach (var edge in response.Result.Data.MeetupNetwork.EventsSearch.Edges)
	        {
		        var meetupEvent = edge.MeetupEvent;
		        context.WriteLine(
			        $"Meetup group {meetupEvent.Group.UrlName} has an event in {meetupEvent.Venue?.Name ?? "[online?]"} on {meetupEvent.StartDateLocal} titled {meetupEvent.Title} - more info: {meetupEvent.EventUrl}");
		        var result = _cosmosService.UpsertItemAsync(meetupEvent, Constants.MeetupEventsContainerId).Result;
	        }
        }
        else
        {
	        context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
        }
    }
    
    public void ImportHistoricMeetupEvents(PerformContext context)
    {
	    var hasNextPage = true;
	    var endCursor = "null";
	    while (hasNextPage)
	    {
		    var response = GetMeetupEvents(context, endCursor);
		    if(response == null)
		    {
			    context.WriteLine("Response for event search was null, no events found");
			    return;
		    }
		    
		    foreach (var edge in response.Edges)
		    {
			    var meetupEvent = edge.MeetupEvent;
			    context.WriteLine($"Meetup group {meetupEvent.Group.UrlName} has an event in {meetupEvent.Venue?.Name ?? "[online?]"} on {meetupEvent.StartDateLocal} titled {meetupEvent.Title} - more info: {meetupEvent.EventUrl}");
			    var result = _cosmosService.UpsertItemAsync(meetupEvent, Constants.MeetupEventsContainerId).Result;
		    }

		    hasNextPage = response.PageInfo.HasNextPage;
		    endCursor = response.PageInfo.EndCursor;
	    }
    }

    private EventsSearch? GetMeetupEvents(PerformContext context, string? endCursor)
    {
	    const string query = """
	                         query($urlname: String!, $cursor: String) {
	                         	 proNetworkByUrlname(urlname: $urlname) {
	                         	   eventsSearch(filter: { status: PAST }, input: { first: 100, after: $cursor }) {
	                         		 count
	                         		 pageInfo { endCursor hasNextPage }
	                         		 edges {
	                         		   node {
	                         		     id
	                         			 title
	                         			 dateTime
	                         			 duration
	                         			 endTime
	                         			 eventUrl
	                         			 description
	                         			 going
	                         			 isOnline
	                         			 eventType
	                         			 venue { name address city state postalCode country lat lng }
	                         			 onlineVenue { type url }
	                         			 group { groupPhoto { id baseUrl } country city urlname name timezone }
	                         			 image { id baseUrl }
	                         			 tickets {
	                         			   pageInfo {
	                         			     hasNextPage
	                         			     endCursor
	                         			   }
	                         			   edges {
	                         			     node {
	                         			       id
	                         			       user {
	                         			         id
	                         			         name
	                         			         city
	                         			         state
	                         			         zip
	                         			         country
	                         			         bio
	                         			         memberPhoto {
	                         			           source
	                         			         }
	                                           }
	                         			     }
	                         			   }
	                         			 }			 	                         			 
	                         		   }
	                         		 }
	                         	   }
	                         	 }
	                         }
	                         """;

	    var variables = $$"""
	                      {
	                        "urlname": "umbraco",
	                        "cursor": "{{endCursor}}"
	                      }
	                      """;

	    var requestContent = new MeetupRequest
	    {
		    Query = query,
		    Variables = variables
	    };


	    // var responseRaw = _authorizedServiceCaller.SendRequestRawAsync(
	    //  "meetup",
	    //  "/gql",
	    //  HttpMethod.Post,
	    //  requestContent
	    // ).Result;
	    //
	    // var res = responseRaw;

	    // Can't await this one because the context.Writeline doesn't work then
	    var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, MeetupEvents>(
		    "meetup",
		    "/gql",
		    HttpMethod.Post,
		    requestContent
	    ).Result;

	    if (response.Success && response.Result != null)
	    {
		    return response.Result.Data.MeetupNetwork.EventsSearch;		    
	    }
	    else
	    {
		    context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
		    return null;
	    }
    }
    
    public void ImportNetworkGroups(PerformContext context)
    {
        const string query = """
                             query ($urlname: String!) {
                               proNetworkByUrlname(urlname: $urlname) {
                                 groupsSearch(input: {first: 100}) {
                                   count
                                   pageInfo { endCursor hasNextPage }
                                   edges {
                                     node {
                                       id
                                       logo { id baseUrl }
                                       name
                                       urlname
                                       timezone
                                       groupPhoto { id baseUrl }
                                       link
                                       groupAnalytics { totalMembers totalPastEvents }
                                     }
                                   }
                                 }
                               }
                             }
                             """;

        const string variables = """
                                 {
                                   "urlname": "umbraco"
                                 }
                                 """;

        var requestContent = new MeetupRequest
        {
            Query = query,
            Variables = variables
        };
	      
        // Can't await this one because the context.Writeline doesn't work then
        var response = _authorizedServiceCaller.SendRequestAsync<MeetupRequest, Groups>(
	        "meetup",
	        "/gql",
	        HttpMethod.Post,
	        requestContent
        ).Result;

        if (response.Success && response.Result != null)
        {
	        var existingGroups = _cosmosService.GetMeetupGroups().Result;
			foreach (var edge in response.Result.Data.ProNetworkByUrlname.GroupsSearch.Edges)
	        {
		        var group = edge.MeetupGroup;
		        context.WriteLine($"Processing Meetup group {group.Name} with {group.GroupAnalytics.TotalMembers} member and {group.GroupAnalytics.TotalPastEvents} past events");
		        var existingGroup = existingGroups.FirstOrDefault(x => x.id == group.id);
		        
		        // Preserve added metadata
		        if (existingGroup != null && !string.IsNullOrWhiteSpace(existingGroup.Area))
		        {
			        group.Area = existingGroup.Area;
		        }
		        var save = _cosmosService.UpsertItemAsync(group, Constants.MeetupGroupsContainerId).Result;
	        }
        }
        else
        {
	        context.WriteLine($"Something went wrong, error: '{response.Exception?.Message}', trace: '{response.Exception?.StackTrace}'");
        }
    }
    
    public async Task<List<MeetupArea>> GetAvailableAreas()
    {
	    var webRoot = _hostingEnvironment.WebRootPath;
	    var areasConfig = webRoot + "/Config/MeetupAreas.json";
	    await using var openStream = File.OpenRead(areasConfig);
	    var areas = await JsonSerializer.DeserializeAsync<List<MeetupArea>>(openStream);
	    return areas;
    }
}

public class MeetupRequest
{
    [JsonPropertyName("query")] public string Query { get; set; } = string.Empty;

    [JsonPropertyName("variables")] public string? Variables { get; set; } = null;

    [JsonPropertyName("operationName")] public string? OperationName { get; set; } = null;
}