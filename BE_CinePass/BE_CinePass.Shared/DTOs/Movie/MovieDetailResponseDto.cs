namespace BE_CinePass.Shared.DTOs.Movie;

public class MovieDetailResponseDto : MovieResponseDto
{
    public List<MovieActorDto> Actors { get; set; } = new();
    public List<MovieReviewDto> Reviews { get; set; } = new();
}
