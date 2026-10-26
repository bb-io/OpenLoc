using Newtonsoft.Json;

namespace Apps.OpenLoc.Models.Dtos;

public class QaBlockedDto
{
    [JsonProperty("error")] public ApiErrorDto? Error { get; set; }
    [JsonProperty("qa_issues")] public List<QaIssueDto>? QaIssues { get; set; }
}
