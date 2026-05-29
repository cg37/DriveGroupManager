import { useState, useEffect } from 'react';
import {
  Modal,
  Transfer,
  Input,
  List,
  Button,
  Space,
  message,
  Card,
  Typography,
  Empty,
} from 'antd';
import {
  PlusOutlined,
  DeleteOutlined,
  SaveOutlined,
  CloseOutlined,
} from '@ant-design/icons';
import { driveApi, DriveGroup, DriveInfo } from '../api/driveApi';

const { Text } = Typography;

interface GroupEditorProps {
  visible: boolean;
  onClose: () => void;
}

export default function GroupEditor({ visible, onClose }: GroupEditorProps) {
  const [groups, setGroups] = useState<DriveGroup[]>([]);
  const [allDrives, setAllDrives] = useState<DriveInfo[]>([]);
  const [selectedGroup, setSelectedGroup] = useState<DriveGroup | null>(null);
  const [loading, setLoading] = useState(false);
  const [editName, setEditName] = useState('');
  const [editDesc, setEditDesc] = useState('');
  const [targetKeys, setTargetKeys] = useState<string[]>([]);

  useEffect(() => {
    if (visible) {
      loadData();
    }
  }, [visible]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [groupsData, drivesData] = await Promise.all([
        driveApi.getAllGroups(),
        driveApi.getAllDrives(),
      ]);
      setGroups(groupsData);
      setAllDrives(drivesData);
    } catch (error) {
      message.error('加载数据失败');
    } finally {
      setLoading(false);
    }
  };

  const handleSelectGroup = (group: DriveGroup) => {
    setSelectedGroup(group);
    setEditName(group.groupName);
    setEditDesc(group.description);
    setTargetKeys(group.driveLetters);
  };

  const handleAddGroup = () => {
    const newGroup: DriveGroup = {
      groupName: `新分组 ${groups.length + 1}`,
      driveLetters: [],
      description: '',
      iconIndex: 0,
    };
    setGroups([...groups, newGroup]);
    handleSelectGroup(newGroup);
  };

  const handleDeleteGroup = (groupToDelete: DriveGroup) => {
    const newGroups = groups.filter((g) => g.groupName !== groupToDelete.groupName);
    setGroups(newGroups);
    if (selectedGroup?.groupName === groupToDelete.groupName) {
      setSelectedGroup(null);
      setEditName('');
      setEditDesc('');
      setTargetKeys([]);
    }
  };

  const handleSave = () => {
    if (!selectedGroup) return;

    const updatedGroups = groups.map((g) =>
      g.groupName === selectedGroup.groupName
        ? {
            ...g,
            groupName: editName,
            description: editDesc,
            driveLetters: targetKeys,
          }
        : g
    );

    setGroups(updatedGroups);
    setSelectedGroup({ ...selectedGroup, groupName: editName, description: editDesc, driveLetters: targetKeys });
    message.success('分组已更新，请记得点击「保存所有」');
  };

  const handleSaveAll = async () => {
    try {
      await driveApi.updateGroups(groups);
      message.success('保存成功');
      onClose();
    } catch (error) {
      message.error('保存失败');
    }
  };

  const getAvailableDrives = () => {
    // 获取已被其他分组使用的硬盘
    const usedDrives = groups
      .filter((g) => g.groupName !== selectedGroup?.groupName)
      .flatMap((g) => g.driveLetters);

    return allDrives.filter((d) => !usedDrives.includes(d.letter));
  };

  const transferData = getAvailableDrives().map((d) => ({
    key: d.letter,
    title: `${d.letter} - ${d.label} (${d.totalSizeGB}GB)`,
    description: `可用: ${d.freeSpaceGB}GB`,
  }));

  return (
    <Modal
      title="编辑分组"
      open={visible}
      onCancel={onClose}
      width={900}
      footer={[
        <Button key="cancel" onClick={onClose} icon={<CloseOutlined />}>
          取消
        </Button>,
        <Button key="save" type="primary" onClick={handleSaveAll} icon={<SaveOutlined />}>
          保存所有
        </Button>,
      ]}
    >
      <div style={{ display: 'flex', gap: 16, height: 500 }}>
        {/* 左侧：分组列表 */}
        <Card
          title="分组列表"
          style={{ width: 250, flexShrink: 0 }}
          extra={
            <Button type="primary" size="small" icon={<PlusOutlined />} onClick={handleAddGroup}>
              新建
            </Button>
          }
        >
          <List
            dataSource={groups}
            renderItem={(item) => (
              <List.Item
                onClick={() => handleSelectGroup(item)}
                style={{
                  cursor: 'pointer',
                  background: selectedGroup?.groupName === item.groupName ? '#e6f7ff' : 'transparent',
                  padding: '8px 12px',
                  borderRadius: 4,
                }}
                actions={[
                  <Button
                    type="text"
                    danger
                    size="small"
                    icon={<DeleteOutlined />}
                    onClick={(e) => {
                      e.stopPropagation();
                      handleDeleteGroup(item);
                    }}
                  />,
                ]}
              >
                <Text strong>{item.groupName}</Text>
                <br />
                <Text type="secondary" style={{ fontSize: 12 }}>
                  {item.driveLetters.length}个硬盘
                </Text>
              </List.Item>
            )}
          />
        </Card>

        {/* 右侧：分组详情 */}
        <div style={{ flex: 1 }}>
          {selectedGroup ? (
            <Space direction="vertical" style={{ width: '100%' }} size="large">
              <Card size="small" title="分组信息">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div>
                    <Text type="secondary">名称：</Text>
                    <Input
                      value={editName}
                      onChange={(e) => setEditName(e.target.value)}
                      style={{ width: 300 }}
                    />
                  </div>
                  <div>
                    <Text type="secondary">描述：</Text>
                    <Input
                      value={editDesc}
                      onChange={(e) => setEditDesc(e.target.value)}
                      style={{ width: 300 }}
                    />
                  </div>
                  <Button type="primary" onClick={handleSave}>
                    更新分组
                  </Button>
                </Space>
              </Card>

              <Card size="small" title="分配硬盘">
                <Transfer
                  dataSource={transferData}
                  titles={['可用硬盘', '已分配']}
                  targetKeys={targetKeys}
                  onChange={setTargetKeys}
                  render={(item) => item.title}
                  listStyle={{
                    width: 250,
                    height: 250,
                  }}
                />
              </Card>
            </Space>
          ) : (
            <Empty description="请选择或创建一个分组" style={{ marginTop: 100 }} />
          )}
        </div>
      </div>
    </Modal>
  );
}
