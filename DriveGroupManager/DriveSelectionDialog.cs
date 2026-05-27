using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DriveGroupManager
{
    /// <summary>
    /// 单个硬盘选择对话框
    /// </summary>
    public class DriveSelectionDialog : Form
    {
        public string SelectedDrive { get; private set; }
        private ComboBox cboDrives;
        private Button btnOK;
        private Button btnCancel;
        private List<string> excludeDrives;
        
        public DriveSelectionDialog(List<string> existingDrives)
        {
            excludeDrives = existingDrives ?? new List<string>();
            InitializeComponent();
            LoadAvailableDrives();
        }
        
        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.Text = "选择硬盘";
            this.Size = new Size(350, 150);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(10)
            };
            
            panel.Controls.Add(new Label { Text = "选择要添加的硬盘:", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            panel.SetColumnSpan(panel.Controls[0], 2);
            
            cboDrives = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("微软雅黑", 10)
            };
            panel.Controls.Add(cboDrives, 0, 1);
            panel.SetColumnSpan(cboDrives, 2);
            
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft
            };
            
            btnOK = new Button
            {
                Text = "确定",
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(46, 204, 113),
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
            panel.Controls.Add(buttonPanel, 0, 2);
            panel.SetColumnSpan(buttonPanel, 2);
            
            this.Controls.Add(panel);
        }
        
        private void LoadAvailableDrives()
        {
            var availableDrives = DriveInfo.GetDrives()
                .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                .Where(d => !excludeDrives.Contains(d.Name.TrimEnd('\\')))
                .Select(d => new { Drive = d.Name.TrimEnd('\\'), Info = GetDriveInfo(d) })
                .ToList();
            
            foreach (var drive in availableDrives)
            {
                cboDrives.Items.Add($"{drive.Drive} - {drive.Info}");
            }
            
            if (cboDrives.Items.Count > 0)
                cboDrives.SelectedIndex = 0;
            else
                btnOK.Enabled = false;
        }
        
        private string GetDriveInfo(DriveInfo drive)
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
        
        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (cboDrives.SelectedItem != null)
            {
                SelectedDrive = cboDrives.SelectedItem.ToString().Split(' ')[0];
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}