using System.ComponentModel.DataAnnotations;

namespace ZelisEmployeeManager.DTO
{
    public class EmployeeRequest   // used for both POST and PUT routes
    {
        [Required] public string FirstName { get; set; } = "";
        [Required] public string LastName { get; set; } = "";
        [Required, EmailAddress] public string Email { get; set; } = "";

        [Required, RegularExpression(@"^\(\d{3}\)-\d{3}-\d{4}$", ErrorMessage = "Phone must be in format of (XXX)-XXX-XXXX")]
        public string Phone { get; set; } = "";
        [Required] public AddressDto Address { get; set; } = new();
    }
}
