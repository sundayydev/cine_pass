using BE_CinePass.Shared.DTOs.User;
using BE_CinePass.Shared.DTOs.MemberPoint;

namespace BE_CinePass.Shared.DTOs.Auth;

public class AuthMeResponseDto
{
    public UserResponseDto Profile { get; set; } = null!;
    public MemberPointResponseDto? Points { get; set; }
}
