using System.Text.Json;
using Cdsqg.Core.Entities;
namespace Cdsqg.Application.Services;
public static class DigitalPlanMonitoringDefaults {
 private sealed record Seed(DigitalPlanSource[] Sources,DigitalPlanKeyword[] Keywords);
 private static Seed Load(){using var stream=typeof(DigitalPlanMonitoringDefaults).Assembly.GetManifestResourceStream("Cdsqg.DigitalPlanMonitoring.json")!;return JsonSerializer.Deserialize<Seed>(stream)!;}
 public static DigitalPlanSource[] Sources()=>Load().Sources;
 public static DigitalPlanKeyword[] Keywords()=>Load().Keywords;
}
