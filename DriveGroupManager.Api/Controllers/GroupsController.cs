using DriveGroupManager.Api.Models;
using DriveGroupManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DriveGroupManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IDriveService _driveService;

    public GroupsController(IDriveService driveService)
    {
        _driveService = driveService;
    }

    /// <summary>
    /// 获取所有分组
    /// </summary>
    [HttpGet]
    public ActionResult<List<DriveGroup>> GetAllGroups()
    {
        return Ok(_driveService.GetAllGroups());
    }

    /// <summary>
    /// 更新所有分组
    /// </summary>
    [HttpPut]
    public IActionResult UpdateGroups([FromBody] List<DriveGroup> groups)
    {
        _driveService.UpdateGroups(groups);
        return Ok();
    }

    /// <summary>
    /// 获取分组视图（包含硬盘详情）
    /// </summary>
    [HttpGet("view")]
    public ActionResult<List<GroupViewModel>> GetGroupView()
    {
        var groups = _driveService.GetAllGroups();
        var allDrives = _driveService.GetAllDrives();

        var result = groups.Select(g => new GroupViewModel
        {
            GroupName = g.GroupName,
            Description = g.Description,
            DriveCount = g.DriveLetters.Count,
            Drives = allDrives.Where(d => g.DriveLetters.Contains(d.Letter)).ToList()
        }).ToList();

        return Ok(result);
    }
}
