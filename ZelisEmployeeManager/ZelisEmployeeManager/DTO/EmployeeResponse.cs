namespace ZelisEmployeeManager.DTO
{
    public class EmployeeResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public int Age { get; set; }
        public AddressDto Address { get; set; } = new();
    }
}
