namespace UmbraCalendar.Calendar.Models;

public class CalendarItem
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Banner { get; set; }
    public string? Location { get; set; }
    public DateTime DateTimeFrom { get; set; }
    public DateTime DateTimeTo { get; set; }
    public string StartTimeLocal { get; set; }
    public string EndTimeLocal { get; set; }
    public string? Url { get; set; }
    public bool OnlineAvailable { get; set; }
    public List<string> Tags { get; set; }
    
    public string Source { get; set; }
    public string Organizer { get; set; }
    public string? Country { get; set; }
    public int Going { get; set; }
    
    public bool IsMultiDay => DateTimeFrom.Date != DateTimeTo.Date;
}