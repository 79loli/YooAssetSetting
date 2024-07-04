using System.IO;
using UnityEngine;
using YooAsset.Editor;

[DisplayName("只收集场景,排除#开头的文件")]
public class CollectSceneIgnoring : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        var path = data.AssetPath;
        return Path.GetExtension(path) == ".unity" && !path.StartsWith("#");
    }
}
