using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using RoadIncidentApi.Data;
using RoadIncidentApi.Models;
using RoadIncidentApi.DTOs;
using RoadIncidentApi.Services;
using RoadIncidentApi.Exceptions;

public class ReportServiceTests
{
    private AppDbContext GetInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ChangeStatusAsync_WithValidTransition_ShouldUpdateStatus()
    {
        var dbName = nameof(ChangeStatusAsync_WithValidTransition_ShouldUpdateStatus);
        
        using (var context = GetInMemoryDbContext(dbName))
        {
            // Najpierw dodajemy zgłaszającego, aby spełnić relację klucza obcego
            var reporter = new Reporter { Id = 1, FullName = "Jan Kowalski", Email = "jan@example.com" };
            context.Reporters.Add(reporter);

            context.Reports.Add(new IncidentReport 
            { 
                Id = 1, 
                Status = ReportStatus.Nowe,
                Title = "Test Incident",
                Description = "Test Description",
                City = "Warszawa",
                Latitude = 52.2297,
                Longitude = 21.0122,
                OccurredAt = DateTime.UtcNow.AddHours(-1),
                ReporterId = 1
            });
            await context.SaveChangesAsync();
        }

        using var serviceContext = GetInMemoryDbContext(dbName);
        var service = new ReportService(serviceContext);

        var statusDto = new UpdateReportStatusDto
        {
            NewStatus = ReportStatus.Zweryfikowane
        };

        var result = await service.ChangeStatusAsync(1, statusDto);

        result.Should().NotBeNull();
        result.Status.Should().Be(ReportStatus.Zweryfikowane);

        using var assertContext = GetInMemoryDbContext(dbName);
        var updatedReport = await assertContext.Reports.FindAsync(1);
        updatedReport!.Status.Should().Be(ReportStatus.Zweryfikowane);
        updatedReport.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenReportDoesNotExist_ShouldThrowNotFoundException()
    {
        var dbName = nameof(ChangeStatusAsync_WhenReportDoesNotExist_ShouldThrowNotFoundException);
        using var context = GetInMemoryDbContext(dbName);
        var service = new ReportService(context);

        var statusDto = new UpdateReportStatusDto
        {
            NewStatus = ReportStatus.Zweryfikowane
        };

        Func<Task> act = async () => await service.ChangeStatusAsync(999, statusDto);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Nie znaleziono zgłoszenia o id 999.");
    }

    [Fact]
    public async Task ChangeStatusAsync_WhenStatusIsTheSame_ShouldThrowDomainException()
    {
        var dbName = nameof(ChangeStatusAsync_WhenStatusIsTheSame_ShouldThrowDomainException);
        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Reporters.Add(new Reporter { Id = 1, FullName = "Jan Kowalski", Email = "jan@example.com" });
            context.Reports.Add(new IncidentReport 
            { 
                Id = 1, 
                Status = ReportStatus.Nowe,
                Title = "Test",
                Description = "Test",
                City = "Warszawa",
                ReporterId = 1
            });
            await context.SaveChangesAsync();
        }

        using var serviceContext = GetInMemoryDbContext(dbName);
        var service = new ReportService(serviceContext);

        var statusDto = new UpdateReportStatusDto
        {
            NewStatus = ReportStatus.Nowe
        };

        Func<Task> act = async () => await service.ChangeStatusAsync(1, statusDto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Zgłoszenie ma już status 'Nowe'.");
    }

    [Fact]
    public async Task ChangeStatusAsync_WithInvalidTransition_ShouldThrowDomainException()
    {
        var dbName = nameof(ChangeStatusAsync_WithInvalidTransition_ShouldThrowDomainException);
        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Reporters.Add(new Reporter { Id = 1, FullName = "Jan Kowalski", Email = "jan@example.com" });
            context.Reports.Add(new IncidentReport 
            { 
                Id = 1, 
                Status = ReportStatus.Nowe,
                Title = "Test",
                Description = "Test",
                City = "Warszawa",
                ReporterId = 1
            });
            await context.SaveChangesAsync();
        }

        using var serviceContext = GetInMemoryDbContext(dbName);
        var service = new ReportService(serviceContext);

        var statusDto = new UpdateReportStatusDto
        {
            NewStatus = ReportStatus.Zakonczone
        };

        Func<Task> act = async () => await service.ChangeStatusAsync(1, statusDto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*Niedozwolona zmiana statusu*");
    }
}