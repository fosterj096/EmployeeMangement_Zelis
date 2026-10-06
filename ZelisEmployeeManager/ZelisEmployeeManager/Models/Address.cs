namespace ZelisEmployeeManager.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string PrimaryAddress { get; set; } = string.Empty;
        public string? SecondaryAddress { get; set; } // leave as nullable type as this is an optional parameter
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public int EmployeeId { get; set; } // fk for employee
        public Employee Employee { get; set; } = null!;
    }
}
