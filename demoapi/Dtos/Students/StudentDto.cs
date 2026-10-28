namespace demoapi.Dtos.Students;
public sealed record StudentDto(
    int Id,
    string Name,
    string Email,
    DateOnly? DateOfBirth,
    int ActiveCourses)
{
    public int? Age
    {
        get
        {
            if (DateOfBirth is not { } birthDate)
            {
                return null;
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - birthDate.Year;

            if (birthDate > today.AddYears(-age))
            {
                age--;
            }

            return age;
        }
    }
        public decimal? OverallPercentage { get; init; }
}