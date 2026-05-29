/// <reference types="vite/client" />

interface Window {
  // 暴露给 WinForms 调用的 API
  openDrive?: (driveLetter: string) => void;
}
