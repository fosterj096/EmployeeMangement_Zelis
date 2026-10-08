using System.ComponentModel.DataAnnotations;

namespace ZelisEmployeeManager.DTO
{
    public class AddressDto
    {
        [Required] public string Address1 { get; set; } = "";
        public string? Address2 { get; set; }
        [Required] public string City { get; set; } = "";
        [Required, RegularExpression("^[A-Z]{2}$", ErrorMessage = "State must be a 2-letter code, e.g. MO.")]
        public string State { get; set; } = "";
        [Required, RegularExpression(@"^\d{5}$", ErrorMessage = "Zip must be 5 digits.")]
        public string Zip { get; set; } = "";
    }
}
