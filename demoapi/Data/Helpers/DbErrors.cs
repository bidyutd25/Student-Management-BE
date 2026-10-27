using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace demoapi.Data.Helpers;

public static class DbErrors
{
    private const int RuleViolationErrorNumber = 1644;
    private const int DuplicateKeyErrorNumber = 1062;

    // MySQL SIGNAL SQLSTATE '45000'.
    public static bool IsRuleViolation(DbUpdateException exception) =>
        exception.InnerException is MySqlException
        {
            Number: RuleViolationErrorNumber
        };

    // MySQL unique constraint violation.
    public static bool IsDuplicate(DbUpdateException exception) =>
        exception.InnerException is MySqlException
        {
            Number: DuplicateKeyErrorNumber
        };
}