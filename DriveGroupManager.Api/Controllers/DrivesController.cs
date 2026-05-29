using DriveGroupManager.Api.Models;
using DriveGroupManager.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DriveGroupManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DrivesController : ControllerBase
{
    private readonly IDriveService _driveService;

    public DrivesController(IDriveService driveService)
    {
        _driveService = driveService;
    }

    /// <summary>
    /// 获取所有硬盘信息
    /// </summary>
    [HttpGet]
    public ActionResult<List<DriveInfo>> GetAllDrives()
    {
        return Ok(_driveService.GetAllDrives());
    }

    /// <summary>
    /// 获取未分组的可用硬盘
    /// </summary>
    [HttpGet("available")]
    public ActionResult<List<DriveInfo>> GetAvailableDrives()
    {
        return Ok(_driveService.GetAvailableDrives());
    }

    /// <summary>
    /// 打开指定硬盘
    /// </summary>
    [HttpPost("open")]
    public IActionResult OpenDrive([FromBody] OpenDriveRequest request)
    {
        _driveService.OpenDrive(request.DriveLetter);
        return Ok();
    }
}

public class OpenDriveRequest
{
    public string DriveLetter { get; set; } = string.Empty;
}
