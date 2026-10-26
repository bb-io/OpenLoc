using Apps.OpenLoc.Models.Dtos;
using Apps.OpenLoc.Utils;
using Newtonsoft.Json;

namespace Tests.OpenLoc;

[TestClass]
public class ProjectLabelSelectionTests
{
    [TestMethod]
    public void CustomerReplacement_PreservesDomainAndCustomLabelsInheritedFromTemplate()
    {
        var existing = JsonConvert.DeserializeObject<PageEnvelope<ProjectLabelDto>>(
            """{"data":[{"label_id":1,"value_id":10,"label_name":"Customer","value":"Old customer"},{"label_id":2,"value_id":20,"label_name":"Domain","value":"Medical"},{"label_id":3,"value_id":30,"label_name":"Region","value":"Europe"}],"cursor":0,"has_more":false}""")!;
        var values = ProjectLabelSelection.WithCustomer(existing.Data!, "1", "11");
        CollectionAssert.AreEquivalent(new long[] { 11, 20, 30 }, values);
        CollectionAssert.DoesNotContain(values, 10L);
    }

    [TestMethod]
    public void CustomerAssignment_AddsToEmptySelectionAndPreservesInt64Ids()
    {
        const string id = "9223372036854775807";
        CollectionAssert.AreEqual(new[] { long.MaxValue }, ProjectLabelSelection.WithCustomer([], "1", id));
        var labels = JsonConvert.DeserializeObject<DataEnvelope<LabelDto>>(
            """{"data":{"id":1,"key":"customer","name":"Localized customer name","values":[{"id":9223372036854775807,"value":"Example customer"}]}}""")!;
        Assert.AreEqual("customer", labels.Data!.Key);
        Assert.AreEqual(id, labels.Data.Values.Single().Id);
    }
}
