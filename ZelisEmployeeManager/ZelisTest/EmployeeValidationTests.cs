using System.ComponentModel.DataAnnotations;
using ZelisEmployeeManager.DTO;
using ZelisEmployeeManager.Models;

namespace ZelisTest.Unit;

public class EmployeeValidationTests
{

    private static List<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
        return results;
    }

    private static bool HasErrorFor(List<ValidationResult> results, string member) =>
        results.Any(r => r.MemberNames.Contains(member));

    private static EmployeeRequest ValidEmployee() => new()
    {
        FirstName = "Test",
        LastName = "User",
        Email = "test@example.com",
        Phone = "(555)-123-4567",
        Address = ValidAddress()
    };

    private static AddressDto ValidAddress() => new()
    {
        Address1 = "123 Main St",
        City = "St. Louis",
        State = "MO",
        Zip = "63123"
    };

    [Fact]
    public void ValidEmployeeAndAddress()
    {
        Assert.Empty(Validate(ValidEmployee()));
        Assert.Empty(Validate(ValidAddress()));
    }

    [Theory]
    [InlineData("555-123-4567")]
    [InlineData("(555)123-4567")]
    [InlineData("(555)-1234-567")]
    [InlineData("(55)-123-4567")]
    [InlineData("(555)-123-4567 ")]
    [InlineData("(abc)-123-4567")]
    [InlineData("")]
    public void InvalidPhoneFormat(string phone)
    {
        var employee = ValidEmployee();
        employee.Phone = phone;

        Assert.True(HasErrorFor(Validate(employee), nameof(EmployeeRequest.Phone)));
    }

    [Theory]
    [InlineData("1111")]
    [InlineData("123456")]
    [InlineData("abcde")]
    [InlineData("63123-1234")]
    public void InvalidZipCode(string zip)
    {
        var address = ValidAddress();
        address.Zip = zip;

        Assert.True(HasErrorFor(Validate(address), nameof(AddressDto.Zip)));
    }

    [Theory]
    [InlineData("M")]
    [InlineData("MOO")]
    [InlineData("12")]
    public void InvalidState(string state)
    {
        var address = ValidAddress();
        address.State = state;

        Assert.True(HasErrorFor(Validate(address), nameof(AddressDto.State)));
    }

    [Fact]
    public void SecondaryAddress_IsOptionalCheck()
    {
        var address = ValidAddress();
        address.Address2 = null;

        Assert.Empty(Validate(address));
    }
}