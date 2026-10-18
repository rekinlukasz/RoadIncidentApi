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


    [Fact]
    public async Task CreateAsync_WithValidData_ShouldCreateReportAndReturnDto()
    {
        var dbName = nameof(CreateAsync_WithValidData_ShouldCreateReportAndReturnDto);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Jan Kowalski", Email = "jan@test.pl" });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Title = "Dziura w drodze",
            Type = IncidentType.Przeszkoda,
            City = "Gdańsk",
            Latitude = 54.3520,
            Longitude = 18.6466,
            OccurredAt = DateTime.UtcNow,
            ReporterId = 1
        };

        var result = await service.CreateAsync(dto);

        result.Should().NotBeNull();
        result.Title.Should().Be("Dziura w drodze");
        result.Status.Should().Be(ReportStatus.Nowe);
        result.ReporterName.Should().Be("Jan Kowalski");

        using var assertContext = GetInMemoryDbContext(dbName);
        var savedReport = await assertContext.Reports.FirstOrDefaultAsync(r => r.Id == result.Id);
        savedReport.Should().NotBeNull();
        savedReport!.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task CreateAsync_WhenReporterDoesNotExist_ShouldThrowDomainException()
    {
        var dbName = nameof(CreateAsync_WhenReporterDoesNotExist_ShouldThrowDomainException);
        using var context = GetInMemoryDbContext(dbName);
        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Title = "Wypadek",
            ReporterId = 999
        };

        Func<Task> act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Nie istnieje zgłaszający o id 999.");
    }

    [Fact]
    public async Task CreateAsync_WhenOccurredAtIsFarInFuture_ShouldThrowDomainException()
    {
        var dbName = nameof(CreateAsync_WhenOccurredAtIsFarInFuture_ShouldThrowDomainException);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Test", Email = "t@t.pl" });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            ReporterId = 1,
            OccurredAt = DateTime.UtcNow.AddMinutes(6)
        };

        Func<Task> act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Data wystąpienia zdarzenia nie może być z przyszłości.");
    }

    [Fact]
    public async Task CreateAsync_WhenOccurredAtIsWithin5MinTolerance_ShouldCreateReport()
    {
        var dbName = nameof(CreateAsync_WhenOccurredAtIsWithin5MinTolerance_ShouldCreateReport);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Test", Email = "t@t.pl" });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Title = "Tolerancja czasu",
            ReporterId = 1,
            OccurredAt = DateTime.UtcNow.AddMinutes(4)
        };

        var result = await service.CreateAsync(dto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateExistsWithin100Meters_ShouldThrowDomainException()
    {
        var dbName = nameof(CreateAsync_WhenDuplicateExistsWithin100Meters_ShouldThrowDomainException);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Test", Email = "t@t.pl" });

        context.Reports.Add(new IncidentReport
        {
            Type = IncidentType.Awaria,
            Status = ReportStatus.WTrakcie,
            Latitude = 54.000000,
            Longitude = 18.000000,
            ReporterId = 1
        });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Title = "Duplikat",
            Type = IncidentType.Awaria,
            Latitude = 54.000100,
            Longitude = 18.000100,
            ReporterId = 1,
            OccurredAt = DateTime.UtcNow
        };

        Func<Task> act = async () => await service.CreateAsync(dto);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Istnieje już aktywne zgłoszenie tego samego typu w tej lokalizacji.");
    }

    [Fact]
    public async Task CreateAsync_WhenReportIsSameTypeAndLocationButInactive_ShouldCreateReport()
    {
        var dbName = nameof(CreateAsync_WhenReportIsSameTypeAndLocationButInactive_ShouldCreateReport);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Test", Email = "t@t.pl" });

        context.Reports.Add(new IncidentReport
        {
            Type = IncidentType.Awaria,
            Status = ReportStatus.Zakonczone,
            Latitude = 54.000000,
            Longitude = 18.000000,
            ReporterId = 1
        });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Type = IncidentType.Awaria,
            Latitude = 54.000000,
            Longitude = 18.000000,
            ReporterId = 1,
            OccurredAt = DateTime.UtcNow
        };

        var result = await service.CreateAsync(dto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenReportIsSameLocationButDifferentType_ShouldCreateReport()
    {
        var dbName = nameof(CreateAsync_WhenReportIsSameLocationButDifferentType_ShouldCreateReport);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Test", Email = "t@t.pl" });

        context.Reports.Add(new IncidentReport
        {
            Type = IncidentType.Wypadek,
            Status = ReportStatus.Nowe,
            Latitude = 54.000000,
            Longitude = 18.000000,
            ReporterId = 1
        });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Type = IncidentType.RobotyDrogowe,
            Latitude = 54.000000,
            Longitude = 18.000000,
            ReporterId = 1,
            OccurredAt = DateTime.UtcNow
        };

        var result = await service.CreateAsync(dto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateAsync_WhenReportIsSameTypeButFarAway_ShouldCreateReport()
    {
        var dbName = nameof(CreateAsync_WhenReportIsSameTypeButFarAway_ShouldCreateReport);
        using var context = GetInMemoryDbContext(dbName);
        context.Reporters.Add(new Reporter { Id = 1, FullName = "Test", Email = "t@t.pl" });

        context.Reports.Add(new IncidentReport
        {
            Type = IncidentType.Przeszkoda,
            Status = ReportStatus.Nowe,
            Latitude = 54.000000,
            Longitude = 18.000000,
            ReporterId = 1
        });
        await context.SaveChangesAsync();

        var service = new ReportService(context);
        var dto = new CreateReportDto
        {
            Type = IncidentType.Przeszkoda,
            Latitude = 54.500000,
            Longitude = 18.500000,
            ReporterId = 1,
            OccurredAt = DateTime.UtcNow
        };

        var result = await service.CreateAsync(dto);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenReportExists_ShouldDeleteReport()
    {
        var dbName = nameof(DeleteAsync_WhenReportExists_ShouldDeleteReport);
        using (var context = GetInMemoryDbContext(dbName))
        {
            context.Reporters.Add(new Reporter { Id = 1, FullName = "Jan Kowalski", Email = "jan@example.com" });
            context.Reports.Add(new IncidentReport
            {
                Id = 1,
                Title = "Test Incident",
                Description = "Test Description",
                City = "Warszawa",
                Status = ReportStatus.Nowe,
                ReporterId = 1,
                OccurredAt = DateTime.UtcNow.AddHours(-1)
            });
            await context.SaveChangesAsync();
        }

        using var serviceContext = GetInMemoryDbContext(dbName);
        var service = new ReportService(serviceContext);

        await service.DeleteAsync(1);

        using var assertContext = GetInMemoryDbContext(dbName);
        var deletedReport = await assertContext.Reports.FindAsync(1);
        deletedReport.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenReportDoesNotExist_ShouldThrowNotFoundException()
    {
        var dbName = nameof(DeleteAsync_WhenReportDoesNotExist_ShouldThrowNotFoundException);
        using var context = GetInMemoryDbContext(dbName);
        var service = new ReportService(context);

        Func<Task> act = async () => await service.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Nie znaleziono zgłoszenia o id 999.");
    }

}