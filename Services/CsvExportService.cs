using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using RoadIncidentApi.DTOs;

namespace RoadIncidentApi.Services;

public class CsvExportService : ICsvExportService
{
    public byte[] ExportReports(IEnumerable<ReportDto> reports)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";" // średnik — przyjazny dla Excela w polskiej lokalizacji
        };

        using var memory = new MemoryStream();
        // UTF-8 z BOM, aby Excel poprawnie wyświetlał polskie znaki.
        using (var writer = new StreamWriter(memory, new UTF8Encoding(true)))
        using (var csv = new CsvWriter(writer, config))
        {
            csv.WriteRecords(reports.Select(r => new
            {
                r.Id,
                r.Title,
                Typ = r.Type.ToString(),
                Status = r.Status.ToString(),
                r.City,
                r.Street,
                r.Latitude,
                r.Longitude,
                OccurredAt = r.OccurredAt.ToString("yyyy-MM-dd HH:mm"),
                CreatedAt = r.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                Zglaszajacy = r.ReporterName
            }));
        }

        return memory.ToArray();
    }
}
