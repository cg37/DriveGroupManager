import { Card, Progress, Row, Col, Badge, Typography } from 'antd';
import { FolderOutlined, HddOutlined, DatabaseOutlined } from '@ant-design/icons';
import type { GroupViewModel, DriveInfo } from '../api/driveApi';

const { Text } = Typography;

interface GroupTreeProps {
  groups: GroupViewModel[];
  ungroupedDrives: DriveInfo[];
  allDrives: DriveInfo[];
  onSelectDrive: (driveLetter: string) => void;
  onOpenDrive: (driveLetter: string) => void;
}

const getStatusColor = (status: string) => {
  switch (status) {
    case 'danger':
      return '#ff4d4f';
    case 'warning':
      return '#faad14';
    default:
      return '#52c41a';
  }
};

const DriveCard = ({ drive, onClick, selected }: { drive: DriveInfo; onClick: () => void; selected?: boolean }) => (
  <Card
    className={`drive-card ${selected ? 'selected' : ''}`}
    onClick={onClick}
    size="small"
  >
    <div className="drive-header">
      <span>
        <HddOutlined style={{ marginRight: 8 }} />
        <span className="drive-letter">{drive.letter}</span>
        <Text className="drive-label" style={{ marginLeft: 8 }}>
          {drive.label}
        </Text>
      </span>
      <Badge
        color={getStatusColor(drive.status)}
        text={drive.status === 'danger' ? '空间不足' : drive.status === 'warning' ? '空间紧张' : '空间充足'}
      />
    </div>
    <Progress
      percent={Math.round(100 - drive.freePercent)}
      status={drive.status === 'danger' ? 'exception' : drive.status === 'warning' ? 'active' : 'success'}
      format={() => `${drive.freeSpaceGB}GB / ${drive.totalSizeGB}GB`}
    />
    <div className="space-info">
      <span>可用: {drive.freeSpaceGB}GB</span>
      <span>已用: {drive.totalSizeGB - drive.freeSpaceGB}GB</span>
    </div>
  </Card>
);

export default function GroupTree({
  groups,
  ungroupedDrives,
  allDrives,
  onSelectDrive,
  onOpenDrive,
}: GroupTreeProps) {
  const [selectedLetter, setSelectedLetter] = useState<string | null>(null);

  const handleDriveClick = (drive: DriveInfo) => {
    setSelectedLetter(drive.letter);
    onSelectDrive(drive.letter);
  };

  return (
    <div className="group-tree">
      {/* 分组列表 */}
      {groups.map((group) => (
        <div key={group.groupName} className="group-section">
          <div className="group-title">
            <FolderOutlined className="group-icon" />
            <Text strong style={{ fontSize: 16 }}>
              {group.groupName}
            </Text>
            <Text type="secondary">({group.driveCount}个硬盘)</Text>
            {group.description && (
              <Text type="secondary" style={{ marginLeft: 8 }}>
                - {group.description}
              </Text>
            )}
          </div>
          <Row gutter={[16, 16]}>
            {group.drives.map((drive) => (
              <Col span={8} key={drive.letter}>
                <DriveCard
                  drive={drive}
                  onClick={() => handleDriveClick(drive)}
                  selected={selectedLetter === drive.letter}
                />
              </Col>
            ))}
          </Row>
        </div>
      ))}

      {/* 未分组硬盘 */}
      {ungroupedDrives.length > 0 && (
        <div className="group-section ungrouped-section">
          <div className="group-title">
            <FolderOutlined className="group-icon" style={{ color: '#8c8c8c' }} />
            <Text strong style={{ fontSize: 16, color: '#8c8c8c' }}>
              未分组硬盘
            </Text>
            <Text type="secondary">({ungroupedDrives.length}个)</Text>
          </div>
          <Row gutter={[16, 16]}>
            {ungroupedDrives.map((drive) => (
              <Col span={8} key={drive.letter}>
                <DriveCard
                  drive={drive}
                  onClick={() => handleDriveClick(drive)}
                  selected={selectedLetter === drive.letter}
                />
              </Col>
            ))}
          </Row>
        </div>
      )}

      {/* 所有硬盘 */}
      <div className="all-drives-section">
        <div className="group-title">
          <DatabaseOutlined className="group-icon" style={{ color: '#1890ff' }} />
          <Text strong style={{ fontSize: 16 }}>
            所有硬盘
          </Text>
          <Text type="secondary">({allDrives.length}个)</Text>
        </div>
        <Row gutter={[16, 16]}>
          {allDrives.map((drive) => (
            <Col span={8} key={drive.letter}>
              <DriveCard
                drive={drive}
                onClick={() => handleDriveClick(drive)}
                selected={selectedLetter === drive.letter}
              />
            </Col>
          ))}
        </Row>
      </div>
    </div>
  );
}

import { useState } from 'react';
