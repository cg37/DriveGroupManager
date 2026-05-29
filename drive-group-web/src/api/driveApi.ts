import axios from 'axios';

const API_BASE_URL = 'http://localhost:5000/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export interface DriveInfo {
  letter: string;
  label: string;
  totalSizeGB: number;
  freeSpaceGB: number;
  freePercent: number;
  status: 'normal' | 'warning' | 'danger';
}

export interface DriveGroup {
  groupName: string;
  driveLetters: string[];
  description: string;
  iconIndex: number;
}

export interface GroupViewModel {
  groupName: string;
  description: string;
  driveCount: number;
  drives: DriveInfo[];
}

export const driveApi = {
  // 获取所有硬盘
  getAllDrives: () => api.get<DriveInfo[]>('/drives').then(res => res.data),

  // 获取可用硬盘
  getAvailableDrives: () => api.get<DriveInfo[]>('/drives/available').then(res => res.data),

  // 打开硬盘
  openDrive: (driveLetter: string) => api.post('/drives/open', { driveLetter }),

  // 获取所有分组
  getAllGroups: () => api.get<DriveGroup[]>('/groups').then(res => res.data),

  // 获取分组视图
  getGroupView: () => api.get<GroupViewModel[]>('/groups/view').then(res => res.data),

  // 更新分组
  updateGroups: (groups: DriveGroup[]) => api.put('/groups', groups),
};
