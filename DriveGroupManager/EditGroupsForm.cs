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
    public class EditGroupsForm : MaterialForm
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
        private MaterialTextBox txtGroupName;
        private MaterialTextBox txtDescription;

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

            // 初始化 MaterialSkin 主题
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue600,
                Primary.Blue700,
                Primary.Blue200,
                Accent.Cyan200,
                TextShade.WHITE
            );

            LoadGroups();

            // 关键：初始化后立即刷新可用硬盘列表
            RefreshAllDriveLists();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.Text = "编辑硬盘分组";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(950, 600);

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                Padding = new Padding(10)
            };

            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));  // 固定宽度给按钮
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 85F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 15F));

            // ========== 左侧：分组列表 ==========
            var groupPanel = new GroupBox
            {
                Text = "📁 分组列表",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };

            lbGroups = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10F),
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            lbGroups.DrawItem += LbGroups_DrawItem;
            lbGroups.SelectedIndexChanged += LbGroups_SelectedIndexChanged;

            var groupButtonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Padding = new Padding(5)
            };
            btnAddGroup = new MaterialButton
            {
                Text = "新建分组",
                AutoSize = false,
                Size = new Size(100, 36),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false
            };
            btnDeleteGroup = new MaterialButton
            {
                Text = "删除",
                AutoSize = false,
                Size = new Size(80, 36),
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Outlined
            };
            btnAddGroup.Click += BtnAddGroup_Click;
            btnDeleteGroup.Click += BtnDeleteGroup_Click;
            groupButtonPanel.Controls.AddRange(new Control[] { btnAddGroup, btnDeleteGroup });

            groupPanel.Controls.Add(lbGroups);
            groupPanel.Controls.Add(groupButtonPanel);

            // ========== 中间：操作按钮 ==========
            var middlePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Padding = new Padding(0)
            };
            middlePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            middlePanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            middlePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            var buttonContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(5),
                AutoSize = true,
                Anchor = AnchorStyles.None
            };

            btnAddDrive = new MaterialButton
            {
                Text = "添加 →",
                AutoSize = false,
                Size = new Size(90, 40),
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true,
                Margin = new Padding(0, 0, 0, 10)
            };
            btnRemoveDrive = new MaterialButton
            {
                Text = "← 移除",
                AutoSize = false,
                Size = new Size(90, 40),
                Enabled = false,
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = false
            };
            btnAddDrive.Click += BtnAddDrive_Click;
            btnRemoveDrive.Click += BtnRemoveDrive_Click;

            buttonContainer.Controls.AddRange(new Control[] { btnAddDrive, btnRemoveDrive });
            middlePanel.Controls.Add(buttonContainer, 0, 1);

            // ========== 右侧：硬盘管理 ==========
            var rightPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

            // 分组内硬盘
            var drivesInGroupPanel = new GroupBox
            {
                Text = "📀 分组内的硬盘",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };
            lbDrivesInGroup = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10F),
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            lbDrivesInGroup.DrawItem += LbDrives_DrawItem;
            drivesInGroupPanel.Controls.Add(lbDrivesInGroup);

            // 可用硬盘
            var availableDrivesPanel = new GroupBox
            {
                Text = "💾 可用的硬盘（双击添加到分组）",
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };
            lbAvailableDrives = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 10F),
                IntegralHeight = false,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            lbAvailableDrives.DrawItem += LbDrives_DrawItem;
            lbAvailableDrives.DoubleClick += (s, e) =>
            {
                if (btnAddDrive.Enabled && lbAvailableDrives.SelectedItem != null)
                {
                    BtnAddDrive_Click(s, e);
                }
            };
            availableDrivesPanel.Controls.Add(lbAvailableDrives);

            rightPanel.Controls.Add(drivesInGroupPanel, 0, 0);
            rightPanel.Controls.Add(availableDrivesPanel, 0, 1);

            // ========== 底部：分组信息编辑栏 ==========
            var bottomPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            var bottomLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Padding = new Padding(5)
            };
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60F));
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));

            bottomLayout.Controls.Add(new Label
            {
                Text = "分组名称:",
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("微软雅黑", 9F)
            }, 0, 0);

            txtGroupName = new MaterialTextBox
            {
                Dock = DockStyle.Fill,
                Enabled = false,
                Hint = "分组名称"
            };
            bottomLayout.Controls.Add(txtGroupName, 1, 0);

            bottomLayout.Controls.Add(new MaterialLabel
            {
                Text = "描述:",
                Dock = DockStyle.Fill,
                FontType = MaterialSkin.MaterialSkinManager.fontType.Body2,
                TextAlign = ContentAlignment.MiddleLeft
            }, 2, 0);

            txtDescription = new MaterialTextBox
            {
                Dock = DockStyle.Fill,
                Enabled = false,
                Hint = "分组描述"
            };
            bottomLayout.Controls.Add(txtDescription, 3, 0);

            bottomPanel.Controls.Add(bottomLayout);

            // ========== 底部按钮 ==========
            var buttonPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(5) };
            var buttonFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(5)
            };

            btnSave = new MaterialButton
            {
                Text = "保存",
                AutoSize = false,
                Size = new Size(100, 40),
                Type = MaterialButton.MaterialButtonType.Contained,
                UseAccentColor = true
            };
            btnCancel = new MaterialButton
            {
                Text = "取消",
                AutoSize = false,
                Size = new Size(100, 40),
                Type = MaterialButton.MaterialButtonType.Outlined
            };
            btnSave.Click += (s, e) =>
            {
                // 保存前更新当前分组信息
                if (lbGroups.SelectedItem is DriveGroup group)
                {
                    group.GroupName = txtGroupName.Text;
                    group.Description = txtDescription.Text;
                }
                this.DialogResult = DialogResult.OK;
                Close();
            };
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; Close(); };

            buttonFlow.Controls.AddRange(new Control[] { btnSave, btnCancel });
            buttonPanel.Controls.Add(buttonFlow);

            // 组装主面板
            mainPanel.Controls.Add(groupPanel, 0, 0);
            mainPanel.Controls.Add(middlePanel, 1, 0);
            mainPanel.Controls.Add(rightPanel, 2, 0);
            mainPanel.Controls.Add(bottomPanel, 0, 1);
            mainPanel.SetColumnSpan(bottomPanel, 2);
            mainPanel.Controls.Add(buttonPanel, 2, 1);

            this.Controls.Add(mainPanel);
        }

        private void LoadGroups()
        {
            lbGroups.Items.Clear();
            foreach (var group in workingGroups)
            {
                lbGroups.Items.Add(group);
            }
            
            // 如果有分组，默认选中第一个
            if (lbGroups.Items.Count > 0)
            {
                lbGroups.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// 刷新所有硬盘列表（可用硬盘 + 当前分组内硬盘）
        /// </summary>
        private void RefreshAllDriveLists()
        {
            RefreshAvailableDrives();
            
            if (lbGroups.SelectedItem is DriveGroup selectedGroup)
            {
                RefreshDrivesInGroup(selectedGroup);
            }
        }

        /// <summary>
        /// 刷新可用硬盘列表（只显示未被任何分组使用的硬盘）
        /// </summary>
        private void RefreshAvailableDrives()
        {
            // 获取所有固定硬盘
            var allDrives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Select(d => d.Name.TrimEnd('\\'))
                .ToList();

            // 获取已被使用的硬盘
            var usedDrives = workingGroups.SelectMany(g => g.DriveLetters).Distinct().ToList();
            
            // 可用硬盘 = 所有硬盘 - 已使用硬盘
            var available = allDrives.Except(usedDrives).ToList();

            lbAvailableDrives.Items.Clear();
            foreach (var drive in available)
            {
                string displayText = GetDriveDisplayText(drive);
                lbAvailableDrives.Items.Add(displayText);
            }
        }

        /// <summary>
        /// 刷新指定分组内的硬盘列表
        /// </summary>
        private void RefreshDrivesInGroup(DriveGroup group)
        {
            lbDrivesInGroup.Items.Clear();
            foreach (string drive in group.DriveLetters)
            {
                string displayText = GetDriveDisplayText(drive);
                lbDrivesInGroup.Items.Add(displayText);
            }
        }

        /// <summary>
        /// 获取硬盘的显示文本（带详细信息）
        /// </summary>
        private string GetDriveDisplayText(string driveLetter)
        {
            try
            {
                var drive = new DriveInfo(driveLetter);
                if (drive.IsReady)
                {
                    string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "本地磁盘" : drive.VolumeLabel;
                    long totalGB = drive.TotalSize / (1024 * 1024 * 1024);
                    long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
                    return $"{driveLetter}  [{label}]  {totalGB}GB (剩余 {freeGB}GB)";
                }
                return $"{driveLetter}  [不可访问]";
            }
            catch
            {
                return $"{driveLetter}  [无法读取]";
            }
        }

        /// <summary>
        /// 从显示文本中提取盘符
        /// </summary>
        private string ExtractDriveLetter(string displayText)
        {
            if (string.IsNullOrEmpty(displayText)) return "";
            // 盘符格式如 "C:"，提取前两个字符
            if (displayText.Length >= 2 && displayText[1] == ':')
            {
                return displayText.Substring(0, 2);
            }
            return "";
        }

        private void LbGroups_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var lb = (ListBox)sender;

            // 绘制背景
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(25, 118, 210)), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(lb.BackColor), e.Bounds);
            }

            // 绘制文本
            if (lb.Items[e.Index] is DriveGroup group)
            {
                string text = group.ToString();
                Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? Color.White
                    : Color.FromArgb(33, 33, 33);

                TextRenderer.DrawText(e.Graphics, text, lb.Font,
                    new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
                    textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            }

            e.DrawFocusRectangle();
        }

        private void LbDrives_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var lb = (ListBox)sender;

            // 绘制背景
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(25, 118, 210)), e.Bounds);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(lb.BackColor), e.Bounds);
            }

            // 绘制文本
            string text = lb.Items[e.Index]?.ToString() ?? "";
            Color textColor = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                ? Color.White
                : Color.FromArgb(33, 33, 33);

            TextRenderer.DrawText(e.Graphics, text, lb.Font,
                new Rectangle(e.Bounds.X + 4, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height),
                textColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);

            e.DrawFocusRectangle();
        }

        private void LbGroups_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool hasSelection = lbGroups.SelectedItem != null;
            btnDeleteGroup.Enabled = hasSelection;
            btnAddDrive.Enabled = hasSelection;
            btnRemoveDrive.Enabled = hasSelection;
            txtGroupName.Enabled = hasSelection;
            txtDescription.Enabled = hasSelection;

            if (hasSelection)
            {
                var group = (DriveGroup)lbGroups.SelectedItem;
                txtGroupName.Text = group.GroupName;
                txtDescription.Text = group.Description;
                RefreshDrivesInGroup(group);
            }
            else
            {
                txtGroupName.Text = "";
                txtDescription.Text = "";
                lbDrivesInGroup.Items.Clear();
            }
            
            // 切换分组时刷新可用硬盘列表
            RefreshAvailableDrives();
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
                if (MessageBox.Show($"确定删除分组「{group.GroupName}」吗？\n\n分组内的硬盘不会被删除，将变为未分组状态。", 
                    "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    workingGroups.Remove(group);
                    LoadGroups();
                    
                    // 刷新可用硬盘列表
                    RefreshAvailableDrives();
                    
                    // 清空右侧显示
                    lbDrivesInGroup.Items.Clear();
                    txtGroupName.Text = "";
                    txtDescription.Text = "";
                }
            }
        }

        private void BtnAddDrive_Click(object sender, EventArgs e)
        {
            if (lbGroups.SelectedItem is not DriveGroup group) 
            {
                MessageBox.Show("请先选择一个分组", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (lbAvailableDrives.SelectedItem is not string selectedItem) 
            {
                MessageBox.Show("请先在「可用的硬盘」中选择一个硬盘", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string driveLetter = ExtractDriveLetter(selectedItem);
            if (string.IsNullOrEmpty(driveLetter)) 
            {
                MessageBox.Show("无法识别硬盘盘符", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // 检查是否已在分组中
            if (group.DriveLetters.Contains(driveLetter))
            {
                MessageBox.Show($"硬盘 {driveLetter} 已经在当前分组中。", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // 添加到分组
            group.DriveLetters.Add(driveLetter);
            
            // 刷新显示
            RefreshDrivesInGroup(group);
            RefreshAvailableDrives();
            
            // 重新选中同一个分组（保持选中状态）
            int index = workingGroups.IndexOf(group);
            if (index >= 0)
            {
                lbGroups.SelectedIndex = index;
            }
        }

        private void BtnRemoveDrive_Click(object sender, EventArgs e)
        {
            if (lbGroups.SelectedItem is not DriveGroup group) 
            {
                MessageBox.Show("请先选择一个分组", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (lbDrivesInGroup.SelectedItem is not string selectedItem) 
            {
                MessageBox.Show("请先在「分组内的硬盘」中选择一个硬盘", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string driveLetter = ExtractDriveLetter(selectedItem);
            if (string.IsNullOrEmpty(driveLetter)) 
            {
                MessageBox.Show("无法识别硬盘盘符", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            // 从分组移除
            if (group.DriveLetters.Contains(driveLetter))
            {
                group.DriveLetters.Remove(driveLetter);
                
                // 刷新显示
                RefreshDrivesInGroup(group);
                RefreshAvailableDrives();
                
                // 重新选中同一个分组
                int index = workingGroups.IndexOf(group);
                if (index >= 0)
                {
                    lbGroups.SelectedIndex = index;
                }
            }
        }
    }
}