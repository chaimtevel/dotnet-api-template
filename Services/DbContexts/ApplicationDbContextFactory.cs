using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;

namespace Services.DbContexts
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // as per https://github.com/aspnet/EntityFrameworkCore/issues/8332
            // args are currently not being passed in so using environment varibale instead
            // powershel: $env:DefaultConnection = ""
            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseSqlServer(Environment.GetEnvironmentVariable("DefaultConnection"));

            return new ApplicationDbContext(builder.Options);
        }
    }
}
