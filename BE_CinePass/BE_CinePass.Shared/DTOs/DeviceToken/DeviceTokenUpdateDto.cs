using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.DeviceToken;

public class DeviceTokenUpdateDto
{
    [MaxLength(100)]
    public string? DeviceModel { get; set; }

    [MaxLength(20)]
    public string? AppVersion { get; set; }

    public bool? IsActive { get; set; }
}