using UnityEngine;
using UnityEditor;
using YooAsset.Editor;
using System.Threading.Tasks;
using System;
using System.IO;
using System.Collections.Generic;

public class FolderYooAssetBundleWindow : EditorWindow
{
    private const string AssetBundleCollectorSettingPath = "Assets/YooAssetSetting/AssetBundleSetting/AssetBundleCollectorSetting.asset";
    AssetBundleCollectorSetting setting;

    private string oldPath;
    private string newPath;
    private bool isOldHas;
    private bool isNewHas;

    //包裹
    private string[] packageNames;
    private int[] packageNameOptions;
    private int selectedPackageIndex = 0;
    //Group分组
    private string[] groupNames;
    private int[] groupNameOptions;
    private int selectedGroupIndex = 0;
    //
    private ECollectorType collectorType = ECollectorType.MainAssetCollector;
    //寻址规则
    private string[] AddressRuleNames;
    private int[] AddressRuleNameOptions;
    private int selectedAddressRuleIndex = 0;
    //打包规则
    private string[] PackRuleNames;
    private int[] PackRuleNameOptions;
    private int selectedPackRuleIndex = 1;
    //过滤规则
    private string[] FilterRuleNames;
    private int[] FilterRuleOptions;
    private int selectedFilterRuleIndex = 0;
    //用户自定义数据
    private string userData = string.Empty;
    //tag
    private string assetTags = string.Empty;

    public static void OnWillMoveAsset(string oldPath, string newPath, bool isCreate = false)
    {
        string oldName = Path.GetFileNameWithoutExtension(oldPath);
        string newName = Path.GetFileNameWithoutExtension(newPath);
        var isOldHas = isCreate ? false : oldName.StartsWith("$");
        var isNewHas = newName.StartsWith("$");
        if (isNewHas)
        {
            if (isOldHas)
            {
                ReNameColecotr(oldPath, newPath);
            }
            else
            {

                ShowWindow(oldPath, newPath, isOldHas, isNewHas);
            }
        }
        else if (isOldHas && isNewHas == false)
        {
            RemoveColector(oldPath);
        }
    }

    public static void ShowResetWindow(string path)
    {
        FolderYooAssetBundleWindow window = CreateInstance<FolderYooAssetBundleWindow>();
        window.titleContent = new GUIContent("Bundle配置");
        window.oldPath = path;
        window.newPath = path;
        window.isOldHas = true;
        window.isNewHas = true;
        window.OnShowResetWindow();
        window.Show();
    }

    private static void ShowWindow(string oldPath, string newPath, bool isOldHas, bool isNewHas)
    {
        FolderYooAssetBundleWindow window = CreateInstance<FolderYooAssetBundleWindow>();
        window.titleContent = new GUIContent("Bundle配置");
        window.oldPath = oldPath;
        window.newPath = newPath;
        window.isOldHas = isOldHas;
        window.isNewHas = isNewHas;
        window.OnShow();
        window.Show();
    }

    public void OnShowResetWindow()
    {
        setting = LoadSettingData();
        var folderGUID = AssetDatabase.AssetPathToGUID(oldPath);
        var bundleDataList = FindBundleDataList(setting, folderGUID);
        if (bundleDataList.Count <= 0)
        {
            bool userClickedOK = EditorUtility.DisplayDialog("修改bundle错误", $"不存在:{oldPath}", "确定");
            return;
        }
        else if (bundleDataList.Count > 1)
        {
            bool userClickedOK = EditorUtility.DisplayDialog("修改bundle错误", $"存在多个:{oldPath}", "确定");
            return;
        }
        InitAllEnumDisPlay();
        RefreshGroupUI();
        var bundleData = bundleDataList[0];
        for (var i = 0; i < packageNames.Length; i++)
        {
            if (bundleData.Package.PackageName == packageNames[i])
            {
                selectedPackageIndex = i;
            }
        }
        for (var i = 0; i < groupNames.Length; i++)
        {
            if (bundleData.group.GroupName == groupNames[i])
            {
                selectedGroupIndex = i;
            }
        }
        collectorType = bundleData.collector.CollectorType;
        for (var i = 0; i < AddressRuleNames.Length; i++)
        {
            if (bundleData.collector.AddressRuleName == ((AddressRuleEnum)i).ToString())
            {
                selectedAddressRuleIndex = i;
            }
        }
        for (var i = 0; i < PackRuleNames.Length; i++)
        {
            if (bundleData.collector.PackRuleName == ((PackRuleNameEnum)i).ToString())
            {
                selectedPackRuleIndex = i;
            }
        }
        for (var i = 0; i < FilterRuleNames.Length; i++)
        {
            if (bundleData.collector.FilterRuleName == ((FilterRuleEnum)i).ToString())
            {
                selectedFilterRuleIndex = i;
            }
        }
        userData = bundleData.collector.UserData;
        assetTags = bundleData.collector.AssetTags;
    }

    public void OnShow()
    {
        setting = LoadSettingData();
        InitAllEnumDisPlay();
        RefreshGroupUI();
        OnFirstCreate();
    }

    void InitAllEnumDisPlay()
    {
        InitAddressRuleNamesDisPlay();
        InitPackRuleNamesDisPlay();
        InitFilterRuleNamesDisPlay();
    }

    void InitAddressRuleNamesDisPlay()
    {
        AddressRuleNames = GetAllDisplayNames<AddressRuleEnum>();
        var values = Enum.GetValues(typeof(AddressRuleEnum));
        AddressRuleNameOptions = new int[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            AddressRuleNameOptions[i] = i;
        }
    }

    void InitPackRuleNamesDisPlay()
    {
        PackRuleNames = GetAllDisplayNames<PackRuleNameEnum>();
        var values = Enum.GetValues(typeof(PackRuleNameEnum));
        PackRuleNameOptions = new int[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            PackRuleNameOptions[i] = i;
        }
    }

    void InitFilterRuleNamesDisPlay()
    {
        FilterRuleNames = GetAllDisplayNames<FilterRuleEnum>();
        var values = Enum.GetValues(typeof(FilterRuleEnum));
        FilterRuleOptions = new int[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            FilterRuleOptions[i] = i;
        }
    }

    public void OnFirstCreate()
    {
        //如果是赋值美分号，提前创建一个默认bundle，避免没有后续操作导致文件夹含有美分号，却没有分配Bundle
        if (isOldHas == false && isNewHas)
        {
            var folderGUID = AssetDatabase.AssetPathToGUID(oldPath);
            CreateColector(folderGUID);
        }
    }

    void RefreshGroupUI()
    {
        var length = setting.Packages.Count;
        packageNames = new string[length];
        packageNameOptions = new int[length];
        for (var i = 0; i < length; i++)
        {
            packageNames[i] = setting.Packages[i].PackageName;
            packageNameOptions[i] = i;
        }

        var package = setting.Packages[selectedPackageIndex];
        length = package.Groups.Count;
        groupNames = new string[length];
        groupNameOptions = new int[length];
        for (var i = 0; i < length; i++)
        {
            groupNames[i] = package.Groups[i].GroupName;
            groupNameOptions[i] = i;
        }
    }

    void OnGUI()
    {
        if (isOldHas == false && isNewHas)
        {
            //创建Bundle
            RefreshCreateColectorUI();
        }
        else if (isOldHas && isNewHas)
        {
            //修改Bundle
            RefreshResetColectorUI();
        }
        GUILayout.Label("使用规则：", EditorStyles.boldLabel);
        GUILayout.Label("   同一个文件夹只能打一个bundle,避免无法通过路径识别bundle配置", EditorStyles.boldLabel);
        GUILayout.Label("   对文件夹添加$,会创建默认Bundle,你可以在弹出的窗口中修改它", EditorStyles.boldLabel);
        GUILayout.Label("   无法监听到复制名称带有$的文件夹的操作", EditorStyles.boldLabel);
    }

    void RefreshResetColectorUI()
    {
        GUILayout.Label($"修改Bundle:{newPath}", EditorStyles.boldLabel);
        RefreshMainUI();
        if (GUILayout.Button("修改Bundle"))
        {
            ResetColector();
        }
    }

    void RefreshCreateColectorUI()
    {
        GUILayout.Label($"创建Bundle:{newPath}", EditorStyles.boldLabel);

        RefreshMainUI();
        if (GUILayout.Button("创建Bundle"))
        {
            var folderGUID = AssetDatabase.AssetPathToGUID(newPath);
            CreateColector(folderGUID);
        }
    }

    void RefreshMainUI()
    {
        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("包裹名称:", EditorStyles.boldLabel);
            var oldIndex = selectedPackageIndex;
            selectedPackageIndex = EditorGUILayout.IntPopup(selectedPackageIndex, packageNames, packageNameOptions);
            if (oldIndex != selectedPackageIndex)
            {
                RefreshGroupUI();
            }
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("分组名称:", EditorStyles.boldLabel);
            selectedGroupIndex = EditorGUILayout.IntPopup(selectedGroupIndex, groupNames, groupNameOptions);
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("收集器类型:", EditorStyles.boldLabel);
            collectorType = (ECollectorType)EditorGUILayout.EnumPopup(collectorType);
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("可寻址规则:", EditorStyles.boldLabel);
            selectedAddressRuleIndex = EditorGUILayout.IntPopup(selectedAddressRuleIndex, AddressRuleNames, AddressRuleNameOptions);
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("打包规则:", EditorStyles.boldLabel);
            selectedPackRuleIndex = EditorGUILayout.IntPopup(selectedPackRuleIndex, PackRuleNames, PackRuleNameOptions);
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("过滤规则:", EditorStyles.boldLabel);
            selectedFilterRuleIndex = EditorGUILayout.IntPopup(selectedFilterRuleIndex, FilterRuleNames, FilterRuleOptions);
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("用户自定义数据:", EditorStyles.boldLabel);
            userData = EditorGUILayout.TextField("UserData", userData);
            EditorGUILayout.EndHorizontal();
        }
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("资源分类标签列表:", EditorStyles.boldLabel);
            assetTags = EditorGUILayout.TextField("AssetTags", assetTags);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    void CreateColector(string folderGUID)
    {
        var packageName = packageNames[selectedPackageIndex];
        var groupName = groupNames[selectedGroupIndex];
        AssetBundleCollectorGroup tagetGroup = FindGroup(packageName, groupName);
        AssetBundleCollector targetCollector = null;
        foreach (AssetBundleCollector collector in tagetGroup.Collectors)
        {
            if (folderGUID == collector.CollectorGUID)
            {
                targetCollector = collector;
                break;
            }
        }
        if (targetCollector != null)
        {
            targetCollector.CollectPath = newPath;
            targetCollector.CollectorType = collectorType;
            targetCollector.AddressRuleName = ((AddressRuleEnum)selectedAddressRuleIndex).ToString();
            targetCollector.PackRuleName = ((PackRuleNameEnum)selectedPackRuleIndex).ToString();
            targetCollector.FilterRuleName = ((FilterRuleEnum)selectedFilterRuleIndex).ToString();
            targetCollector.AssetTags = assetTags;
            targetCollector.UserData = userData;
        }
        else
        {
            AssetBundleCollector newCollector = CreateNewCollector(folderGUID);
            tagetGroup.Collectors.Add(newCollector);
        }
        AssetDatabase.Refresh();
    }

    void ResetColector()
    {
        RemoveColector(oldPath);
        var folderGUID = AssetDatabase.AssetPathToGUID(oldPath);
        var packageName = packageNames[selectedPackageIndex];
        var groupName = groupNames[selectedGroupIndex];
        AssetBundleCollectorGroup group = FindGroup(packageName, groupName);
        AssetBundleCollector newCollector = CreateNewCollector(folderGUID);
        group.Collectors.Add(newCollector);
        AssetDatabase.Refresh();
    }

    static void ReNameColecotr(string oldPath, string newPath)
    {
        var setting = LoadSettingData();
        var folderGUID = AssetDatabase.AssetPathToGUID(oldPath);
        var bundleDataList = FindBundleDataList(setting, folderGUID);
        if (bundleDataList.Count <= 0)
        {
            bool userClickedOK = EditorUtility.DisplayDialog("重命名bundle错误", $"不存在:{oldPath}", "关闭");
            return;
        }
        foreach (var bundleData in bundleDataList)
        {
            bundleData.collector.CollectPath = newPath;
        }
    }

    public static void RemoveColector(string oldPath)
    {
        var setting = LoadSettingData();
        var folderGUID = AssetDatabase.AssetPathToGUID(oldPath);
        var bundleDataList = FindBundleDataList(setting, folderGUID);
        if (bundleDataList.Count <= 0)
        {
            bool userClickedOK = EditorUtility.DisplayDialog("删除bundle错误", $"不存在:{oldPath}", "确定");
            return;
        }
        foreach (var bundleData in bundleDataList)
        {
            bundleData.group.Collectors.Remove(bundleData.collector);
        }
        AssetDatabase.Refresh();
    }

    AssetBundleCollector CreateNewCollector(string folderGUID)
    {
        AssetBundleCollector newCollector = new AssetBundleCollector()
        {
            CollectPath = newPath,
            CollectorGUID = folderGUID,
            CollectorType = collectorType,
            AddressRuleName = ((AddressRuleEnum)selectedAddressRuleIndex).ToString(),
            PackRuleName = ((PackRuleNameEnum)selectedPackRuleIndex).ToString(),
            FilterRuleName = ((FilterRuleEnum)selectedFilterRuleIndex).ToString(),
            AssetTags = assetTags,
            UserData = userData
        };
        return newCollector;
    }

    AssetBundleCollectorGroup FindGroup(string packageName, string groupName)
    {
        AssetBundleCollectorPackage tagetPackage = null;
        foreach (AssetBundleCollectorPackage package in setting.Packages)
        {
            if (package.PackageName == packageName)
            {
                tagetPackage = package;
                break;
            }
        }

        if (tagetPackage == null)
        {
            bool userClickedOK = EditorUtility.DisplayDialog("添加bundle错误", $"package不存在:{packageName}", "确定");
            return null;
        }

        AssetBundleCollectorGroup tagetGroup = null;
        foreach (AssetBundleCollectorGroup group in tagetPackage.Groups)
        {
            if (group.GroupName == groupName)
            {
                tagetGroup = group;
                break;
            }
        }
        if (tagetGroup == null)
        {
            bool userClickedOK = EditorUtility.DisplayDialog("添加bundle错误", $"Group不存在:{groupName}", "确定");
            return null;
        }
        return tagetGroup;
    }

    private static List<BundleData> FindBundleDataList(AssetBundleCollectorSetting setting, string folderGUID)
    {
        List<BundleData> bundleList = new List<BundleData>();
        foreach (AssetBundleCollectorPackage package in setting.Packages)
        {
            foreach (AssetBundleCollectorGroup group in package.Groups)
            {
                foreach (AssetBundleCollector collector in group.Collectors)
                {
                    if (collector.CollectorGUID == folderGUID)
                    {
                        bundleList.Add(new BundleData() { Package = package, group = group, collector = collector });
                    }
                }
            }
        }
        return bundleList;
    }

    private static AssetBundleCollectorSetting LoadSettingData()
    {
        var setting = AssetDatabase.LoadAssetAtPath<AssetBundleCollectorSetting>(AssetBundleCollectorSettingPath);
        if (!setting)
        {
            setting = ScriptableObject.CreateInstance<AssetBundleCollectorSetting>();
            AssetDatabase.CreateAsset(setting, AssetBundleCollectorSettingPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        return setting;
    }

    public static string[] GetAllDisplayNames<T>() where T : Enum
    {
        var enumType = typeof(T);
        var enums = Enum.GetNames(enumType);
        var length = enums.Length;
        var displayNames = new string[length];
        for (var i = 0; i < length; i++)
        {
            var enumName = enums[i];
            var field = enumType.GetField(enumName);
            var attributes = field.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (attributes.Length > 0)
            {
                var displayNameAttr = (DisplayNameAttribute)attributes[0];
                displayNames[i] = displayNameAttr.DisplayName;
            }
            else
            {
                displayNames[i] = enumName;
            }
        }
        return displayNames;
    }
    class BundleData
    {
        public AssetBundleCollectorPackage Package;
        public AssetBundleCollectorGroup group;
        public AssetBundleCollector collector;
    }
    enum AddressRuleEnum
    {
        [DisplayName("定位地址: 文件名")]
        AddressByFileName,
        [DisplayName("定位地址: 文件夹名_文件名")]
        AddressByFolderAndFileName,
        [DisplayName("定位地址: 分组名_文件名")]
        AddressByGroupAndFileName,
        [DisplayName("定位地址: 禁用")]
        AddressDisable,
    }
    enum PackRuleNameEnum
    {
        [DisplayName("以文件路径作为资源包名，每个资源文件单独打包")]
        PackSeparately,
        [DisplayName("以文件所在的文件夹路径作为资源包名，该文件夹下所有文件打进一个资源包")]
        PackDirectory,
        [DisplayName("以收集器下顶级文件夹为资源包名，该文件夹下所有文件打进一个资源包")]
        PackTopDirectory,
        [DisplayName("以收集器路径作为资源包名，收集的所有文件打进一个资源包")]
        PackCollector,
        [DisplayName("以分组名称作为资源包名，收集的所有文件打进一个资源包")]
        PackGroup,
        [DisplayName("目录下的资源文件会被处理为原生资源包")]
        PackRawFile,
        [DisplayName("打包着色器变种集合文件")]
        PackShaderVariants,
        [DisplayName("打包着色器文件")]
        PackShader,
    }

    enum FilterRuleEnum
    {
        [DisplayName("收集所有资源,过滤#开头的文件")]
        CollectAllIgnoring,
        [DisplayName("只收集场景,过滤#开头的文件")]
        CollectSceneIgnoring,
        [DisplayName("只收集预制件,过滤#开头的文件")]
        CollectPrefabIgnoring,
        [DisplayName("只收集精灵图,过滤#开头的文件")]
        CollectSpriteIgnoring,
        [DisplayName("只收集shader变体,过滤#开头的文件")]
        CollectShaderVariantsIgnoring,
        [DisplayName("只收集shader,过滤#开头的文件")]
        CollectShaderIgnoring,
    }
}