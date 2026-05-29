using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using DriveGroupManager.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DriveGroupManager
{
    static class Program
    {
        private static IHost? _apiHost;

        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 启动后端 API 服务
            StartApiService();

            // 启动 WinForms 应用
            Application.Run(new MainForm());

            // 关闭 API 服务
            StopApiService();
        }

        private static void StartApiService()
        {
            var builder = WebApplication.CreateBuilder();

            // 配置服务
            builder.Services.AddSingleton<IDriveService, DriveService>();
            builder.Services.AddControllers();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // 配置 Kestrel 监听端口
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(5000);
            });

            var app = builder.Build();

            app.UseCors("AllowAll");
            app.UseAuthorization();
            app.MapControllers();

            _apiHost = app;

            // 在后台线程启动 API
            Task.Run(async () =>
            {
                await app.RunAsync();
            });
        }

        private static void StopApiService()
        {
            if (_apiHost != null)
            {
                _apiHost.StopAsync().Wait();
                _apiHost.Dispose();
            }
        }
    }
}
