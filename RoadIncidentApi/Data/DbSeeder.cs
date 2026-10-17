using RoadIncidentApi.Models;

namespace RoadIncidentApi.Data;

/// <summary>Wypełnia bazę przykładowymi danymi przy pierwszym uruchomieniu.</summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Reporters.Any())
            return;

        var anna = new Reporter
        {
            FullName = "Anna Kowalska",
            Email = "anna.kowalska@example.com",
            PhoneNumber = "+48600100200",
            CreatedAt = DateTime.UtcNow
        };

        var jan = new Reporter
        {
            FullName = "Jan Nowak",
            Email = "jan.nowak@example.com",
            PhoneNumber = "+48600300400",
            CreatedAt = DateTime.UtcNow
        };

        db.Reporters.AddRange(anna, jan);
        db.SaveChanges();

        db.Reports.AddRange(
            new IncidentReport
            {
                Title = "Kolizja dwóch samochodów na skrzyżowaniu",
                Description = "Zablokowany prawy pas, utrudnienia w ruchu.",
                Type = IncidentType.Wypadek,
                Status = ReportStatus.Nowe,
                City = "Gdynia",
                Street = "ul. Morska",
                Latitude = 54.5189,
                Longitude = 18.5305,
                OccurredAt = DateTime.UtcNow.AddHours(-2),
                CreatedAt = DateTime.UtcNow,
                ReporterId = anna.Id
            },
            new IncidentReport
            {
                Title = "Roboty drogowe — wymiana nawierzchni",
                Description = "Zwężenie do jednego pasa, ruch wahadłowy.",
                Type = IncidentType.RobotyDrogowe,
                Status = ReportStatus.WTrakcie,
                City = "Gdańsk",
                Street = "al. Grunwaldzka",
                Latitude = 54.3852,
                Longitude = 18.5926,
                OccurredAt = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                ReporterId = jan.Id
            }
        );

        db.SaveChanges();
    }
}
