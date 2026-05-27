using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DriveGroupManager
{
    /// <summary>
    /// 主窗体 - 处理所有用户界面交互
    /// </summary>
    public partial class MainForm : Form
    {
        private DriveGroupManagerLogic manager;
        private ListBox lstGroups;           // 显示分组列表
        private ListBox lstDrivesInGroup;    // 显示选中分组内的硬盘
        private Button btnOpenGroup;         // 打开分组按钮
        private Button btnAddGroup;          // 添加分组按钮
        private Button btnEditGroup;         // 编辑分组按钮
        private Button btnDeleteGroup;       // 删除分组按钮
        private Button btnAddDrive;          // 添加硬盘到分组
        private Button btnRemoveDrive;       // 从分组移除硬盘
        private Label lblStatus;             // 状态栏
        private CheckBox chkConfirmOpen;     // 是否确认打开的复选框
        
        public MainForm()
        {
            InitializeComponent();
            manager = new DriveGroupManagerLogic();
            LoadGroupsToList();
            UpdateStatus();
        }
        
        /// <summary>
        /// 初始化窗体的所有控件
        /// </summary>
        private void InitializeComponent()
        {
            // 设置窗体属性
            this.Text = "硬盘分组管理器";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 500);
            this.BackColor = Color.White;
            
            // 创建主布局面板
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                Padding = new Padding(10),
                BackColor = Color.White
            };
            
            // 设置列宽比例
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            
            // 设置行高比例
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            
            // ========== 左侧：分组管理区域 ==========
            var leftGroupBox = new GroupBox
            {
                Text = "📁 我的硬盘分组",
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            
            var leftLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 80F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            
            // 分组列表
            lstGroups = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10),
                IntegralHeight = false
            };
            lstGroups.SelectedIndexChanged += LstGroups_SelectedIndexChanged;
            
            // 分组操作按钮面板
            var groupButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 0)
            };
            
            btnAddGroup = new Button
            {
                Text = "➕ 新建分组",
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9)
            };
            btnAddGroup.Click += BtnAddGroup_Click;
            
            btnEditGroup = new Button
            {
                Text = "✏️ 编辑分组",
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                Enabled = false
            };
            btnEditGroup.Click += BtnEditGroup_Click;
            
            btnDeleteGroup = new Button
            {
                Text = "🗑️ 删除分组",
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                Enabled = false
            };
            btnDeleteGroup.Click += BtnDeleteGroup_Click;
            
            groupButtonPanel.Controls.AddRange(new Control[] { btnAddGroup, btnEditGroup, btnDeleteGroup });
            
            leftLayout.Controls.Add(lstGroups, 0, 0);
            leftLayout.Controls.Add(groupButtonPanel, 0, 1);
            
            leftGroupBox.Controls.Add(leftLayout);
            
            // ========== 右侧：硬盘管理区域 ==========
            var rightGroupBox = new GroupBox
            {
                Text = "💾 分组内的硬盘",
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            
            var rightLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                Padding = new Padding(5)
            };
            
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));
            
            // 硬盘列表
            lstDrivesInGroup = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10),
                IntegralHeight = false
            };
            
            // 硬盘操作按钮面板
            var driveButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 5, 0, 0)
            };
            
            btnAddDrive = new Button
            {
                Text = "➕ 添加硬盘",
                Size = new Size(110, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                Enabled = false
            };
            btnAddDrive.Click += BtnAddDrive_Click;
            
            btnRemoveDrive = new Button
            {
                Text = "➖ 移除硬盘",
                Size = new Size(110, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 9),
                Enabled = false
            };
            btnRemoveDrive.Click += BtnRemoveDrive_Click;
            
            driveButtonPanel.Controls.AddRange(new Control[] { btnAddDrive, btnRemoveDrive });
            
            rightLayout.Controls.Add(lstDrivesInGroup, 0, 0);
            rightLayout.Controls.Add(driveButtonPanel, 0, 1);
            
            rightGroupBox.Controls.Add(rightLayout);
            
            // ========== 底部：操作栏 ==========
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            
            var bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };
            
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            
            // 状态标签
            lblStatus = new Label
            {
                Text = "就绪",
                Font = new Font("微软雅黑", 9),
                ForeColor = Color.Gray,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            // 确认复选框
            chkConfirmOpen = new CheckBox
            {
                Text = "打开前确认",
                Font = new Font("微软雅黑", 9),
                Checked = true,
                AutoSize = true
            };
            
            // 打开分组按钮
            btnOpenGroup = new Button
            {
                Text = "🚀 打开此分组的所有硬盘",
                Size = new Size(200, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("微软雅黑", 10, FontStyle.Bold),
                Enabled = false
            };
            btnOpenGroup.Click += BtnOpenGroup_Click;
            
            bottomLayout.Controls.Add(lblStatus, 0, 0);
            bottomLayout.Controls.Add(chkConfirmOpen, 1, 0);
            bottomLayout.Controls.Add(btnOpenGroup, 2, 0);
            
            bottomPanel.Controls.Add(bottomLayout);
            
            // 组装主面板
            mainPanel.Controls.Add(leftGroupBox, 0, 0);
            mainPanel.Controls.Add(rightGroupBox, 1, 0);
            mainPanel.Controls.Add(bottomPanel, 0, 1);
            mainPanel.SetColumnSpan(bottomPanel, 2);  // 底部横跨两列
            
            this.Controls.Add(mainPanel);
        }
        
        /// <summary>
        /// 加载所有分组到列表
        /// </summary>
        private void LoadGroupsToList()
        {
            lstGroups.Items.Clear();
            var groups = manager.GetAllGroups();
            
            foreach (var group in groups)
            {
                lstGroups.Items.Add(group);
            }
            
            if (lstGroups.Items.Count > 0)
                lstGroups.SelectedIndex = 0;
        }
        
        /// <summary>
        /// 当选中分组改变时，更新右侧硬盘列表
        /// </summary>
        private void LstGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lstGroups.SelectedItem != null;
            btnEditGroup.Enabled = hasSelection;
            btnDeleteGroup.Enabled = hasSelection;
            btnOpenGroup.Enabled = hasSelection;
            btnAddDrive.Enabled = hasSelection;
            
            if (hasSelection)
            {
                var selectedGroup = (DriveGroup)lstGroups.SelectedItem;
                UpdateDriveList(selectedGroup);
                UpdateStatus($"当前选中: {selectedGroup.GroupName} (包含 {selectedGroup.DriveLetters.Count} 个硬盘)");
            }
            else
            {
                lstDrivesInGroup.Items.Clear();
                btnRemoveDrive.Enabled = false;
            }
        }
        
        /// <summary>
        /// 更新右侧硬盘列表
        /// </summary>
        private void UpdateDriveList(DriveGroup group)
        {
            lstDrivesInGroup.Items.Clear();
            
            foreach (string driveLetter in group.DriveLetters)
            {
                // 获取硬盘详细信息
                string driveInfo = GetDriveInfo(driveLetter);
                lstDrivesInGroup.Items.Add($"{driveLetter} - {driveInfo}");
            }
            
            btnRemoveDrive.Enabled = lstDrivesInGroup.Items.Count > 0;
        }
        
        /// <summary>
        /// 获取硬盘的详细信息（卷标、大小等）
        /// </summary>
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
                    return $"{label} ({totalGB} GB, 剩余 {freeGB} GB)";
                }
                else
                {
                    return "硬盘未就绪";
                }
            }
            catch
            {
                return "无法访问";
            }
        }
        
        /// <summary>
        /// 打开分组按钮点击事件
        /// </summary>
        private void BtnOpenGroup_Click(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItem == null)
                return;
            
            var selectedGroup = (DriveGroup)lstGroups.SelectedItem;
            
            // 如果需要确认
            if (chkConfirmOpen.Checked)
            {
                string drives = string.Join("\n  • ", selectedGroup.DriveLetters);
                var result = MessageBox.Show(
                    $"即将打开分组 [{selectedGroup.GroupName}] 中的以下硬盘:\n\n  • {drives}\n\n是否继续？",
                    "确认打开",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result != DialogResult.Yes)
                    return;
            }
            
            // 打开所有硬盘
            manager.OpenAllDrivesInGroup(selectedGroup.GroupName);
        }
        
        /// <summary>
        /// 新建分组按钮点击事件
        /// </summary>
        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            using (var dialog = new GroupEditDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var newGroup = new DriveGroup(
                        dialog.GroupName,
                        dialog.SelectedDrives,
                        dialog.Description,
                        0
                    );
                    
                    if (manager.AddGroup(newGroup))
                    {
                        LoadGroupsToList();
                        UpdateStatus($"成功添加分组: {newGroup.GroupName}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 编辑分组按钮点击事件
        /// </summary>
        private void BtnEditGroup_Click(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItem == null)
                return;
            
            var oldGroup = (DriveGroup)lstGroups.SelectedItem;
            
            using (var dialog = new GroupEditDialog(oldGroup))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var updatedGroup = new DriveGroup(
                        dialog.GroupName,
                        dialog.SelectedDrives,
                        dialog.Description,
                        oldGroup.IconIndex
                    );
                    
                    if (manager.UpdateGroup(oldGroup.GroupName, updatedGroup))
                    {
                        LoadGroupsToList();
                        UpdateStatus($"成功更新分组: {updatedGroup.GroupName}");
                    }
                }
            }
        }
        
        /// <summary>
        /// 删除分组按钮点击事件
        /// </summary>
        private void BtnDeleteGroup_Click(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItem == null)
                return;
            
            var group = (DriveGroup)lstGroups.SelectedItem;
            
            var result = MessageBox.Show(
                $"确定要删除分组 [{group.GroupName}] 吗？\n\n此操作不会影响硬盘中的数据。",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);
            
            if (result == DialogResult.Yes)
            {
                if (manager.DeleteGroup(group.GroupName))
                {
                    LoadGroupsToList();
                    UpdateStatus($"成功删除分组: {group.GroupName}");
                }
            }
        }
        
        /// <summary>
        /// 添加硬盘到分组
        /// </summary>
        private void BtnAddDrive_Click(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItem == null)
                return;
            
            var group = (DriveGroup)lstGroups.SelectedItem;
            
            using (var dialog = new DriveSelectionDialog(group.DriveLetters))
            {
                if (dialog.ShowDialog() == DialogResult.OK && dialog.SelectedDrive != null)
                {
                    group.DriveLetters.Add(dialog.SelectedDrive);
                    manager.UpdateGroup(group.GroupName, group);
                    
                    // 刷新显示
                    UpdateDriveList(group);
                    LoadGroupsToList();  // 刷新左侧列表显示
                    UpdateStatus($"已添加硬盘 {dialog.SelectedDrive} 到分组 {group.GroupName}");
                }
            }
        }
        
        /// <summary>
        /// 从分组移除硬盘
        /// </summary>
        private void BtnRemoveDrive_Click(object sender, EventArgs e)
        {
            if (lstGroups.SelectedItem == null || lstDrivesInGroup.SelectedItem == null)
                return;
            
            var group = (DriveGroup)lstGroups.SelectedItem;
            int selectedIndex = lstDrivesInGroup.SelectedIndex;
            
            if (selectedIndex >= 0 && selectedIndex < group.DriveLetters.Count)
            {
                string driveToRemove = group.DriveLetters[selectedIndex];
                
                var result = MessageBox.Show(
                    $"确定要从分组 [{group.GroupName}] 中移除硬盘 {driveToRemove} 吗？",
                    "确认移除",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                
                if (result == DialogResult.Yes)
                {
                    group.DriveLetters.RemoveAt(selectedIndex);
                    manager.UpdateGroup(group.GroupName, group);
                    
                    UpdateDriveList(group);
                    LoadGroupsToList();
                    UpdateStatus($"已从分组 {group.GroupName} 移除硬盘 {driveToRemove}");
                }
            }
        }
        
        /// <summary>
        /// 更新状态栏信息
        /// </summary>
        private void UpdateStatus(string message = null)
        {
            if (message != null)
                lblStatus.Text = message;
            else
                lblStatus.Text = $"就绪 | 共 {manager.GetAllGroups().Count} 个分组";
        }
    }
}