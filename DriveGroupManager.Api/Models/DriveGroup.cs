namespace DriveGroupManager.Api.Models;

/// <summary>
/// 硬盘分组数据模型
/// </summary>
public class DriveGroup
{
    public string GroupName { get; set; } = string.Empty;
    public List<string> DriveLetters { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public int IconIndex { get; set; }

    public override string ToString()
    {
        return $"{GroupName} ({DriveLetters.Count}个硬盘)";
    }
}

/// <summary>
/// 硬盘信息模型
/// </summary>
public class DriveInfo
{
    public string Letter { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public long TotalSizeGB { get; set; }
    public long FreeSpaceGB { get; set; }
    public double FreePercent { get; set; }
    public string Status { get; set; } = "normal"; // normal, warning, danger
}

/// <summary>
/// 分组视图模型
/// </summary>
public class GroupViewModel
{
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DriveCount { get; set; }
    public List<DriveInfo> Drives { get; set; } = new();
}
