using Apps.OpenLoc.Models.Dtos;

namespace Apps.OpenLoc.Utils;

public static class ProjectLabelSelection
{
    public static long[] WithCustomer(IEnumerable<ProjectLabelDto> existing, string customerLabelId, string customerValueId)
    {
        var labelId = InputValidation.NumericId(customerLabelId, "Customer label ID");
        var valueId = InputValidation.NumericId(customerValueId, "Customer label value ID");
        return existing.Where(x => InputValidation.NumericId(x.LabelId, "Label ID") != labelId)
            .Select(x => InputValidation.NumericId(x.ValueId, "Label value ID"))
            .Append(valueId).Distinct().ToArray();
    }
}
