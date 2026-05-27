using System;
using System.Collections.Generic;

namespace DriveGroupManager
{
    /// <summary>
    /// 硬盘分组数据模型
    /// </summary>
    [Serializable]
    public class DriveGroup
    {
        /// <summary>
        /// 分组名称（如"活跃硬盘"）
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// 分组内的硬盘盘符列表（如["C:", "D:"]）
        /// </summary>
        public List<string> DriveLetters { get; set; }

        /// <summary>
        /// 分组描述信息
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 分组图标索引（用于显示不同图标）
        /// </summary>
        public int IconIndex { get; set; }

        /// <summary>
        /// 无参数构造函数（XML序列化必需）
        /// </summary>
        public DriveGroup()
        {
            // 初始化属性默认值
            GroupName = string.Empty;
            DriveLetters = new List<string>();
            Description = string.Empty;
            IconIndex = 0;
        }

        /// <summary>
        /// 带参数构造函数
        /// </summary>
        public DriveGroup(string groupName, List<string> driveLetters, 
                          string description = "", int iconIndex = 0)
        {
            GroupName = groupName;
            DriveLetters = driveLetters ?? new List<string>();
            Description = description;
            IconIndex = iconIndex;
        }

        /// <summary>
        /// 获取分组的显示文本（用于列表显示）
        /// </summary>
        public override string ToString()
        {
            string drives = string.Join(", ", DriveLetters);
            return $"{GroupName} ({drives})";
        }
    }
}