using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;

namespace ZelisTest.Integration;

/// <summary>
/// Boots the real app in memory against a throwaway SQLite file.
/// Your normal startup code creates the schema and seeds it, exactly like production.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private const string ConnectionStringName = "ZelisDBConnString";

    private readonly string _dbPath =
        Path.Combine(Path.GetTempPath(), $"zelis-test-{Guid.NewGuid():N}.db");

    private bool _cleanedUp;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting($"ConnectionStrings:{ConnectionStringName}", $"Data Source={_dbPath}");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) Cleanup();
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        Cleanup();
    }

    private void Cleanup()
    {
        if (_cleanedUp) return;
        _cleanedUp = true;

        SqliteConnection.ClearAllPools(); // release the file lock
        try { File.Delete(_dbPath); } catch (IOException) { /* temp file; not worth failing a run over */ }
    }
}