using System.IO;
using YooAsset.Editor;

[DisplayName("只收集shader,排除#开头的文件")]
public class CollectShaderIgnoring : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        var path = data.AssetPath;
        return Path.GetExtension(data.AssetPath) == ".shader" && !path.StartsWith("#");
    }
}
