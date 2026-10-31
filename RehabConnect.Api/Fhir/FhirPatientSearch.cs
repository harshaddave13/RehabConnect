
using Hl7.Fhir.Model;

namespace RehabConnect.Api.Fhir;

public static class FhirPatientSearch
{
    public static Bundle Search(string? family,string baseUrl)
    {
        // In-memory synthetic data for our prototype.
        var patients = new[]
        {
            FhirSampleData.CreatePatient()
        };

        var matches = patients
            .Where(patient =>
                string.IsNullOrWhiteSpace(family) ||
                patient.Name.Any(name =>
                    name.Family != null &&
                    name.Family.StartsWith(
                        family,
                        StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Searchset,
            Total = matches.Count
        };

        foreach (var patient in matches)
        {
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl =
                    $"{baseUrl.TrimEnd('/')}/fhir/Patient/{patient.Id}",

                Resource = patient,

                Search = new Bundle.SearchComponent
                {
                    Mode = Bundle.SearchEntryMode.Match
                }
            });
        }

        return bundle;
    }
}
