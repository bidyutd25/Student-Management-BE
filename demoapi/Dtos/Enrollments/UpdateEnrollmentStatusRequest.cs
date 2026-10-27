namespace demoapi.Dtos.Enrollments;
using demoapi.Models;
using System.ComponentModel.DataAnnotations;


public sealed class UpdateEnrollmentStatusRequest
{
    [EnumDataType(typeof(EnrollmentStatus))]
    public EnrollmentStatus Status { get; set; }
}