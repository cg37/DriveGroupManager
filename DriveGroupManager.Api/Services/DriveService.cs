using System.Diagnostics;
using System.IO;
using DriveGroupManager.Api.Models;

namespace DriveGroupManager.Api.Services;

public interface IDriveService
{
    List<DriveGroup> GetAllGroups();
    void UpdateGroups(List<DriveGroup> groups);
    List<DriveInfo> GetAllDrives();
    List<DriveInfo> GetAvailableDrives();
    void OpenDrive(string driveLetter);
}

public class DriveService : IDriveService
{
    private List<DriveGroup> _groups = new();
    private readonly string _dataFilePath;

    public DriveService()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DriveGroupManager");

        if (!Directory.Exists(appDataPath))
            Directory.CreateDirectory(appDataPath);

        _dataFilePath = Path.Combine(appDataPath, "groups.json");
        LoadGroups();

        if (_groups.Count == 0)
        {
            CreateSampleGroups();
            SaveGroups();
        }
    }

    public List<DriveGroup> GetAllGroups()
    {
        return _groups;
    }

    public void UpdateGroups(List<DriveGroup> groups)
    {
        _groups = groups;
        SaveGroups();
    }

    public List<DriveInfo> GetAllDrives()
    {
        return System.IO.DriveInfo.GetDrives()
            .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
            .Select(MapToDriveInfo)
            .ToList();
    }

    public List<DriveInfo> GetAvailableDrives()
    {
        var allDrives = GetAllDrives();
        var usedDrives = _groups.SelectMany(g => g.DriveLetters).Distinct().ToList();
        return allDrives.Where(d => !usedDrives.Contains(d.Letter)).ToList();
    }

    public void OpenDrive(string driveLetter)
    {
        string path = driveLetter.EndsWith("\\") ? driveLetter : driveLetter + "\\";
        if (Directory.Exists(path))
        {
            Process.Start("explorer.exe", path);
        }
    }

    private void CreateSampleGroups()
    {
        var availableDrives = GetAllDrives();

        if (availableDrives.Count >= 2)
        {
            _groups.Add(new DriveGroup
            {
                GroupName = "系统与软件",
                DriveLetters = availableDrives.Take(1).Select(d => d.Letter).ToList(),
                Description = "操作系统和应用程序",
                IconIndex = 0
            });

            if (availableDrives.Count >= 3)
            {
                _groups.Add(new DriveGroup
                {
                    GroupName = "工作文档",
                    DriveLetters = availableDrives.Skip(1).Take(1).Select(d => d.Letter).ToList(),
                    Description = "工作相关文件",
                    IconIndex = 1
                });

                _groups.Add(new DriveGroup
                {
                    GroupName = "娱乐媒体",
                    DriveLetters = availableDrives.Skip(2).Take(1).Select(d => d.Letter).ToList(),
                    Description = "音乐、视频、游戏",
                    IconIndex = 2
                });
            }
        }

        if (_groups.Count == 0 && availableDrives.Any())
        {
            _groups.Add(new DriveGroup
            {
                GroupName = "我的硬盘",
                DriveLetters = availableDrives.Select(d => d.Letter).ToList(),
                Description = "所有硬盘",
                IconIndex = 0
            });
        }
    }

    private void SaveGroups()
    {
        try
        {
            string json = System.Text.Json.JsonSerializer.Serialize(_groups,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_dataFilePath, json);
        }
        catch { }
    }

    private void LoadGroups()
    {
        if (!File.Exists(_dataFilePath)) return;

        try
        {
            string json = File.ReadAllText(_dataFilePath);
            _groups = System.Text.Json.JsonSerializer.Deserialize<List<DriveGroup>>(json) ?? new List<DriveGroup>();
        }
        catch
        {
            _groups = new List<DriveGroup>();
        }
    }

    private DriveInfo MapToDriveInfo(System.IO.DriveInfo drive)
    {
        var info = new DriveInfo
        {
            Letter = drive.Name.TrimEnd('\\'),
            Label = string.IsNullOrEmpty(drive.VolumeLabel) ? "本地磁盘" : drive.VolumeLabel,
            TotalSizeGB = drive.TotalSize / (1024 * 1024 * 1024),
            FreeSpaceGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024)
        };

        info.FreePercent = info.TotalSizeGB > 0
            ? (double)info.FreeSpaceGB / info.TotalSizeGB * 100
            : 0;

        info.Status = info.FreePercent switch
        {
            < 10 => "danger",
            < 20 => "warning",
            _ => "normal"
        };

        return info;
    }
}
