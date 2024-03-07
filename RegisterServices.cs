using UmbraCalendar.CosmosDb;
using UmbraCalendar.Meetup;
using Umbraco.Cms.Core.Composing;

namespace UmbraCalendar;

public class RegisterServices : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddSingleton<ICosmosService, CosmosService>();
        builder.Services.AddSingleton<Options>();
        builder.Services.AddSingleton<IMeetupService, MeetupService>();
        
        
        var externalSettingsSection =  builder.Config.GetSection("ExternalServices");
        builder.Services.Configure<Options>(externalSettingsSection);
    }
}