# EmployeeMangement_Zelis

An employee management application built with .NET 10, SQLite, and Blazor.

## Getting Started

1. Clone the repo and run the project
2. On first run, the app seeds 5 default employees, each with valid employee and address information
3. From there you can add, edit, and delete employees
4. Swagger documentation is available through a link in the top left corner of the Employee Manager UI

`Program.cs` calls `db.Database.Migrate()` to automatically apply any pending migrations. It will throw an error if the model has changes that don't have a corresponding migration.

This also makes it easy to reset: after testing edits or deletes, delete the local `employee` database and rerun the app to rebuild it fresh.

## Running Tests

In Visual Studio, select **Test > Run All Tests**.

## Technical Decisions

### DTOs for Address, EmployeeRequest, and EmployeeResponse

I've used Razor Pages in the past, where page models bind entities directly. Since this application exposes a REST API consumed by the UI, Swagger, and unit tests, I added separate API contracts derived from the database models:

- Clients can only send fields defined on the request DTO. They cannot set `Id`, `EmployeeId`, or other internal values.
- Input rules, such as 2-letter state codes and phone format, are annotated on the DTOs. `EmployeeController.cs` returns a `400 Bad Request` if validation fails.
- `Employee` references `Address`, which references `Employee` again, creating a cycle that can't be serialized by default. The DTO flattens this into a single nested object that serializes cleanly.
- Since there are only two entities to map, and I wanted experience writing mappings by hand, I created `EmployeeMappings.cs` instead of using AutoMapper.

### Age Field

The lower portion of the assessment mentions an `Age` field, but it isn't included in the model definition on the first page. I chose to leave it out and am noting it here.

### NavigationManager for HttpClient Registration

`NavigationManager` resolves the base URI from the URL the browser used to reach the app, so there's no hard-coded localhost port. This ensures the app works for any reviewer who clones and runs the project.

This approach would likely fail in production where the browser URL differs from what the server expects (e.g., in a container or behind a load balancer) without the proper network setup.

## Validation and Integration Testing

I created an xUnit test project through the Visual Studio UI and added the `Microsoft.AspNetCore.Mvc.Testing` package. I chose xUnit because it's the testing framework I have the most experience with.

Tests are run with **Test > Run All Tests**.

## Packages Used

- Microsoft.AspNetCore.OpenApi
- Swashbuckle.AspNetCore.SwaggerUI
- Microsoft.EntityFrameworkCore.Sqlite
- Microsoft.EntityFrameworkCore.Tools
- Microsoft.EntityFrameworkCore.Design
- Microsoft.AspNetCore.Mvc.Testing
