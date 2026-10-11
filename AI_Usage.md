AI Usage
  - My main overall usage of AI in this project was for designing the front end, notably the Employee.razor and Employee.razor.css pages. I fed Claude the color palette of Zelis' main home page,
    and used a standard table + form layout to quickly and cleanly create a page that fits into the Zelis theme, as well as adding some extra utility such as deleting employees. This wasn't a concern in the project   details I was given, but I think it adds a nice touch and Claude handles front-end UI work extremely well in my experience.

  - I originally had a "NoteTaking.txt" file where I wrote down any AI intregations or questions I posed to the AI, and these questions are from that. I did not commit this file to the repo.
      
      Q1. 
      	My understand of Entity Framework is decent, and I know we need the database context, where we define our database models and then
      	use a OnModelCreate function to apply our constraints to those models, and that we use migrations to update those models and add new ones.
      	What are some of the best practices here to keep the models aligned and obey the rules of foreign and primary keys?
      
      A1.
      	- Declare an explicit foreign key, or EF creates a "shadow" foreign key
      	- Ensure nullabilty is enabled, so that nullable properties retain, and the compiler warns you if your C# models do not match your db schema 
      	- Primary key is inferred by any column named "Id" or {className}Id


I prompted Claude to give me a clean and generic GetAll() method in the EmployeeController to see if we had a better way of writing a simple EF query. Claude generated this simple GetAll function:

    await db.Employees.AsNoTracking().Include(e => e.Address)
            .Select(e => e.ToResponse()).ToListAsync();

On initial testing, this endpoint threw a Null Reference Exception inside of `ToResponse()` in
EmployeeMappings.cs. Claude identified the likely culprit, and I verified it
against the EF Core eager-loading docs and the generated SQL log: because the query ends in a `Select`
that projects to `EmployeeResponse` instead of returning `Employee` entities, EF Core ignores
the `Include`. The generated SQL had no JOIN to Addresses, so every `Address` was null.

The fix was to load the entities first, then map them in memory:

    var employees = await db.Employees.AsNoTracking()
                            .Include(e => e.Address)
                            .ToListAsync();
    return employees.Select(e => e.ToResponse()).ToList();

`ToListAsync()` now returns `Employee` entities, so the `Include` is applied and the JOIN on Address runs successfully

