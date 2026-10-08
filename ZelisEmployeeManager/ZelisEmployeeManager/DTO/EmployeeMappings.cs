using ZelisEmployeeManager.Models;

namespace ZelisEmployeeManager.DTO
{
    public static class EmployeeMappings
    {
        public static EmployeeResponse ToResponse(this Employee e) => new()
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email,
            Phone = e.Phone,
            Address = new AddressDto
            {
                Address1 = e.Address.PrimaryAddress,
                Address2 = e.Address.SecondaryAddress,
                City = e.Address.City,
                State = e.Address.State,
                Zip = e.Address.ZipCode
            }
        };

        public static void ApplyTo(this EmployeeRequest r, Employee e)
        {
            e.FirstName = r.FirstName.Trim();
            e.LastName = r.LastName.Trim();
            e.Email = r.Email.Trim();
            e.Phone = r.Phone.Trim();

            e.Address ??= new Address();
            e.Address.PrimaryAddress = r.Address.Address1.Trim();
            e.Address.SecondaryAddress = string.IsNullOrWhiteSpace(r.Address.Address2) ? null : r.Address.Address2.Trim();
            e.Address.City = r.Address.City.Trim();
            e.Address.State = r.Address.State.Trim().ToUpperInvariant();
            e.Address.ZipCode = r.Address.Zip.Trim();
        }
    }
}
