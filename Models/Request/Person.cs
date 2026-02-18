using System.ComponentModel.DataAnnotations;

namespace Transaction_Sql_Crud_Operation.Models;

public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public int Age { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

public class Qualification
{
    public int QualificationId { get; set; }
    public int PersonId { get; set; }
    public string QualificationName { get; set; } = string.Empty;
    public decimal Marks { get; set; }
}

public record PersonRequest(
    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    string Name,

    [Required(ErrorMessage = "Mobile number is required")]
    [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits")]
    string MobileNo,

    [Required(ErrorMessage = "Age is required")]
    [Range(1, 100, ErrorMessage = "Age must be between 1 and 100")]
    int Age,

    [StringLength(40, ErrorMessage = "Address cannot exceed 40 characters")]
    string? Address,

    List<QualificationRequest>? Qualifications);

public record QualificationRequest(
    [Required(ErrorMessage = "Qualification name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Qualification name must be between 2 and 100 characters")]
    string QualificationName,

    [Required(ErrorMessage = "Marks is required")]
    [Range(0, 100, ErrorMessage = "Marks must be between 0 and 100")]
    decimal Marks);