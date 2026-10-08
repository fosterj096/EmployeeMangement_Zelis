using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ZelisEmployeeManager.Data;
using ZelisEmployeeManager.DTO;
using ZelisEmployeeManager.Models;

/*  Notes

    Adding relevant HTTP Status codes to each route

    Typically I am used to Razor pages, where the page model entity is used 
    For here, using Blazor pages, I have created these DTO models that correspond to those models, found in /DTO folder

*/


namespace ZelisEmployeeManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController(ApplicationDbContext db) : ControllerBase
    {

        // Get all employees
        [HttpGet]
        public async Task<ActionResult<List<EmployeeResponse>>> GetAll()
        {
            var employees = await db.Employees.AsNoTracking()
                                    .Include(e => e.Address)
                                    .ToListAsync();

            return employees.Select(e => e.ToResponse()).ToList();
        }

        // Get employee by Id
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponse>> GetById(int id)
        {
            var e = await db.Employees.AsNoTracking().Include(x => x.Address)
                            .FirstOrDefaultAsync(x => x.Id == id);
            return e is null ? NotFound() : e.ToResponse();
        }

        // Create Employee
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeResponse>> Create(EmployeeRequest req)
        {
            if (await db.Employees.AnyAsync(e => e.Email == req.Email))
                return EmailConflict();

            var emp = new Employee { Address = new Address() };
            req.ApplyTo(emp);
            db.Employees.Add(emp);
            if (!await TrySave()) return EmailConflict();

            return CreatedAtAction(nameof(GetById), new { id = emp.Id }, emp.ToResponse());
        }

        // Update Employee
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, EmployeeRequest req)
        {
            var emp = await db.Employees.Include(e => e.Address).FirstOrDefaultAsync(e => e.Id == id);
            if (emp is null) return NotFound();

            if (await db.Employees.AnyAsync(e => e.Id != id && e.Email == req.Email))
                return EmailConflict();

            req.ApplyTo(emp);
            return await TrySave() ? NoContent() : EmailConflict();
        }

        // Delete Employee
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var emp = await db.Employees.Include(e => e.Address).FirstOrDefaultAsync(e => e.Id == id);
            if (emp is null) return NotFound();

            db.Employees.Remove(emp);          // cascade removes the Address

            await db.SaveChangesAsync();
            return NoContent();
        }

        // Avoid email duplication
        private ObjectResult EmailConflict() =>
            Problem(statusCode: 409, title: "Email already exists.");


        // idempotency for if any requests pass multiple "AnyAsync" checks at the same time, the unique index rejects the second reqest
        private async Task<bool> TrySave()
        {
            try { await db.SaveChangesAsync(); return true; }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
            { return false; }   // 19 = SQLITE_CONSTRAINT
        }
    }
}
