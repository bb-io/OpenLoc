using Apps.OpenLoc.Handlers.Static;
using Apps.OpenLoc.Handlers;
using Apps.OpenLoc.Models.Identifiers;
using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Dictionaries;
using Blackbird.Applications.Sdk.Common.Dynamic;

namespace Apps.OpenLoc.Models.Requests;

public class SearchJobsRequest : ProjectIdentifier
{
    [Display("File name", Description = "Only include file names containing this text.")]
    public string? FileName { get; set; }

    [Display("Status", Description = "Only include jobs with this translation status."), StaticDataSource(typeof(JobStatusDataSourceHandler))]
    public string? Status { get; set; }

    [Display("Processing status", Description = "Only include this automatic processing state, for example queued, in_progress, completed, or failed.")]
    public string? ProcessingStatus { get; set; }

    [Display("Source language", Description = "Only include this source language code, for example en.")]
    public string? SourceLanguage { get; set; }

    [Display("Target language", Description = "Only include this target language code, for example de.")]
    public string? TargetLanguage { get; set; }

    [Display("Assigned user ID", Description = "Only include jobs whose current workflow step is assigned to this user."), DataSource(typeof(MemberDataSourceHandler))]
    public string? AssigneeId { get; set; }
}
