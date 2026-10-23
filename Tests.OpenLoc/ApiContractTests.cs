using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Models.Responses;
using Apps.OpenLoc.Utils;
using Blackbird.Applications.Sdk.Common.Exceptions;
using Newtonsoft.Json;

namespace Tests.OpenLoc;

[TestClass]
public class ApiContractTests
{
    [TestMethod]
    public void NumericIds_PreserveInt64PrecisionAsText()
    {
        const string id = "9223372036854775807";
        var dto = JsonConvert.DeserializeObject<ProjectDto>(
            """{"id":9223372036854775807,"created_by":9223372036854775806,"new_field":"ignored"}""")!;
        var result = new ProjectResponse(dto);
        Assert.AreEqual(id, result.ProjectId);
        Assert.AreEqual("9223372036854775806", result.CreatedByUserId);
        Assert.AreEqual(id, InputValidation.IdPath(result.ProjectId, "Project ID"));
    }

    [TestMethod]
    public void PaginationEnvelope_ReadsCursorAndEmptyLastPage()
    {
        var page = JsonConvert.DeserializeObject<PageEnvelope<JobDto>>(
            """{"data":[],"cursor":0,"has_more":false}""")!;
        Assert.IsNotNull(page.Data);
        Assert.AreEqual(0, page.Data.Count);
        Assert.AreEqual(0L, page.Cursor);
        Assert.AreEqual(false, page.HasMore);
    }

    [TestMethod]
    public void SegmentResponse_PreservesUntranslatedAndMissingHistory()
    {
        var dto = JsonConvert.DeserializeObject<SegmentDto>(
            """{"id":42,"source_text":"Hello","target_text":null,"previous_step":null,"tm_match_percent":null,"status":"needs_review","position":0}""")!;
        var result = new SegmentResponse(dto);
        Assert.AreEqual("42", result.SegmentId);
        Assert.AreEqual("Hello", result.SourceText);
        Assert.IsNull(result.TargetText);
        Assert.IsNull(result.PreviousStep);
        Assert.IsNull(result.TmMatchPercent);
    }

    [DataTestMethod]
    [DataRow("")]
    [DataRow("-1")]
    [DataRow("1/segments")]
    [DataRow("9223372036854775808")]
    public void InvalidIds_ExplainHowToCorrectInput(string id)
    {
        var error = Assert.ThrowsException<PluginMisconfigurationException>(() => InputValidation.IdPath(id, "Project ID"));
        StringAssert.Contains(error.Message, "Project ID");
    }

    [TestMethod]
    public void Deadline_NormalizesLocalTimeAndTreatsUnspecifiedAsUtc()
    {
        var local = new DateTime(2026, 10, 23, 12, 0, 0, DateTimeKind.Local);
        Assert.AreEqual(local.ToUniversalTime(), InputValidation.UtcDeadline(local));
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        var utc = InputValidation.UtcDeadline(unspecified);
        Assert.AreEqual(DateTimeKind.Utc, utc.Kind);
        Assert.AreEqual(unspecified.Ticks, utc.Ticks);
    }
}
