using System.IO;
using UnityEngine;
using YooAsset.Editor;

[DisplayName("收集所有资源,排除#开头的文件")]
public class CollectAllIgnoring : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        var path = data.AssetPath;
        return !path.StartsWith("#");
    }
}
