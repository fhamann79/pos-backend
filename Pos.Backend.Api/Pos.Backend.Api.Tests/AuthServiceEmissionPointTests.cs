using Microsoft.AspNetCore.Identity;
using Pos.Backend.Api.Core.DTOs;
using Pos.Backend.Api.Core.Entities;
using Pos.Backend.Api.Core.Services;
using Xunit;

namespace Pos.Backend.Api.Tests;

public class AuthServiceEmissionPointTests
{
    [Fact]
    public async Task ValidateLoginAsync_ReturnsUser_WhenEmissionPointIsActive()
    {
        // Arrange: contexto con entidades activas y usuario asignado.
        await using var context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var password = "P@ssw0rd!";

        var company = new Company
        {
            Id = 1,
            Name = "Demo Co",
            Ruc = "1234567890001",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var establishment = new Establishment
        {
            Id = 10,
            CompanyId = company.Id,
            Company = company,
            Code = "001",
            Name = "Matriz",
            Address = "Av. Central",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var emissionPoint = new EmissionPoint
        {
            Id = 100,
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            Code = "001",
            Name = "Caja 1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var user = BuildUserWithPassword(
            id: 1000,
            username: "demo",
            email: "demo@example.com",
            password: password,
            companyId: company.Id,
            establishmentId: establishment.Id,
            emissionPointId: emissionPoint.Id);

        user.Company = company;
        user.Establishment = establishment;
        user.EmissionPoint = emissionPoint;

        context.Companies.Add(company);
        context.Establishments.Add(establishment);
        context.EmissionPoints.Add(emissionPoint);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context);
        var loginDto = new LoginDto { Username = user.Username, Password = password };

        // Act
        var (resultUser, error) = await service.ValidateLoginAsync(loginDto);

        // Assert
        Assert.NotNull(resultUser);
        Assert.Equal(user.Id, resultUser?.Id);
        Assert.Equal(string.Empty, error);
    }

    [Fact]
    public async Task ValidateLoginAsync_ReturnsError_WhenEmissionPointIsInactive()
    {
        // Arrange: punto de emisión existente pero inactivo.
        await using var context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var password = "P@ssw0rd!";

        var company = new Company
        {
            Id = 2,
            Name = "Demo Co",
            Ruc = "1234567890002",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var establishment = new Establishment
        {
            Id = 20,
            CompanyId = company.Id,
            Company = company,
            Code = "002",
            Name = "Sucursal",
            Address = "Av. Norte",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var emissionPoint = new EmissionPoint
        {
            Id = 200,
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            Code = "002",
            Name = "Caja 2",
            CreatedAt = DateTime.UtcNow,
            IsActive = false
        };

        var user = BuildUserWithPassword(
            id: 2000,
            username: "inactive-ep",
            email: "inactive-ep@example.com",
            password: password,
            companyId: company.Id,
            establishmentId: establishment.Id,
            emissionPointId: emissionPoint.Id);

        user.Company = company;
        user.Establishment = establishment;
        user.EmissionPoint = emissionPoint;

        context.Companies.Add(company);
        context.Establishments.Add(establishment);
        context.EmissionPoints.Add(emissionPoint);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context);
        var loginDto = new LoginDto { Username = user.Username, Password = password };

        // Act
        var (resultUser, error) = await service.ValidateLoginAsync(loginDto);

        // Assert
        Assert.Null(resultUser);
        Assert.Equal("EMISSION_POINT_INACTIVE_OR_NOT_FOUND", error);
    }

    [Fact]
    public async Task ValidateLoginAsync_ReturnsError_WhenEmissionPointNotAssigned()
    {
        // Arrange: usuario sin punto de emisión asignado.
        await using var context = TestDbContextFactory.CreateInMemoryContext(Guid.NewGuid().ToString());
        var password = "P@ssw0rd!";

        var company = new Company
        {
            Id = 3,
            Name = "Demo Co",
            Ruc = "1234567890003",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var establishment = new Establishment
        {
            Id = 30,
            CompanyId = company.Id,
            Company = company,
            Code = "003",
            Name = "Bodega",
            Address = "Av. Sur",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var user = BuildUserWithPassword(
            id: 3000,
            username: "no-ep",
            email: "no-ep@example.com",
            password: password,
            companyId: company.Id,
            establishmentId: establishment.Id,
            emissionPointId: 0);

        user.Company = company;
        user.Establishment = establishment;

        context.Companies.Add(company);
        context.Establishments.Add(establishment);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context);
        var loginDto = new LoginDto { Username = user.Username, Password = password };

        // Act
        var (resultUser, error) = await service.ValidateLoginAsync(loginDto);

        // Assert
        Assert.Null(resultUser);
        Assert.Equal("EMISSION_POINT_NOT_ASSIGNED", error);
    }

    private static User BuildUserWithPassword(
        int id,
        string username,
        string email,
        string password,
        int companyId,
        int? establishmentId,
        int emissionPointId)
    {
        var user = new User
        {
            Id = id,
            Username = username,
            Email = email,
            CompanyId = companyId,
            EstablishmentId = establishmentId,
            EmissionPointId = emissionPointId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, password);

        return user;
    }
}
