# EmployeeMangement_Zelis
An employee management application created using .NET 10.




Technical Decisions

  - Adding the DTO for Address, EmployeeRequest, and EmployeeResponse
      I have used Razor Pages in the past, where the Page Models bind entities directly. Since this application exposes a REST API
      to be consumed by the Razor UI, Swagger, and unit tests -  I added these separate API contracts derived from the respective database models:
  
       - Clients can only send fields defined on the request DTO. They cannot set "Id", "EmployeeId", or any other internal values we would need
       - Input rules such as 2 letter state names and phone format are annotated in these DTOs, the EmployeeController.cs API will return a BadRequest (400) if these fail.
       - The Employee object that references Address, then Employee again creates a reference that can't normally be serialized. The DTO flattens this instead a single nested
          object that can be serialized properly.
  
       - Because we only have two entities to map and I wanted some experience with writing these mappings, I created the EmployeeMappings.cs to work with these DTOs
          opting out of installing the AutoMapper package
      
  - For the Model definitions, I noticed that an "Age" parameter was included in the lower portion of this assessment, but not included in the actual Model Definition on the first page. I am choosing to leave it out
      but making this note for it.

  - Using NavigationManager for HttpClient registration
      This allows the app to resolve the BaseUri from the URL the browser used to reach the app, eliminating the need for a hard-coded localhost port
      Mainly using this approach to ensure the application will work for any reviewer who clones and run the project
      This approach would likely fail in a production environment where the browser URL differs from what the server expects, such as in a containerized application or behind a load balancer,
        without the proper network set up

  - Packages Used:
      Microsoft.AspNetCore.OpenApi
      Swashbuckle.AspNetCore.SwaggerUI
      Microsoft.EntityFrameworkCore.Sqlite
      Microsoft.EntityFrameworkCore.Tools
      Microsoft.EntityFrameworkCore.Design


AI Usage
  - 
