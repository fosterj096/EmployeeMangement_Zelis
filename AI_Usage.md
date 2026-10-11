# AI Usage

## Tooling

My go-to AI tool is Anthropic's Claude, so that's what I used as my assistant for this project. I've tried others, such as ChatGPT, Copilot, and Gemini, but I've found Claude gives the most concise and accurate answers for software development.

I set Claude's instructions so that it isn't self-serving or a "yes man", tries the simplest solution first where possible, and cites the documentation it used for each response. This improves accuracy and gives me source material to check whether Claude made errors or incorrect assumptions, whether from lack of context or hallucination.

## Front End

My main use of AI in this project was for the front end and the testing suite, notably `Employee.razor` and `Employee.razor.css`. I gave Claude the color palette from Zelis' home page and used a standard table and form layout to quickly build a page that fits the Zelis theme.

I also added some extra utility, such as deleting employees. This wasn't required in the project details, but it adds a nice touch. Claude handles front-end UI work well in my experience, and it demonstrates my ability to deliver a customer-facing UI under time constraints.

## Questions I Asked

I kept a `NoteTaking.txt` file to record AI interactions and questions I asked. I did not commit this file to the repo, but the questions below come from it.

**Q1.** My understanding of Entity Framework is decent. I know we need a database context where we define our models, use `OnModelCreating` to apply constraints, and use migrations to update existing models and add new ones. What are some best practices for keeping the models aligned and obeying primary and foreign key rules?

**A1.**
- Declare an explicit foreign key property, or EF will create a "shadow" foreign key.
- Enable nullable reference types. EF uses them to infer which columns are required, so your C# models and the database schema stay consistent.
- The primary key is inferred by convention from any property named `Id` or `{ClassName}Id`.

## EF Core Include Bug

I asked Claude for a clean, generic `GetAll()` method in `EmployeeController` to see if there was a better way to write a simple EF query. It generated:

```csharp
await db.Employees.AsNoTracking()
    .Include(e => e.Address)
    .Select(e => e.ToResponse())
    .ToListAsync();
```

On initial testing, this endpoint threw a null reference exception inside `ToResponse()` in `EmployeeMappings.cs`. Claude identified the likely cause, and I verified it against the EF Core documentation and the generated SQL log: because the query ends in a `Select` that projects to `EmployeeResponse` instead of returning `Employee` entities, EF Core ignores the `Include`. The generated SQL had no JOIN to `Addresses`, so every `Address` was null.

The fix was to load the entities first, then map them in memory:

```csharp
var employees = await db.Employees.AsNoTracking()
    .Include(e => e.Address)
    .ToListAsync();

return employees.Select(e => e.ToResponse()).ToList();
```

`ToListAsync()` now returns `Employee` entities, so the `Include` is applied and the JOIN on `Addresses` runs as expected.

## Testing

I also used Claude to help set up the validation and integration testing suite. Unit testing is one of my weaker areas and something I'm working to improve. Claude helped create the API test factory and `EmployeeApiTest.cs`.

I'm more comfortable with validation tests. Using `[Fact]` and `[Theory]` to validate objects and required properties is familiar to me, but I haven't done much API integration testing.

A few other points AI helped with during testing:

- `Validator.TryValidateObject()` does not recurse into nested objects, so the `Address` on an `Employee` must be validated separately.
- Each test run creates a uniquely named `test_*.db` file so runs don't share state.
