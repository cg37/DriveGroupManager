using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DriveGroupManager
{
    public class EditGroupsForm : Form
    {
        public List<DriveGroup> UpdatedGroups { get; private set; }
        private List<DriveGroup> workingGroups;
        private ListBox lbGroups;
        private ListBox lbDrivesInGroup;
        private ListBox lbAvailableDrives;
        private Button btnAddGroup;
        private Button btnDeleteGroup;
        private Button btnAddDrive;
        private Button btnRemoveDrive;
        private Button btnSave;
        private Button btnCancel;
        private TextBox txtGroupName;
        private TextBox txtDescription;

        public EditGroupsForm(List<DriveGroup> existingGroups)
        {
            // 深拷贝现有分组
            workingGroups = existingGroups.Select(g => new DriveGroup
            {
                GroupName = g.GroupName,
                DriveLetters = new List<string>(g.DriveLetters),
                Description = g.Description,
                IconIndex = g.IconIndex
            }).ToList();
            
            UpdatedGroups = workingGroups;
            InitializeComponent();
            LoadGroups();
        }

        private void InitializeComponent()
        {
            this.Text = "编辑硬盘分组";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(800, 500);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10)
            };
            
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 5F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));

            // 左侧：分组列表
            var groupPanel = new GroupBox { Text = "分组列表", Dock = DockStyle.Fill };
            lbGroups = new ListBox { Dock = DockStyle.Fill, Font = new Font("微软雅黑", 10) };
            lbGroups.SelectedIndexChanged += LbGroups_SelectedIndexChanged;
            groupPanel.Controls.Add(lbGroups);

            var groupButtonPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 35 };
            btnAddGroup = new Button { Text = "➕ 新建分组", Size = new Size(90, 30) };
            btnDeleteGroup = new Button { Text = "🗑️ 删除", Size = new Size(70, 30), Enabled = false };
            btnAddGroup.Click += BtnAddGroup_Click;
            btnDeleteGroup.Click += BtnDeleteGroup_Click;
            groupButtonPanel.Controls.AddRange(new Control[] { btnAddGroup, btnDeleteGroup });
            groupPanel.Controls.Add(groupButtonPanel);

            // 中间：分组信息编辑
            var editPanel = new GroupBox { Text = "分组信息", Dock = DockStyle.Fill };
            var editLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, Padding = new Padding(5) };
            editLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            editLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            editLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
            editLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            
            editLayout.Controls.Add(new Label { Text = "分组名称:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            txtGroupName = new TextBox { Dock = DockStyle.Fill };
            editLayout.Controls.Add(txtGroupName, 1, 0);
            editLayout.SetColumnSpan(txtGroupName, 2);
            
            editLayout.Controls.Add(new Label { Text = "描述:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            txtDescription = new TextBox { Dock = DockStyle.Fill };
            editLayout.Controls.Add(txtDescription, 1, 1);
            editLayout.SetColumnSpan(txtDescription, 2);
            
            editPanel.Controls.Add(editLayout);

            // 右侧上：分组内硬盘
            var drivesInGroupPanel = new GroupBox { Text = "分组内的硬盘", Dock = DockStyle.Fill };
            lbDrivesInGroup = new ListBox { Dock = DockStyle.Fill, Font = new Font("微软雅黑", 9) };
            drivesInGroupPanel.Controls.Add(lbDrivesInGroup);
            
            // 右侧下：可用硬盘
            var availableDrivesPanel = new GroupBox { Text = "可用的硬盘", Dock = DockStyle.Fill };
            lbAvailableDrives = new ListBox { Dock = DockStyle.Fill, Font = new Font("微软雅黑", 9) };
            availableDrivesPanel.Controls.Add(lbAvailableDrives);
            
            var rightPanel = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 250
            };
            rightPanel.Panel1.Controls.Add(drivesInGroupPanel);
            rightPanel.Panel2.Controls.Add(availableDrivesPanel);
            
            // 中间按钮
            var middlePanel = new Panel { Dock = DockStyle.Fill };
            var middleFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(5)
            };
            
            btnAddDrive = new Button { Text = "▶ 添加", Size = new Size(60, 30), Enabled = false };
            btnRemoveDrive = new Button { Text = "◀ 移除", Size = new Size(60, 30), Enabled = false };
            btnAddDrive.Click += BtnAddDrive_Click;
            btnRemoveDrive.Click += BtnRemoveDrive_Click;
            middleFlow.Controls.AddRange(new Control[] { btnAddDrive, btnRemoveDrive });
            middlePanel.Controls.Add(middleFlow);
            
            // 底部按钮
            var buttonPanel = new Panel { Dock = DockStyle.Fill };
            var buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };
            
            btnSave = new Button { Text = "保存", Size = new Size(100, 35), BackColor = Color.FromArgb(46, 204, 113), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnCancel = new Button { Text = "取消", Size = new Size(100, 35), FlatStyle = FlatStyle.Flat };
            btnSave.Click += (s, e) => { this.DialogResult = DialogResult.OK; Close(); };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; Close(); };
            
            buttonFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            buttonPanel.Controls.Add(buttonFlow);
            
            mainPanel.Controls.Add(groupPanel, 0, 0);
            mainPanel.Controls.Add(middlePanel, 1, 0);
            mainPanel.Controls.Add(rightPanel, 2, 0);
            mainPanel.Controls.Add(buttonPanel, 0, 1);
            mainPanel.SetColumnSpan(buttonPanel, 3);
            
            this.Controls.Add(mainPanel);
            
            LoadAvailableDrives();
        }

        private void LoadGroups()
        {
            lbGroups.Items.Clear();
            foreach (var group in workingGroups)
            {
                lbGroups.Items.Add(group);
            }
        }

        private void LoadAvailableDrives()
        {
            var allDrives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.Name.TrimEnd('\\'))
                .ToList();
            
            var usedDrives = workingGroups.SelectMany(g => g.DriveLetters).ToList();
            var available = allDrives.Except(usedDrives).ToList();
            
            lbAvailableDrives.Items.Clear();
            foreach (var drive in available)
            {
                lbAvailableDrives.Items.Add(drive);
            }
        }

        private void LbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lbGroups.SelectedItem != null;
            btnDeleteGroup.Enabled = hasSelection;
            btnAddDrive.Enabled = hasSelection;
            btnRemoveDrive.Enabled = hasSelection;
            
            if (hasSelection)
            {
                var group = (DriveGroup)lbGroups.SelectedItem;
                txtGroupName.Text = group.GroupName;
                txtDescription.Text = group.Description;
                
                lbDrivesInGroup.Items.Clear();
                foreach (string drive in group.DriveLetters)
                {
                    lbDrivesInGroup.Items.Add(drive);
                }
            }
            else
            {
                txtGroupName.Text = "";
                txtDescription.Text = "";
                lbDrivesInGroup.Items.Clear();
            }
        }

        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            string newName = $"新分组 {workingGroups.Count + 1}";
            var newGroup = new DriveGroup(newName, new List<string>(), "", 0);
            workingGroups.Add(newGroup);
            LoadGroups();
            lbGroups.SelectedItem = newGroup;
        }

        private void BtnDeleteGroup_Click(object sender, EventArgs e)
        {
            if (lbGroups.SelectedItem is DriveGroup group)
            {
                if (MessageBox.Show($"确定删除分组「{group.GroupName}」吗？", "确认", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    workingGroups.Remove(group);
                    LoadGroups();
                    LoadAvailableDrives();
                }
            }
        }

        private void BtnAddDrive_Click(object sender, EventArgs e)
        {
            if (lbGroups.SelectedItem is DriveGroup group && lbAvailableDrives.SelectedItem is string drive)
            {
                group.DriveLetters.Add(drive);
                RefreshDisplayForGroup(group);
                LoadAvailableDrives();
            }
        }

        private void BtnRemoveDrive_Click(object sender, EventArgs e)
        {
            if (lbGroups.SelectedItem is DriveGroup group && lbDrivesInGroup.SelectedItem is string drive)
            {
                group.DriveLetters.Remove(drive);
                RefreshDisplayForGroup(group);
                LoadAvailableDrives();
            }
        }

        private void RefreshDisplayForGroup(DriveGroup group)
        {
            // 更新分组显示
            int index = lbGroups.Items.IndexOf(group);
            if (index >= 0)
            {
                lbGroups.Items[index] = group;
            }
            
            // 更新硬盘列表
            lbDrivesInGroup.Items.Clear();
            foreach (string drive in group.DriveLetters)
            {
                lbDrivesInGroup.Items.Add(drive);
            }
            
            // 更新分组名称
            if (lbGroups.SelectedItem == group)
            {
                group.GroupName = txtGroupName.Text;
                group.Description = txtDescription.Text;
            }
        }
    }
}