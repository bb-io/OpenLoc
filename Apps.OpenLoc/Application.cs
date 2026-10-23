using Blackbird.Applications.Sdk.Common;
using Blackbird.Applications.Sdk.Common.Metadata;

namespace Apps.OpenLoc;

public class Application : IApplication, ICategoryProvider
{
    public string Name => "OpenLoc";
    public IEnumerable<ApplicationCategory> Categories
    {
        get => [ApplicationCategory.CatAndTms];
        set { }
    }
    public T GetInstance<T>() => throw new NotImplementedException();
}

