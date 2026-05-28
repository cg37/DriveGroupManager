using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;

namespace DriveGroupManager
{
    public partial class MainForm : MaterialForm
    {
        private DriveGroupManagerLogic manager;
        private TreeView tvGroupsAndDrives;  // 树形视图显示分组和硬盘
        private Label lblStatus;
        private Button btnEditGroups;
        private Button btnRefresh;
        private Button btnOpenDrive;

        private readonly MaterialSkinManager materialSkinManager;

        public MainForm()
        {
            InitializeComponent();

            // 初始化 MaterialSkin 主题管理器
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue600,    // 主色
                Primary.Blue700,    // 深色主色
                Primary.Blue200,    // 浅色主色
                Accent.Cyan200,     // 强调色
                TextShade.WHITE     // 文字阴影
            );

            manager = new DriveGroupManagerLogic();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.Font = new Font("微软雅黑", 12F);
            this.Text = "硬盘分组管理器";
            this.Size = new Size(900, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 550);

            // 创建主面板（使用 MaterialDivider 作为分隔）
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(10)
            };
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 90F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

            // 树形视图（显示分组和硬盘）
            tvGroupsAndDrives = new TreeView
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10),
                ShowLines = true,
                ShowPlusMinus = true,
                FullRowSelect = true,
                HideSelection = false,
                Indent = 20,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                DrawMode = TreeViewDrawMode.OwnerDrawText
            };
            tvGroupsAndDrives.DrawNode += TvGroupsAndDrives_DrawNode;
            tvGroupsAndDrives.NodeMouseDoubleClick += TvGroupsAndDrives_NodeDoubleClick;

            // 使用 MaterialCard 包裹树形视图
            var card = new MaterialCard
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            card.Controls.Add(tvGroupsAndDrives);

            // 底部按钮面板 - 使用 MaterialButton
            var buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5),
                WrapContents = false
            };

            var btnOpenDriveMaterial = new MaterialButton
            {
                Text = "打开选中硬盘",
                AutoSize = false,
                Size = new Size(160, 40),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Enabled = false
            };
            btnOpenDriveMaterial.Click += (s, e) => OpenSelectedDrive();
            btnOpenDrive = btnOpenDriveMaterial;

            var btnEditGroupsMaterial = new MaterialButton
            {
                Text = "编辑分组",
                AutoSize = false,
                Size = new Size(130, 40),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false
            };
            btnEditGroupsMaterial.Click += (s, e) => EditGroups();
            btnEditGroups = btnEditGroupsMaterial;

            var btnRefreshMaterial = new MaterialButton
            {
                Text = "刷新",
                AutoSize = false,
                Size = new Size(100, 40),
                Type = MaterialButton.MaterialButtonType.Outlined,
                UseAccentColor = false
            };
            btnRefreshMaterial.Click += (s, e) => RefreshDisplay();
            btnRefresh = btnRefreshMaterial;

            buttonFlow.Controls.AddRange(new Control[] { btnOpenDriveMaterial, btnEditGroupsMaterial, btnRefreshMaterial });

            // 状态栏使用 MaterialLabel
            var statusLabel = new MaterialLabel
            {
                Text = "就绪",
                Dock = DockStyle.Fill,
                FontType = MaterialSkin.MaterialSkinManager.fontType.Body2
            };
            lblStatus = statusLabel;

            mainPanel.Controls.Add(card, 0, 0);
            mainPanel.Controls.Add(buttonFlow, 0, 1);
            mainPanel.Controls.Add(statusLabel, 0, 2);

            this.Controls.Add(mainPanel);
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

        private void TvGroupsAndDrives_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            // 使用 OwnerDrawText 模式：让系统绘制默认部分，我们只自定义文本颜色和字体
            TreeNode node = e.Node;

            // 如果节点未被选中，使用自定义颜色和字体
            if ((e.State & TreeNodeStates.Selected) == 0)
            {
                Color textColor = node.ForeColor;
                Font nodeFont = node.NodeFont ?? e.Node.TreeView.Font;

                // 计算文本绘制区域
                Rectangle textRect = e.Bounds;
                textRect.X += 2;

                // 绘制背景（处理选中/焦点状态）
                if ((e.State & TreeNodeStates.Hot) != 0)
                {
                    using (var brush = new SolidBrush(Color.FromArgb(230, 240, 250)))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }

                // 绘制文本
                TextRenderer.DrawText(e.Graphics, node.Text, nodeFont, textRect, textColor,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }
            else
            {
                // 选中状态使用默认绘制
                e.DrawDefault = true;
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