using System;
using System.IO;
using System.Text.Json;
using GDLog;
using Godot;

namespace LF;

public static class Archive
{
    public static readonly string ArchivePath = Path.Combine(ProjectSettings.GlobalizePath("user://"), "Archive");
    public static readonly string QuickArchivePath = Path.Combine(ArchivePath, "Quick/quick.json");

    /// <summary>
    /// 创建一个新存档
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T CreateNewArchive<T>() where T : ArchiveData, new()
    {
        return new T();
    }

    /// <summary>
    /// 保存存档
    /// </summary>
    /// <param name="data"></param>
    public static void SaveArchive(ArchiveData data)
    {
        SaveArchive(Path.Combine(ArchivePath, data.GUID + ".json"), data);
    }

    /// <summary>
    /// 保存快速存档，推荐用于自动保存
    /// </summary>
    /// <param name="data"></param>
    public static void SaveQuickArchive(ArchiveData data)
    {
        SaveArchive(QuickArchivePath, data);
    }

    /// <summary>
    /// 根据存档id加载存档
    /// </summary>
    /// <param name="archiveGuid"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T LoadArchive<T>(Guid archiveGuid) where T : ArchiveData
    {
        var filePath = ArchivePath + archiveGuid + ".json";
        return LoadArchive<T>(filePath);
    }

    /// <summary>
    /// 加载快速存档，推荐用于自动保存
    /// </summary>
    /// <typeparam name="T">存档数据类型</typeparam>
    /// <returns></returns>
    public static T LoadQuickArchive<T>() where T : ArchiveData
    {
        return LoadArchive<T>(QuickArchivePath);
    }

    private static T LoadArchive<T>(string filePath) where T : ArchiveData
    {
        if (!File.Exists(filePath))
        {
            GLog.Error("不存在存档文件:" + filePath);
            return null;
        }

        var json = File.ReadAllText(filePath);
        try
        {
            var archiveData = JsonSerializer.Deserialize<T>(json);
            if (archiveData == null)
            {
                GLog.Error("反序列化 json 文件为空");
                return null;
            }

            if (!archiveData.IsArchive)
            {
                GLog.Error("不是存档文件:" + filePath);
                return null;
            }

            return archiveData;
        }
        catch (Exception e)
        {
            GLog.Error($"反序列化 json 文件失败:\n{e.Message}{e.StackTrace}");
            return null;
        }
    }

    private static void SaveArchive(string path, ArchiveData data)
    {
        var directory = new FileInfo(path).DirectoryName;

        if (string.IsNullOrEmpty(directory))
        {
            GLog.Error("存储路径为空");
            return;
        }

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, JsonSerializer.Serialize(data));
    }
}