using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DecisionHelper.Data;

public class DecisionHelperDbContextFactory
    : IDesignTimeDbContextFactory<DecisionHelperDbContext>
{
    public DecisionHelperDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<DecisionHelperDbContext>();

        optionsBuilder.UseSqlite(
            "Data Source=decision-helper.db");

        return new DecisionHelperDbContext(
            optionsBuilder.Options);
    }
}
