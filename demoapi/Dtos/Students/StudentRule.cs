namespace demoapi.Dtos.Students;
public static class StudentRules
{
    public const string NamePattern = @"^(?=(?:.*?\p{L}){3})\p{L}+(?: \p{L}+)*$";
    public const int NameMinLength = 3;
    public const int NameMaxLength = 100;
    public const int MinimumAge = 16;   // age must be greater than 15
}