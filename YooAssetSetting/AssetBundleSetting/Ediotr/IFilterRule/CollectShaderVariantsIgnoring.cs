using System.IO;
using YooAsset.Editor;

[DisplayName("只收集shader变体,排除#开头的文件")]
public class CollectShaderVariantsIgnoring : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        var path = data.AssetPath;
        return Path.GetExtension(data.AssetPath) == ".shadervariants" && !path.StartsWith("#");
    }
}
