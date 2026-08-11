using DecisionHelper.Configuration;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DecisionHelper.Data;

public class DecisionHelperDbContextFactory
    : IDesignTimeDbContextFactory<DecisionHelperDbContext>
{
    public DecisionHelperDbContext CreateDbContext(string[] args)
    {
        Env.NoClobber().Load();

        var optionsBuilder =
            new DbContextOptionsBuilder<DecisionHelperDbContext>();

        optionsBuilder.UseSqlite(
            AppConfiguration.GetDatabaseConnectionString());

        return new DecisionHelperDbContext(
            optionsBuilder.Options);
    }
}
