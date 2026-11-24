
using Hl7.Fhir.Model;

namespace RehabConnect.Api.Fhir;

public static class FhirObservationSearch
{
    public static Bundle Search(
        string? patientId,
        string baseUrl)
    {
        var observations = FhirSampleData.CreatePainObservations();

        var matches = observations
            .Where(observation =>
                string.IsNullOrWhiteSpace(patientId) ||
                observation.Subject?.Reference ==
                    $"Patient/{patientId}")
            .ToList();

        var bundle = new Bundle
        {
            Type = Bundle.BundleType.Searchset,
            Total = matches.Count
        };

        foreach (var observation in matches)
        {
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl =
                    $"{baseUrl.TrimEnd('/')}/fhir/Observation/{observation.Id}",

                Resource = observation,

                Search = new Bundle.SearchComponent
                {
                    Mode = Bundle.SearchEntryMode.Match
                }
            });
        }

        return bundle;
    }
}
