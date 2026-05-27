using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DriveGroupManager
{
    public partial class MainForm : Form
    {
        private DriveGroupManagerLogic manager;
        private TreeView tvGroupsAndDrives;  // 树形视图显示分组和硬盘
        private Label lblStatus;
        private Button btnEditGroups;
        private Button btnRefresh;
        private Button btnOpenDrive;

        public MainForm()
        {
            InitializeComponent();
            manager = new DriveGroupManagerLogic();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "硬盘分组管理器 - 我的硬盘";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 500);
            this.BackColor = Color.White;

            // 创建工具栏
            var toolStrip = new ToolStrip();
            toolStrip.Items.Add(new ToolStripButton("✏️ 编辑分组", null, (s, e) => EditGroups()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("🔄 刷新", null, (s, e) => RefreshDisplay()));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("📂 帮助", null, (s, e) => ShowHelp()));
            
            // 创建主面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));

            // 树形视图（显示分组和硬盘）
            tvGroupsAndDrives = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10),
                ShowLines = true,
                ShowPlusMinus = true,
                FullRowSelect = true,
                HideSelection = false,
                Indent = 20
            };
            tvGroupsAndDrives.NodeMouseDoubleClick += TvGroupsAndDrives_NodeDoubleClick;

            // 底部按钮面板
            var bottomPanel = new Panel { Dock = DockStyle.Fill };
            var buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            btnOpenDrive = new Button
            {
                Text = "🚀 打开选中硬盘",
                Size = new Size(140, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                Enabled = false
            };
            btnOpenDrive.Click += (s, e) => OpenSelectedDrive();

            btnEditGroups = new Button
            {
                Text = "✏️ 编辑分组设置",
                Size = new Size(130, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9)
            };
            btnEditGroups.Click += (s, e) => EditGroups();

            btnRefresh = new Button
            {
                Text = "🔄 刷新",
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9)
            };
            btnRefresh.Click += (s, e) => RefreshDisplay();

            buttonFlow.Controls.AddRange(new Control[] { btnOpenDrive, btnEditGroups, btnRefresh });

            lblStatus = new Label
            {
                Text = "就绪",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("微软雅黑", 9),
                ForeColor = Color.Gray
            };

            bottomPanel.Controls.Add(buttonFlow);
            mainPanel.Controls.Add(tvGroupsAndDrives, 0, 0);
            mainPanel.Controls.Add(bottomPanel, 0, 1);
            mainPanel.Controls.Add(lblStatus, 0, 2);
            mainPanel.SetRowSpan(lblStatus, 1);
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));

            this.Controls.Add(mainPanel);
            this.Controls.Add(toolStrip);
        }

        private void LoadData()
        {
            tvGroupsAndDrives.Nodes.Clear();
            var groups = manager.GetAllGroups();
            
            // 创建"未分组的硬盘"节点
            var allDrives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.Name.TrimEnd('\\'))
                .ToList();
            
            var groupedDrives = groups.SelectMany(g => g.DriveLetters).ToList();
            var ungroupedDrives = allDrives.Except(groupedDrives).ToList();

            // 添加分组节点
            foreach (var group in groups)
            {
                TreeNode groupNode = new TreeNode($"📁 {group.GroupName}");
                groupNode.Tag = group;
                groupNode.ForeColor = Color.FromArgb(44, 62, 80);
                groupNode.NodeFont = new Font("微软雅黑", 10, FontStyle.Bold);
                
                foreach (string drive in group.DriveLetters)
                {
                    if (allDrives.Contains(drive))
                    {
                        TreeNode driveNode = CreateDriveNode(drive);
                        groupNode.Nodes.Add(driveNode);
                    }
                }
                
                tvGroupsAndDrives.Nodes.Add(groupNode);
                groupNode.Expand();
            }

            // 添加"未分组"节点
            if (ungroupedDrives.Any())
            {
                TreeNode ungroupedNode = new TreeNode($"📂 未分组硬盘 ({ungroupedDrives.Count})");
                ungroupedNode.ForeColor = Color.FromArgb(127, 140, 141);
                ungroupedNode.NodeFont = new Font("微软雅黑", 9, FontStyle.Italic);
                
                foreach (string drive in ungroupedDrives)
                {
                    TreeNode driveNode = CreateDriveNode(drive);
                    ungroupedNode.Nodes.Add(driveNode);
                }
                
                tvGroupsAndDrives.Nodes.Add(ungroupedNode);
                ungroupedNode.Expand();
            }

            // 添加"所有硬盘"节点
            TreeNode allDrivesNode = new TreeNode($"💾 所有硬盘 ({allDrives.Count})");
            allDrivesNode.ForeColor = Color.FromArgb(41, 128, 185);
            allDrivesNode.NodeFont = new Font("微软雅黑", 9, FontStyle.Bold);
            
            foreach (string drive in allDrives)
            {
                TreeNode driveNode = CreateDriveNode(drive);
                allDrivesNode.Nodes.Add(driveNode);
            }
            
            tvGroupsAndDrives.Nodes.Add(allDrivesNode);
            allDrivesNode.Expand();

            lblStatus.Text = $"共 {groups.Count} 个分组, {allDrives.Count} 个硬盘";
        }

        private TreeNode CreateDriveNode(string driveLetter)
        {
            string driveInfo = GetDriveInfo(driveLetter);
            TreeNode node = new TreeNode($"💿 {driveLetter} - {driveInfo}");
            node.Tag = driveLetter;
            
            // 根据剩余空间设置颜色
            try
            {
                var drive = new DriveInfo(driveLetter);
                if (drive.IsReady)
                {
                    double freePercent = (double)drive.AvailableFreeSpace / drive.TotalSize * 100;
                    if (freePercent < 10)
                        node.ForeColor = Color.Red;
                    else if (freePercent < 20)
                        node.ForeColor = Color.Orange;
                    else
                        node.ForeColor = Color.FromArgb(39, 174, 96);
                }
            }
            catch { }
            
            return node;
        }

        private string GetDriveInfo(string driveLetter)
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                if (drive.IsReady)
                {
                    string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "本地磁盘" : drive.VolumeLabel;
                    long totalGB = drive.TotalSize / (1024 * 1024 * 1024);
                    long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    return $"{label} ({totalGB}GB, 剩余{freeGB}GB)";
                }
                return "不可访问";
            }
            catch
            {
                return "无法读取";
            }
        }

        private void TvGroupsAndDrives_NodeDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is string driveLetter)
            {
                manager.OpenDrive(driveLetter);
            }
        }

        private void OpenSelectedDrive()
        {
            if (tvGroupsAndDrives.SelectedNode?.Tag is string driveLetter)
            {
                manager.OpenDrive(driveLetter);
            }
        }

        private void EditGroups()
        {
            using (var editForm = new EditGroupsForm(manager.GetAllGroups()))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    manager.UpdateAllGroups(editForm.UpdatedGroups);
                    LoadData();
                    lblStatus.Text = "分组设置已更新";
                }
            }
        }

        private void RefreshDisplay()
        {
            LoadData();
            lblStatus.Text = "已刷新";
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "硬盘分组管理器使用说明\n\n" +
                "• 双击硬盘图标可以直接打开该硬盘\n" +
                "• 点击「编辑分组设置」可以管理分组\n" +
                "• 未分组的硬盘会显示在「未分组」区域\n" +
                "• 硬盘颜色代表剩余空间：绿色充足、橙色紧张、红色不足",
                "帮助",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}