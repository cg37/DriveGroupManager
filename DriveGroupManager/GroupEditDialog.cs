using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DriveGroupManager
{
    /// <summary>
    /// 分组编辑对话框（用于新建和编辑分组）
    /// </summary>
    public partial class GroupEditDialog : Form
    {
        public string GroupName { get; private set; }
        public List<string> SelectedDrives { get; private set; }
        public string Description { get; private set; }
        
        private TextBox txtGroupName;
        private TextBox txtDescription;
        private CheckedListBox chkDriveList;
        private Button btnOK;
        private Button btnCancel;
        
        /// <summary>
        /// 构造函数 - 新建分组
        /// </summary>
        public GroupEditDialog()
        {
            InitializeComponent();
            SelectedDrives = new List<string>();
            LoadAvailableDrives();
        }
        
        /// <summary>
        /// 构造函数 - 编辑现有分组
        /// </summary>
        public GroupEditDialog(DriveGroup existingGroup)
        {
            InitializeComponent();
            SelectedDrives = new List<string>(existingGroup.DriveLetters);
            txtGroupName.Text = existingGroup.GroupName;
            txtDescription.Text = existingGroup.Description;
            LoadAvailableDrives();
            
            // 预选中已有的硬盘
            for (int i = 0; i < chkDriveList.Items.Count; i++)
            {
                string drive = chkDriveList.Items[i].ToString().Split(' ')[0];  // 提取盘符
                if (SelectedDrives.Contains(drive))
                    chkDriveList.SetItemChecked(i, true);
            }
        }
        
        private void InitializeComponent()
        {
            this.Text = "硬盘分组";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new Size(450, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                Padding = new Padding(10)
            };
            
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            
            // 分组名称
            mainPanel.Controls.Add(new Label { Text = "分组名称:", TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            txtGroupName = new TextBox { Dock = DockStyle.Fill, Font = new Font("微软雅黑", 10) };
            mainPanel.Controls.Add(txtGroupName, 1, 0);
            
            // 描述
            mainPanel.Controls.Add(new Label { Text = "描述:", TextAlign = ContentAlignment.MiddleRight }, 0, 1);
            txtDescription = new TextBox { Dock = DockStyle.Fill, Font = new Font("微软雅黑", 9) };
            mainPanel.Controls.Add(txtDescription, 1, 1);
            
            // 硬盘选择
            mainPanel.Controls.Add(new Label { Text = "选择硬盘:", TextAlign = ContentAlignment.MiddleRight }, 0, 2);
            
            chkDriveList = new CheckedListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("微软雅黑", 9),
                CheckOnClick = true,
                Height = 200
            };
            mainPanel.Controls.Add(chkDriveList, 1, 2);
            
            // 提示信息
            var tipLabel = new Label
            {
                Text = "提示：可以同时选择多个硬盘",
                ForeColor = Color.Gray,
                Font = new Font("微软雅黑", 8),
                TextAlign = ContentAlignment.MiddleLeft
            };
            mainPanel.Controls.Add(tipLabel, 1, 3);
            
            // 按钮面板
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 5, 0, 0)
            };
            
            btnOK = new Button
            {
                Text = "确定",
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOK.Click += BtnOK_Click;
            
            btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(80, 30),
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;
            
            buttonPanel.Controls.AddRange(new Control[] { btnOK, btnCancel });
            mainPanel.Controls.Add(buttonPanel, 1, 4);
            
            this.Controls.Add(mainPanel);
        }
        
        /// <summary>
        /// 加载系统中所有可用的固定硬盘
        /// </summary>
        private void LoadAvailableDrives()
        {
            chkDriveList.Items.Clear();
            
            var drives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .OrderBy(d => d.Name);
            
            foreach (var drive in drives)
            {
                string driveInfo = $"{drive.Name} - {GetDriveDescription(drive)}";
                chkDriveList.Items.Add(driveInfo);
            }
            
            if (chkDriveList.Items.Count == 0)
            {
                chkDriveList.Items.Add("未检测到可用硬盘");
                chkDriveList.Enabled = false;
            }
        }
        
        /// <summary>
        /// 获取硬盘描述信息
        /// </summary>
        private string GetDriveDescription(DriveInfo drive)
        {
            try
            {
                string label = string.IsNullOrEmpty(drive.VolumeLabel) ? "本地磁盘" : drive.VolumeLabel;
                long totalGB = drive.TotalSize / (1024 * 1024 * 1024);
                return $"{label} ({totalGB} GB)";
            }
            catch
            {
                return "无法获取信息";
            }
        }
        
        /// <summary>
        /// 确定按钮点击事件
        /// </summary>
        private void BtnOK_Click(object sender, EventArgs e)
        {
            // 验证分组名称
            if (string.IsNullOrWhiteSpace(txtGroupName.Text))
            {
                MessageBox.Show("请输入分组名称。", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGroupName.Focus();
                return;
            }
            
            // 收集选中的硬盘
            SelectedDrives.Clear();
            for (int i = 0; i < chkDriveList.CheckedItems.Count; i++)
            {
                string item = chkDriveList.CheckedItems[i].ToString();
                // 提取盘符（格式如 "C:\ - ..."）
                string driveLetter = item.Split(' ')[0];
                SelectedDrives.Add(driveLetter);
            }
            
            if (SelectedDrives.Count == 0)
            {
                MessageBox.Show("请至少选择一个硬盘。", "提示", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            GroupName = txtGroupName.Text.Trim();
            Description = txtDescription.Text.Trim();
            
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}