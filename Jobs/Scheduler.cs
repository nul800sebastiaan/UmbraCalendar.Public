using Hangfire;
using UmbraCalendar.Meetup;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Services;

namespace UmbraCalendar.Jobs;

public class Scheduler : IComposer
{
    
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Components().Append<SchedulerComponent>();
    }

    public class SchedulerComponent : IComponent
    {
        private readonly IRuntimeState _runtimeState;

        public SchedulerComponent(IRuntimeState runtimeState)
        {
            _runtimeState = runtimeState;
            
        }

        public void Initialize()
        {
            if(_runtimeState.Level < RuntimeLevel.Run) return;
            
            RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import upcoming Umbraco Meetup events", x =>
                x.ImportUpcomingMeetupEvents(null), "0 */4 * * *");
            RecurringJob.AddOrUpdate<IMeetupService>($"➡️ Import historic Umbraco Meetup events", x =>
                x.ImportHistoricMeetupEvents(null), Cron.Never);
            RecurringJob.AddOrUpdate<IMeetupService>($"️➡️ Import Umbraco pro network Meetup groups", x =>
                x.ImportNetworkGroups(null), Cron.Daily());
            
            RecurringJob.AddOrUpdate<IMeetupService>($"📅 Get Upcoming Umbraco Meetup events", x =>
                x.GetUpcomingMeetupEvents(null), GetTiming("0 */2 * * *"));
            RecurringJob.AddOrUpdate<IMeetupService>($"Ⓜ️ Get pro network Meetup groups", x =>
                x.GetMeetupGroups(null), GetTiming("0 */2 * * *"));
        }
        
        private static string GetTiming(string timing)
        {
            return Environment.MachineName == "FLASH" || Environment.MachineName == "KILLERQUEEN" ? Cron.Never() : timing;
        }

        public void Terminate()
        { }
    }
}