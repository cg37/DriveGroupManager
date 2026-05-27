namespace DriveGroupManager
{
    /// <summary>
    /// 硬盘分组数据模型
    /// </summary>
    public class DriveGroup
    {
        public string GroupName { get; set; }
        public List<string> DriveLetters { get; set; }
        public string Description { get; set; }
        public int IconIndex { get; set; }

        /// <summary>
        /// 无参数构造函数（JSON序列化需要）
        /// </summary>
        public DriveGroup()
        {
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

        public override string ToString()
        {
            string drives = string.Join(", ", DriveLetters);
            return $"{GroupName} ({DriveLetters.Count}个硬盘)";
        }
    }
}