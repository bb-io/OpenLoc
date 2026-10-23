namespace Apps.OpenLoc.Models.Polling;

public class ProjectStatusMemory
{
    public string? ProjectId { get; set; }
    public string? StatusFilter { get; set; }
    public Dictionary<string, string> Statuses { get; set; } = [];
}
