# 硬盘分组管理器 (DriveGroupManager)

一款 Windows 桌面工具，用于将电脑中的多块硬盘按用途组织为自定义分组，方便集中管理和快速访问。

**架构：WinForms + WebView2 + React + ASP.NET Core Web API**

## 功能

- **可视化分组** — 以卡片形式展示分组与硬盘的层级关系
- **自定义分组管理** — 新建、删除、重命名分组，自由分配硬盘成员
- **空间预警** — 根据剩余空间百分比自动切换颜色：绿色（充足）、橙色（紧张）、红色（不足）
- **一键打开** — 点击硬盘卡片即可唤起资源管理器
- **数据持久化** — 分组配置以 JSON 格式保存在本地，启动时自动加载

## 架构说明

本项目采用混合架构：

```
┌─────────────────────────────────────────────────────────┐
│                  WinForms 宿主窗口                        │
│  ┌─────────────────────────────────────────────────────┐│
│  │              WebView2 (Edge Chromium)               ││
│  │  ┌───────────────────────────────────────────────┐  ││
│  │  │           React 前端 (Ant Design)             │  ││
│  │  │                                               │  ││
│  │  │  • 分组卡片展示                                │  ││
│  │  │  • 硬盘容量可视化                              │  ││
│  │  │  • 分组编辑对话框                              │  ││
│  │  └───────────────────────────────────────────────┘  ││
│  └─────────────────────────────────────────────────────┘│
│                          ↑↓ HTTP                        │
│  ┌─────────────────────────────────────────────────────┐│
│  │         ASP.NET Core Web API (localhost:5000)      ││
│  │  • DriveService (硬盘信息获取)                     ││
│  │  • GroupsController (分组CRUD)                     ││
│  │  • DrivesController (硬盘操作)                     ││
│  └─────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────┘
```

## 系统要求

- Windows 10 / Windows 11 (64 位)
- [.NET 10.0 运行时](https://dotnet.microsoft.com/download/dotnet/10.0)
- WebView2 运行时（通常已预装在 Windows 10/11 中）
- Node.js 18+（仅开发时需要）

## 从源码构建

### 1. 克隆仓库

```bash
git clone https://github.com/OWNER/DriveGroupManager.git
cd DriveGroupManager
```

### 2. 构建后端和 WinForms

```bash
dotnet restore
dotnet build -c Release
```

### 3. 构建前端（开发模式）

```bash
cd drive-group-web
npm install
npm run dev
```

### 4. 构建前端（生产模式）

```bash
cd drive-group-web
npm install
npm run build
```

### 5. 发布完整应用

```bash
# 前端构建
cd drive-group-web
npm run build

# 复制前端文件到 WinForms 项目
cp -r dist/* ../DriveGroupManager/bin/Release/net10.0-windows/web/

# 发布 WinForms
cd ..
dotnet publish DriveGroupManager -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./publish
```

## 开发模式运行

### 方式一：同时启动前后端

```bash
# 终端 1：启动后端 API
dotnet run --project DriveGroupManager.Api

# 终端 2：启动前端开发服务器
cd drive-group-web
npm run dev

# 终端 3：启动 WinForms（会加载 localhost:3000）
dotnet run --project DriveGroupManager
```

### 方式二：直接运行 WinForms（自动启动 API）

```bash
# 先确保前端已构建
cd drive-group-web
npm run build

# 运行 WinForms（自动启动内置 API）
cd ..
dotnet run --project DriveGroupManager
```

## 技术栈

| 类别 | 技术 |
|------|------|
| 宿主框架 | WinForms + WebView2 |
| 后端 API | ASP.NET Core 10.0 |
| 前端框架 | React 19 + TypeScript |
| UI 组件库 | Ant Design 5.x |
| 窗口样式 | MaterialSkin.2 |
| 构建工具 | Vite / dotnet CLI |
| 通信 | HTTP + JS 互操作 |

## 项目结构

```
DriveGroupManager/
├── DriveGroupManager/                    # WinForms 宿主应用
│   ├── DriveGroupManager.csproj
│   ├── Program.cs                        # 启动 API + WinForms
│   └── MainForm.cs                       # WebView2 容器
│
├── DriveGroupManager.Api/                # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── DrivesController.cs
│   │   └── GroupsController.cs
│   ├── Services/
│   │   └── DriveService.cs
│   ├── Models/
│   │   └── DriveGroup.cs
│   └── Program.cs
│
├── drive-group-web/                      # React 前端
│   ├── src/
│   │   ├── components/
│   │   │   ├── GroupTree.tsx
│   │   │   └── GroupEditor.tsx
│   │   ├── api/
│   │   │   └── driveApi.ts
│   │   ├── App.tsx
│   │   └── main.tsx
│   ├── package.json
│   └── vite.config.ts
│
└── README.md
```

## JS 与 C# 互操作

前端可以通过 `chrome.webview` 对象调用 WinForms 中的本地方法：

```typescript
// 检测是否在 WebView2 环境
const isWebView2 = () => !!(window as any).chrome?.webview;

// 调用本地方法打开硬盘
if (isWebView2()) {
  const nativeApp = (window as any).chrome.webview.hostObjects.nativeApp;
  nativeApp.OpenDrive('C:');
}
```

支持的本地方法：
- `OpenDrive(driveLetter: string)` - 打开指定硬盘
- `GetVersion()` - 获取应用版本

## 许可证

MIT
