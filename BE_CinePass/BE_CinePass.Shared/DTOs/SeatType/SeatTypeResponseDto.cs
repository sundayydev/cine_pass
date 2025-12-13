namespace BE_CinePass.Shared.DTOs.SeatType;

public class SeatTypeResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string? Name { get; set; }
    public decimal SurchargeRate { get; set; }
}

