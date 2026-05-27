using System;
using System.Windows.Forms;

namespace DriveGroupManager
{
    /// <summary>
    /// 应用程序主入口类
    /// </summary>
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 启用应用程序的视觉样式（让控件看起来更现代）
            Application.EnableVisualStyles();
            
            // 设置控件使用兼容的文本渲染（避免字体模糊）
            Application.SetCompatibleTextRenderingDefault(false);
            
            // 启动主窗体
            Application.Run(new MainForm());
        }
    }
}