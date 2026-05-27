using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;  // 添加这行

namespace DriveGroupManager
{
    /// <summary>
    /// 硬盘分组管理器 - 处理所有业务逻辑
    /// </summary>
    public class DriveGroupManagerLogic
    {
        // 存储所有分组
        private List<DriveGroup> groups;
        
        // 数据保存文件路径（放在用户应用程序数据目录）
        private string dataFilePath;
        
        /// <summary>
        /// 构造函数 - 初始化管理器
        /// </summary>
        public DriveGroupManagerLogic()
        {
            groups = new List<DriveGroup>();
            // 数据文件保存在: C:\Users\用户名\AppData\Local\DriveGroupManager\groups.xml
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DriveGroupManager");
            
            // 确保目录存在
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            
            dataFilePath = Path.Combine(appDataPath, "groups.xml");
            
            // 尝试加载之前保存的数据
            LoadGroups();
            
            // 如果没有数据，创建示例分组
            if (groups.Count == 0)
            {
                CreateSampleGroups();
                SaveGroups();  // 保存示例数据
            }
        }
        
        /// <summary>
        /// 获取所有分组
        /// </summary>
        public List<DriveGroup> GetAllGroups()
        {
            return groups;
        }
        
        /// <summary>
        /// 添加新分组
        /// </summary>
        public bool AddGroup(DriveGroup group)
        {
            // 检查分组名称是否已存在
            if (groups.Any(g => g.GroupName == group.GroupName))
            {
                MessageBox.Show($"分组名称 '{group.GroupName}' 已存在，请使用其他名称。", 
                    "名称冲突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            groups.Add(group);
            SaveGroups();
            return true;
        }
        
        /// <summary>
        /// 删除分组
        /// </summary>
        public bool DeleteGroup(string groupName)
        {
            var group = groups.FirstOrDefault(g => g.GroupName == groupName);
            if (group != null)
            {
                groups.Remove(group);
                SaveGroups();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 更新分组信息
        /// </summary>
        public bool UpdateGroup(string oldName, DriveGroup updatedGroup)
        {
            var index = groups.FindIndex(g => g.GroupName == oldName);
            if (index >= 0)
            {
                // 如果修改了名称，检查新名称是否与其他分组冲突
                if (oldName != updatedGroup.GroupName && 
                    groups.Any(g => g.GroupName == updatedGroup.GroupName))
                {
                    MessageBox.Show($"分组名称 '{updatedGroup.GroupName}' 已存在。", 
                        "名称冲突", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
                
                groups[index] = updatedGroup;
                SaveGroups();
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 打开分组中的所有硬盘（每个硬盘打开独立的资源管理器窗口）
        /// </summary>
        public void OpenAllDrivesInGroup(string groupName)
        {
            var group = groups.FirstOrDefault(g => g.GroupName == groupName);
            if (group == null)
            {
                MessageBox.Show($"找不到分组 '{groupName}'。", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            if (group.DriveLetters.Count == 0)
            {
                MessageBox.Show($"分组 '{groupName}' 中没有添加任何硬盘。", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            int successCount = 0;
            int failCount = 0;
            
            foreach (string driveLetter in group.DriveLetters)
            {
                if (OpenExplorerForDrive(driveLetter))
                    successCount++;
                else
                    failCount++;
                
                // 稍微延迟，避免窗口打开过于密集
                System.Threading.Thread.Sleep(100);
            }
            
            // 显示打开结果
            string message = $"已成功打开 {successCount} 个硬盘";
            if (failCount > 0)
                message += $"，{failCount} 个硬盘打开失败";
            
            MessageBox.Show(message, "打开完成", MessageBoxButtons.OK, 
                failCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }
        
        /// <summary>
        /// 打开单个硬盘的资源管理器
        /// </summary>
        /// <returns>是否成功打开</returns>
        private bool OpenExplorerForDrive(string driveLetter)
        {
            try
            {
                // 确保盘符格式正确
                string path = driveLetter.EndsWith("\\") ? driveLetter : driveLetter + "\\";
                
                // 检查硬盘是否存在且可访问
                if (Directory.Exists(path))
                {
                    // 使用系统默认方式打开资源管理器
                    Process.Start("explorer.exe", path);
                    return true;
                }
                else
                {
                    MessageBox.Show($"硬盘 {driveLetter} 不存在或无法访问。\n请检查硬盘是否已连接。", 
                        "打开失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开硬盘 {driveLetter} 时发生错误:\n{ex.Message}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// 创建示例分组数据
        /// </summary>
        private void CreateSampleGroups()
        {
            // 获取系统中所有可用的硬盘盘符
            var availableDrives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)  // 只取已就绪的固定硬盘
                .Select(d => d.Name.TrimEnd('\\'))  // 去掉末尾的反斜杠，如 "C:" 而不是 "C:\"
                .ToList();
            
            if (availableDrives.Count >= 2)
            {
                groups.Add(new DriveGroup("常用硬盘", 
                    availableDrives.Take(2).ToList(), 
                    "日常工作和娱乐项目", 0));
            }
            
            if (availableDrives.Count >= 4)
            {
                groups.Add(new DriveGroup("备份硬盘", 
                    availableDrives.Skip(2).Take(2).ToList(), 
                    "重要文件备份", 1));
            }
            
            // 如果没有检测到足够多的硬盘，创建演示数据
            if (groups.Count == 0)
            {
                groups.Add(new DriveGroup("示例分组1", 
                    new List<string> { "C:", "D:" }, 
                    "这是一个示例分组", 0));
                groups.Add(new DriveGroup("示例分组2", 
                    new List<string> { "E:" }, 
                    "这是另一个示例分组", 1));
            }
        }
        
        /// <summary>
        /// 保存分组数据到XML文件
        /// </summary>
        private void SaveGroups()
{
    try
    {
        string directory = Path.GetDirectoryName(dataFilePath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        // 使用 JSON 序列化，更简单可靠
        string json = JsonConvert.SerializeObject(groups, Formatting.Indented);
        File.WriteAllText(dataFilePath, json);
    }
    catch (Exception ex)
    {
        MessageBox.Show($"保存数据失败:\n{ex.Message}\n\n程序将继续运行，但修改可能不会被保存。", 
            "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
        
        /// <summary>
        /// 从XML文件加载分组数据
        /// </summary>
      /// <summary>
/// 从JSON文件加载分组数据
/// </summary>
private void LoadGroups()
{
    if (!File.Exists(dataFilePath))
        return;
    
    try
    {
        string json = File.ReadAllText(dataFilePath);
        groups = JsonConvert.DeserializeObject<List<DriveGroup>>(json);
        
        // 如果加载成功但数据为空，初始化为空列表
        if (groups == null)
        {
            groups = new List<DriveGroup>();
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"加载数据失败:\n{ex.Message}\n将使用默认数据。", 
            "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        groups = new List<DriveGroup>();
    }
}
    }
}