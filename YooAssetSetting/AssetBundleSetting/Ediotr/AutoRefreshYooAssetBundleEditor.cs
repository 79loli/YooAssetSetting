using System;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using YooAsset.Editor;

public class AutoRefreshYooAssetBundleEditor : AssetModificationProcessor
{

    [InitializeOnLoadMethod]
    static void ListenAssetEvent()
    {
        //全局监听project面板下资源的变动

        EditorApplication.projectChanged += delegate ()
        {
            // Debug.Log("改变资源!");
        };
    }

    //下面的方法是重写了UnityEditor.AssetModificationProcessor里面的方法

    /// <summary>
    /// 监听点击或打开资源事件，每次点击project面板的资源都会执行这个方法
    /// </summary>
    /// <param name="assetPath">资源路径</param>
    /// <param name="message"></param>
    /// <returns></returns>
    // public static bool IsOpenForEdit(string assetPath, out string message)
    // {
    //     message = null;
    //     Debug.Log($"选择了资源！资源路径：{assetPath}");
    //     //TRUE表示可以打开，FALSE表示不能在unity打开资源（但是我写false也能打开。。。）
    //     return true;
    // }


    /// <summary>
    /// 监听资源创建事件
    /// </summary>
    /// <param name="path">资源路径</param>
    public static void OnWillCreateAsset(string path)
    {
        var folderGUID = AssetDatabase.AssetPathToGUID(path);
        if (folderGUID.Length > 0)
        {
            return;
        }
        var isFile = File.Exists(path);
        var isDirectory = Directory.Exists(path);
        if (!isFile && !isDirectory)
        {
            var isMeta = Path.GetExtension(path).Equals(".meta", StringComparison.OrdinalIgnoreCase);
            if (isMeta)
            {
                string fileNameWithoutExtension = path.Substring(0, path.Length - 5);
                if (Directory.Exists(fileNameWithoutExtension))
                {
                    isDirectory = true;
                    path = fileNameWithoutExtension;
                }
            }
        }
        if (isDirectory && !isFile)
        {
            FolderYooAssetBundleWindow.OnWillMoveAsset(path, path, true);
        }
        Debug.LogFormat($"创建资源！路径：{path}");
    }

    /// <summary>
    /// 监听资源保存事件
    /// </summary>
    /// <param name="paths">资源路径</param>
    /// <returns></returns>
    public static string[] OnWillSaveAssets(string[] paths)
    {
        if (paths != null)
        {
            var length = paths.Length;
            for (var i = 0; i < length; i++)
            {
                var path = paths[i];
                var isDirectory = Directory.Exists(path) && !File.Exists(path);
                if (isDirectory)
                {
                    FolderYooAssetBundleWindow.OnWillMoveAsset(path, path, true);
                }
            }
            //string.Join  连接多个字符串
            Debug.LogFormat("保存资源！路径：{0}", string.Join(",", paths));
        }
        return paths;
    }

    /// <summary>
    /// 监听资源移动事件
    /// </summary>
    /// <param name="oldPath">旧路径</param>
    /// <param name="newPath">新路径</param>
    /// <returns></returns>
    public static AssetMoveResult OnWillMoveAsset(string oldPath, string newPath)
    {
        var folderGUID = AssetDatabase.AssetPathToGUID(newPath);
        if (folderGUID.Length > 0)
        {
            return AssetMoveResult.DidMove;
        }
        var isDirectory = Directory.Exists(oldPath) && !File.Exists(oldPath);
        if (isDirectory)
        {
            string searchPattern = "$*"; // 搜索模式以 $ 开头的任何名称
            string[] folders = Directory.GetDirectories(oldPath, searchPattern, SearchOption.AllDirectories);
            foreach (string folderPath in folders)
            {
                string relativePath = folderPath.Substring(oldPath.Length); // 获取相对路径
                string newFolderPath = newPath + relativePath; // 将旧路径的起始部分替换为新路径
                string normalizedPath = newFolderPath.Replace('\\', '/');
                FolderYooAssetBundleWindow.OnWillMoveAsset(folderPath, normalizedPath);
                Debug.LogFormat($"移动资源！从旧路径:{folderPath}到新路径:{normalizedPath}  {isDirectory}");
            }
            FolderYooAssetBundleWindow.OnWillMoveAsset(oldPath, newPath);
        }
        Debug.LogFormat($"移动资源！从旧路径:{oldPath}到新路径:{newPath}  {isDirectory}");
        //AssetMoveResult.DidNotMove表示该资源可以移动，Didmove表示不能移动   
        return AssetMoveResult.DidNotMove;
    }

    /// <summary>
    /// 监听资源即将被删除事件
    /// </summary>
    /// <param name="path">资源路径</param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
    {
        var isDirectory = Directory.Exists(path) && !File.Exists(path);
        if (isDirectory)
        {
            string searchPattern = "$*"; // 搜索模式以 $ 开头的任何名称
            string[] folders = Directory.GetDirectories(path, searchPattern, SearchOption.AllDirectories);
            foreach (string folderPath in folders)
            {
                string normalizedPath = folderPath.Replace('\\', '/');
                FolderYooAssetBundleWindow.OnWillMoveAsset(normalizedPath, "");
            }
            FolderYooAssetBundleWindow.OnWillMoveAsset(path, "");
        }
        Debug.LogFormat($"删除资源！路径：{path}");
        //AssetDeleteResult.DidNotDelete表示可以删除 DidDelete表示不可以删除  
        return AssetDeleteResult.DidNotDelete;
    }

}
