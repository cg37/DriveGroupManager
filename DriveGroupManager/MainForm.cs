using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin;
using MaterialSkin.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace DriveGroupManager
{
    public partial class MainForm : MaterialForm
    {
        private WebView2 webView;
        private readonly MaterialSkinManager materialSkinManager;

        public MainForm()
        {
            InitializeComponent();

            // 初始化 MaterialSkin 主题管理器
            materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(
                Primary.Blue600,
                Primary.Blue700,
                Primary.Blue200,
                Accent.Cyan200,
                TextShade.WHITE
            );

            // 初始化 WebView2
            InitializeWebView();
        }

        private void InitializeComponent()
        {
            this.AutoScaleMode = AutoScaleMode.Font;
            this.AutoScaleDimensions = new SizeF(96F, 96F);
            this.Font = new Font("微软雅黑", 12F);
            this.Text = "硬盘分组管理器";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(900, 600);
        }

        private async void InitializeWebView()
        {
            webView = new WebView2
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };

            this.Controls.Add(webView);

            // 等待 WebView2 初始化
            await webView.EnsureCoreWebView2Async();

            // 配置 WebView2 设置
            webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
            webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
            webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

            // 注册 JS 互操作对象
            webView.CoreWebView2.AddHostObjectToScript("nativeApp", new NativeBridge(this));

            // 加载前端页面
            string webPath = GetWebPath();
            if (Directory.Exists(webPath))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "drivegroupmanager.local",
                    webPath,
                    CoreWebView2HostResourceAccessKind.Allow);
                webView.Source = new Uri("http://drivegroupmanager.local/index.html");
            }
            else
            {
                // 开发模式：加载开发服务器
                webView.Source = new Uri("http://localhost:3000");
            }
        }

        private string GetWebPath()
        {
            // 发布后的前端文件路径
            string[] possiblePaths = new[]
            {
                Path.Combine(Application.StartupPath, "web"),
                Path.Combine(Application.StartupPath, "..", "..", "..", "drive-group-web", "dist"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "web"),
            };

            foreach (var path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 供 JS 调用的本地方法
        /// </summary>
        [System.Runtime.InteropServices.ComVisible(true)]
        public class NativeBridge
        {
            private readonly MainForm _form;

            public NativeBridge(MainForm form)
            {
                _form = form;
            }

            /// <summary>
            /// 打开硬盘
            /// </summary>
            public void OpenDrive(string driveLetter)
            {
                _form.Invoke(new Action(() =>
                {
                    try
                    {
                        string path = driveLetter.EndsWith("\\") ? driveLetter : driveLetter + "\\";
                        if (Directory.Exists(path))
                        {
                            System.Diagnostics.Process.Start("explorer.exe", path);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"打开失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }

            /// <summary>
            /// 获取应用版本
            /// </summary>
            public string GetVersion()
            {
                return Application.ProductVersion;
            }
        }
    }
}
