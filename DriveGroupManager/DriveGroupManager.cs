using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;

namespace DriveGroupManager
{
    public class DriveGroupManagerLogic
    {
        private List<DriveGroup> groups;
        private string dataFilePath;

        public DriveGroupManagerLogic()
        {
            groups = new List<DriveGroup>();
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DriveGroupManager");
            
            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);
            
            dataFilePath = Path.Combine(appDataPath, "groups.json");
            LoadGroups();
            
            if (groups.Count == 0)
            {
                CreateSampleGroups();
                SaveGroups();
            }
        }

        public List<DriveGroup> GetAllGroups()
        {
            return groups;
        }

        public void UpdateAllGroups(List<DriveGroup> newGroups)
        {
            groups = newGroups;
            SaveGroups();
        }

        public void OpenDrive(string driveLetter)
        {
            try
            {
                string path = driveLetter.EndsWith("\\") ? driveLetter : driveLetter + "\\";
                if (Directory.Exists(path))
                {
                    Process.Start("explorer.exe", path);
                }
                else
                {
                    MessageBox.Show($"硬盘 {driveLetter} 不存在或无法访问。", "提示", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateSampleGroups()
        {
            var availableDrives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.Name.TrimEnd('\\'))
                .ToList();

            if (availableDrives.Count >= 2)
            {
                groups.Add(new DriveGroup("系统与软件", 
                    availableDrives.Take(1).ToList(), "操作系统和应用程序", 0));
                
                if (availableDrives.Count >= 3)
                {
                    groups.Add(new DriveGroup("工作文档", 
                        availableDrives.Skip(1).Take(1).ToList(), "工作相关文件", 1));
                    
                    groups.Add(new DriveGroup("娱乐媒体", 
                        availableDrives.Skip(2).Take(1).ToList(), "音乐、视频、游戏", 2));
                }
            }
            
            if (groups.Count == 0)
            {
                groups.Add(new DriveGroup("我的硬盘", 
                    availableDrives, "所有硬盘", 0));
            }
        }

        private void SaveGroups()
        {
            try
            {
                string json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadGroups()
        {
            if (!File.Exists(dataFilePath)) return;
            
            try
            {
                string json = File.ReadAllText(dataFilePath);
                groups = JsonSerializer.Deserialize<List<DriveGroup>>(json);
                groups ??= new List<DriveGroup>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载失败: {ex.Message}", "错误", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                groups = new List<DriveGroup>();
            }
        }
    }
}