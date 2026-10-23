using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
namespace Apps.OpenLoc.Models.Requests;
public class SearchProjectsRequest
{
    [Display("Project name", Description = "Find project names containing this text.")]
    public string? SearchText { get; set; }
    [Display("Status", Description = "Only include projects with this status."), StaticDataSource(typeof(ProjectStatusDataSourceHandler))]
    public string? Status { get; set; }
    [Display("Source language", Description = "Filter by source language code, for example en.")]
    public string? SourceLanguage { get; set; }
    [Display("Target language", Description = "Filter by a target language code, for example de.")]
    public string? TargetLanguage { get; set; }
}
