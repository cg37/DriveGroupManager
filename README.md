# 硬盘分组管理器 (DriveGroupManager)

一款 Windows 桌面工具，用于将电脑中的多块硬盘按用途组织为自定义分组，方便集中管理和快速访问。

## 功能

- **可视化分组** — 以树形结构展示分组与硬盘的层级关系，支持展开/折叠
- **自定义分组管理** — 新建、删除、重命名分组，自由分配硬盘成员
- **空间预警** — 根据剩余空间百分比自动切换颜色：绿色（充足）、橙色（紧张）、红色（不足）
- **一键打开** — 双击硬盘节点或点击按钮，直接唤起资源管理器
- **数据持久化** — 分组配置以 JSON 格式保存在本地，启动时自动加载

## 界面预览

> 待补充截图

## 下载安装

从 [Releases](https://github.com/OWNER/DriveGroupManager/releases) 页面下载最新版本的 `DriveGroupManager-vX.X.X.zip`，解压后双击 `DriveGroupManager.exe` 即可运行。

**系统要求**
- Windows 10 / Windows 11 (64 位)
- [.NET 10.0 运行时](https://dotnet.microsoft.com/download/dotnet/10.0)

## 从源码构建

```bash
# 克隆仓库
git clone https://github.com/OWNER/DriveGroupManager.git
cd DriveGroupManager

# 还原依赖并编译
dotnet restore
dotnet build -c Release

# 发布为单文件可执行程序
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish
```

## 使用说明

1. 启动后自动读取本地硬盘并加载已保存的分组配置
2. 点击工具栏「编辑分组设置」进入管理界面
3. 在管理界面中可以新建分组，将可用硬盘加入或移出分组
4. 双击任意硬盘节点即可在资源管理器中打开对应盘符
5. 分组数据自动保存在 `%LocalAppData%\DriveGroupManager\groups.json`

## 技术栈

| 类别 | 技术 |
|------|------|
| 框架 | .NET 10.0 (Windows Forms) |
| 语言 | C# |
| 序列化 | System.Text.Json |
| 构建 | dotnet CLI + GitHub Actions |
| DPI 适配 | PerMonitorV2 + AutoScaleMode.Font |

## 项目结构

```
DriveGroupManager/
├── DriveGroupManager.csproj   # 项目文件
├── app.manifest               # Windows DPI 感知声明
├── Program.cs                 # 应用入口，DPI 初始化
├── MainForm.cs                # 主窗体（树形视图、工具栏）
├── EditGroupsForm.cs          # 分组编辑窗体
├── DriveGroup.cs              # 分组数据模型
├── DriveGroupManager.cs       # 业务逻辑（JSON 读写、硬盘操作）
└── .github/workflows/         # CI/CD 自动构建与发布
```

## 许可证

MIT
