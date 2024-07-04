using UnityEngine;
using UnityEditor;
using System.IO;

public class FolderYooAssetBundleMenu
{
    [MenuItem("Assets/重新编辑Bundle配置")]
    private static void ResetBundleConfig()
    {
        // 获取当前选中的文件夹路径
        string selectedPath = Selection.assetGUIDs.Length > 0
            ? AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0])
            : "Assets";

        // 检查选中的是否为文件夹
        if (Directory.Exists(selectedPath) && !File.Exists(selectedPath))
        {
            string name = Path.GetFileNameWithoutExtension(selectedPath);
            var isHas = name.StartsWith("$");
            if (isHas)
            {
                FolderYooAssetBundleWindow.ShowResetWindow(selectedPath);
            }
            else
            {
                bool userClickedOK = EditorUtility.DisplayDialog("bundle设置错误", $"该文件夹还未分配Bundle,请对它添加$", "关闭");
            }
        }
        else
        {
            bool userClickedOK = EditorUtility.DisplayDialog("bundle设置错误", $"不是文件夹", "关闭");
        }
    }

}