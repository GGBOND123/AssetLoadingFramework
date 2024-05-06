using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public class BuildApp
{
    //  “../”代表上一层路径
    //Application.dataPath：在Unity中就是Asset目录
    private static string m_AppName = PlayerSettings.productName;//RealConfig.GetRealFram().m_AppName;
    public static string m_AndroidPath = Application.dataPath + "/../BuildTarget/Android/";
    public static string m_IOSPath = Application.dataPath + "/../BuildTarget/IOS/";
    public static string m_WindowsPath = Application.dataPath + "/../BuildTarget/Windows/";
     
    [MenuItem("Build/标准包")]
    public static void Build()
    {
        //打ab包
        BundleEditor.Build();
        //生成可执行程序
        string abPath = Application.dataPath + "/../AssetBundle/" + EditorUserBuildSettings.activeBuildTarget.ToString() + "/";
        Copy(abPath, Application.streamingAssetsPath);
        string savePath = "";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            savePath = m_AndroidPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now) + ".apk";
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
        {
            savePath = m_IOSPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}", DateTime.Now);
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows|| EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            savePath = m_WindowsPath + m_AppName + "_" + EditorUserBuildSettings.activeBuildTarget + string.Format("_{0:yyyy_MM_dd_HH_mm}/{1}.exe", DateTime.Now, m_AppName);
        }
        //参数列表：场景数组、打包终点路径、平台、
        BuildPipeline.
            BuildPlayer(FindEnableEditorrScenes(), savePath, EditorUserBuildSettings.activeBuildTarget, BuildOptions.None);
        //打完包后删除streamingAssetsPath内的数据
        DeleteDir(Application.streamingAssetsPath);
    }

    //找到所有待打包的场景
    private static string[] FindEnableEditorrScenes()
    {
        List<string> editorScenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            //是否在 Build Settings 窗口中启用了此场景
            if (!scene.enabled) continue;
            editorScenes.Add(scene.path);
        }
        return editorScenes.ToArray();
    }

    private static void Copy(string srcPath, string targetPath)
    {
        try
        {
            if (!Directory.Exists(targetPath))
            {
                Directory.CreateDirectory(targetPath);
            }
            //合并两个路径字符串                           
            //返回由只读字符范围表示的文件路径的文件名和扩展名。 string s1 = Path.GetFileName(“D:\dir\asp.net\readme.txt”); // readme.text 
            string scrdir = Path.Combine(targetPath, Path.GetFileName(srcPath));
            if (Directory.Exists(srcPath))
                scrdir += Path.DirectorySeparatorChar;
            else
                Directory.CreateDirectory(scrdir);

            string[] files = Directory.GetFileSystemEntries(srcPath);
            foreach (string file in files)
            {
                //存在这样一个文件夹的的话，递归加载
                if (Directory.Exists(file))
                {
                    Copy(file, scrdir);
                }
                else
                {
                    File.Copy(file, scrdir + Path.GetFileName(file), true);
                }
            }

        }
        catch
        {
            Debug.LogError("无法复制：" + srcPath + "  到" + targetPath);
        }
    }
    /*
     * 如何获取指定目录包含的文件和子目录
     * 1. DirectoryInfo.GetFiles()：获取目录中（不包含子目录）的文件，返回类型为FileInfo[]，支持通配符查找；
     * 2. DirectoryInfo.GetDirectories()：获取目录（不包含子目录）的子目录，返回类型为DirectoryInfo[]，支持通配符查找；
     * 3. DirectoryInfo. GetFileSystemInfos()：获取指定目录下（不包含子目录）的文件和子目录，返回类型为FileSystemInfo[]，支持通配符查找；
     * 如何获取指定文件的基本信息；
     * FileInfo.Exists：获取指定文件是否存在；
     * FileInfo.Name，FileInfo.Extensioin：获取文件的名称和扩展名；
     * FileInfo.FullName：获取文件的全限定名称（完整路径）；
     * FileInfo.Directory：获取文件所在目录，返回类型为DirectoryInfo；
     * FileInfo.DirectoryName：获取文件所在目录的路径（完整路径）；
     * FileInfo.Length：获取文件的大小（字节数）；
     * FileInfo.IsReadOnly：获取文件是否只读；
     * FileInfo.Attributes：获取或设置指定文件的属性，返回类型为FileAttributes枚举，可以是多个值的组合
     * FileInfo.CreationTime、FileInfo.LastAccessTime、FileInfo.LastWriteTime：分别用于获取文件的创建时间、访问时间、修改时间；
     */
    public static void DeleteDir(string scrPath)
    {
        try
        {
            //目录路径
            DirectoryInfo dir = new DirectoryInfo(scrPath);
            FileSystemInfo[] fileInfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo info in fileInfo)
            {
                if (info is DirectoryInfo)
                {
                    //如果是子目录
                    DirectoryInfo subdir = new DirectoryInfo(info.FullName);
                    subdir.Delete(true);
                }
                else
                {
                    File.Delete(info.FullName);
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
        }
    }
}
