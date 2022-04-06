using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using static Shared.Extensions.StringExtensions;

namespace Services.DbContexts;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // see https://github.com/aspnet/EntityFrameworkCore/issues/8332 on how to pass args
        // it would be something like dotnet ef migrations add MigrationName -- "connection string"
        // if we use env vars then it would be the following for powershell
        // powershel: $env:DefaultConnection = ""
        var builder = new DbContextOptionsBuilder<ApplicationDbContext>();

        var envVarName = "DefaultConnection";
        var connectionString = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable(envVarName);

        if (connectionString.IsEmpty())
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"No connection string was supplied. Either pass it on the command line by specifying '-- <connection string>' or set the {envVarName} env var");
            Console.ResetColor();
            ArgumentNullException.ThrowIfNull(connectionString, nameof(connectionString));
        }

        builder.UseSqlServer(connectionString!);

        return new ApplicationDbContext(builder.Options);
    }
}