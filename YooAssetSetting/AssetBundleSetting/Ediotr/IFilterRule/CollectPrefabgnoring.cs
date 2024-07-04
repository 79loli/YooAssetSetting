using System.IO;
using UnityEngine;
using YooAsset.Editor;

[DisplayName("只收集预制件,排除#开头的文件")]
public class CollectPrefabgnoring : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        var path = data.AssetPath;
        return Path.GetExtension(path) == ".prefab" && !path.StartsWith("#");
    }
}
