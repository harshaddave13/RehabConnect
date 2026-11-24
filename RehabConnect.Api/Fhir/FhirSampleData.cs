
using Hl7.Fhir.Model;

namespace RehabConnect.Api.Fhir;

public static class FhirSampleData
{
    public static Patient CreatePatient()
    {
        return new Patient
        {
            Id = "patient-001",
            Active = true,
            Name =
            [
                new HumanName
                {
                    Use = HumanName.NameUse.Official,
                    Family = "Smith",
                    Given = ["John"]
                }
            ],
            Gender = AdministrativeGender.Male,
            BirthDate = "1985-04-12"
        };
    }


    public static Observation CreatePainObservation()
    {
        return new Observation
        {
            Id = "pain-001",
            Status = ObservationStatus.Final,
            Code = new CodeableConcept(
                "http://loinc.org",
                "72514-3",
                "Pain severity - 0-10 verbal numeric rating [Score] - Reported"
            ),
            Subject = new ResourceReference("Patient/patient-001"),
            Effective = new FhirDateTime("2026-10-23"),
            Value = new Quantity
            {
                Value = 4,
                Unit = "{score}",
                System = "http://unitsofmeasure.org",
                Code = "{score}"
            }
        };
    }



    public static List<Observation> CreatePainObservations()
    {
        return
        [
            CreateAdditionalPainObservation(
            "pain-002", "2026-10-02", 8),

        CreateAdditionalPainObservation(
            "pain-003", "2026-10-09", 6),

        CreateAdditionalPainObservation(
            "pain-004", "2026-10-16", 5),

        CreatePainObservation()
        ];
    }

    private static Observation CreateAdditionalPainObservation(
        string id,
        string date,
        decimal score)
    {
        return new Observation
        {
            Id = id,
            Status = ObservationStatus.Final,
            Code = new CodeableConcept(
                "http://loinc.org",
                "72514-3",
                "Pain severity - 0-10 verbal numeric rating [Score] - Reported"
            ),
            Subject = new ResourceReference("Patient/patient-001"),
            Effective = new FhirDateTime(date),
            Value = new Quantity
            {
                Value = score,
                Unit = "{score}",
                System = "http://unitsofmeasure.org",
                Code = "{score}"
            }
        };
    }


}
