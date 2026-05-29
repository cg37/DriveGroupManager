import { useState, useEffect } from 'react';
import { Layout, Typography, Button, Space, message } from 'antd';
import { ReloadOutlined, FolderOpenOutlined, SettingOutlined } from '@ant-design/icons';
import { driveApi, GroupViewModel, DriveInfo } from './api/driveApi';
import GroupTree from './components/GroupTree';
import GroupEditor from './components/GroupEditor';
import './App.css';

const { Header, Content } = Layout;
const { Title } = Typography;

// 检测是否在 WebView2 环境中
const isWebView2 = () => {
  return !!(window as any).chrome?.webview;
};

// 调用 WinForms 本地方法
const callNative = (method: string, ...args: any[]) => {
  if (isWebView2()) {
    const nativeApp = (window as any).chrome.webview.hostObjects.nativeApp;
    if (nativeApp && typeof nativeApp[method] === 'function') {
      nativeApp[method](...args);
    }
  }
};

function App() {
  const [groups, setGroups] = useState<GroupViewModel[]>([]);
  const [allDrives, setAllDrives] = useState<DriveInfo[]>([]);
  const [ungroupedDrives, setUngroupedDrives] = useState<DriveInfo[]>([]);
  const [loading, setLoading] = useState(false);
  const [editorVisible, setEditorVisible] = useState(false);
  const [selectedDrive, setSelectedDrive] = useState<string | null>(null);

  const loadData = async () => {
    setLoading(true);
    try {
      const [groupData, driveData] = await Promise.all([
        driveApi.getGroupView(),
        driveApi.getAllDrives(),
      ]);
      setGroups(groupData);
      setAllDrives(driveData);

      // 计算未分组的硬盘
      const groupedLetters = groupData.flatMap(g => g.drives.map(d => d.letter));
      setUngroupedDrives(driveData.filter(d => !groupedLetters.includes(d.letter)));
    } catch (error) {
      message.error('加载数据失败');
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleOpenDrive = async (driveLetter: string) => {
    // 优先使用本地方法打开（更直接）
    if (isWebView2()) {
      callNative('OpenDrive', driveLetter);
    } else {
      // 开发模式使用 API
      try {
        await driveApi.openDrive(driveLetter);
      } catch (error) {
        message.error('打开硬盘失败');
      }
    }
  };

  const handleRefresh = () => {
    loadData();
    message.success('已刷新');
  };

  return (
    <Layout className="app-layout">
      <Header className="app-header">
        <Title level={4} style={{ color: 'white', margin: 0 }}>
          硬盘分组管理器
        </Title>
        <Space>
          <Button
            icon={<ReloadOutlined />}
            onClick={handleRefresh}
            loading={loading}
          >
            刷新
          </Button>
          <Button
            type="primary"
            icon={<SettingOutlined />}
            onClick={() => setEditorVisible(true)}
          >
            编辑分组
          </Button>
        </Space>
      </Header>

      <Content className="app-content">
        <GroupTree
          groups={groups}
          ungroupedDrives={ungroupedDrives}
          allDrives={allDrives}
          onSelectDrive={setSelectedDrive}
          onOpenDrive={handleOpenDrive}
        />

        {selectedDrive && (
          <div className="action-bar">
            <Button
              type="primary"
              icon={<FolderOpenOutlined />}
              onClick={() => handleOpenDrive(selectedDrive)}
              size="large"
            >
              打开 {selectedDrive}
            </Button>
          </div>
        )}
      </Content>

      <GroupEditor
        visible={editorVisible}
        onClose={() => {
          setEditorVisible(false);
          loadData();
        }}
      />
    </Layout>
  );
}

export default App;
