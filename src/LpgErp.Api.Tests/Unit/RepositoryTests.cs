using FluentAssertions;
using LpgErp.Domain.Entities;
using LpgErp.Infrastructure.Persistence;
using LpgErp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LpgErp.Api.Tests.Unit;

public class RepositoryTests
{
    private LpgErpDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<LpgErpDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new LpgErpDbContext(options);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        using var context = CreateInMemoryContext();
        var repository = new Repository<Brand>(context);

        var brand = new Brand { Name = "Bashundhara", Code = "BSH" };

        await repository.AddAsync(brand);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(brand.Id);
        result.Should().NotBeNull();
        result!.Name.Should().Be("Bashundhara");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnNonDeletedEntities()
    {
        using var context = CreateInMemoryContext();
        var repository = new Repository<Warehouse>(context);

        await repository.AddAsync(new Warehouse { Name = "Main Warehouse" });
        await repository.AddAsync(new Warehouse { Name = "City Warehouse" });
        await context.SaveChangesAsync();

        var result = await repository.GetAllAsync();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Delete_ShouldSoftDelete()
    {
        using var context = CreateInMemoryContext();
        var repository = new Repository<Warehouse>(context);

        var warehouse = new Warehouse { Name = "Test Warehouse" };
        await repository.AddAsync(warehouse);
        await context.SaveChangesAsync();

        repository.Delete(warehouse);
        await context.SaveChangesAsync();

        var result = await repository.GetByIdAsync(warehouse.Id);
        result.Should().BeNull();
    }
}
