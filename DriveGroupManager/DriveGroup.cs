using System;
using System.Collections.Generic;

namespace DriveGroupManager

{
  [Serializable]

  public class DriveGroupManager
  {
    public string GroupName {get; set;}
    public List<string> DriveLetters{get; set}

    public string Description{get; set;}

    public int IconIndex{get; set;}

    public DriveGroupManager (string groupName, List<string>driveLetters, string description = "", int iconIndex = 0)
    {
      GroupName = groupName;
      DriveLetters = driveLetters ?? new List<string>();
      Description = description;
      IconIndex = iconIndex;
    }

    public override string ToString()
    {
      string drives = string.Join(",", DriveLetters);
      return $"{GroupName}({drives})";
    }
  }
}
