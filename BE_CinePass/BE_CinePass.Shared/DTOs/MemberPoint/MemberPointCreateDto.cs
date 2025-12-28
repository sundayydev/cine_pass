using System.ComponentModel.DataAnnotations;

namespace BE_CinePass.Shared.DTOs.MemberPoint;

public class MemberPointCreateDto
{
    [Required(ErrorMessage = "User ID là bắt buộc")]
    public Guid UserId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Điểm khởi đầu phải lớn hơn hoặc bằng 0")]
    public int InitialPoints { get; set; } = 0;
}
