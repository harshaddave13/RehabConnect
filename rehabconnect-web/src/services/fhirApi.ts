
export interface FhirPatient {
  resourceType: 'Patient'
  id: string
  name?: Array<{
    given?: string[]
    family?: string
  }>
}

export async function getPatient(
  patientId: string,
  token: string
): Promise<FhirPatient> {
  const response = await fetch(
    `/fhir/Patient/${encodeURIComponent(patientId)}`,
    {
      headers: {
        Accept: 'application/fhir+json',
        Authorization: `Bearer ${token}`,
      },
    }
  )

  if (!response.ok) {
    throw new Error(
      `FHIR API returned ${response.status}`
    )
  }

  return response.json() as Promise<FhirPatient>
}


export interface FhirBundle<T> {
  resourceType: 'Bundle'
  type: 'searchset'
  total?: number
  entry?: Array<{
    resource: T
  }>
}

export async function searchPatients(
  family: string,
  token: string
): Promise<FhirPatient[]> {
  const response = await fetch(
    `/fhir/Patient?family=${encodeURIComponent(family)}`,
    {
      headers: {
        Accept: 'application/fhir+json',
        Authorization: `Bearer ${token}`,
      },
    }
  )

  if (!response.ok) {
    throw new Error(
      `Patient search failed: ${response.status}`
    )
  }

  const bundle = await response.json() as FhirBundle<FhirPatient>

  return bundle.entry?.map(entry => entry.resource) ?? []
}



export interface FhirObservation {
  resourceType: 'Observation'
  id: string
  status?: string
  effectiveDateTime?: string
  valueQuantity?: {
    value?: number
    unit?: string
  }
  code?: {
    text?: string
    coding?: Array<{
      display?: string
    }>
  }
}

export async function getPatientObservations(
  patientId: string,
  token: string
): Promise<FhirObservation[]> {
  const response = await fetch(
    `/fhir/Observation?patient=${encodeURIComponent(patientId)}`,
    {
      headers: {
        Accept: 'application/fhir+json',
        Authorization: `Bearer ${token}`,
      },
    }
  )

  if (!response.ok) {
    throw new Error(
      `Observation search failed: ${response.status}`
    )
  }

  const bundle =
    await response.json() as FhirBundle<FhirObservation>

  return bundle.entry?.map(entry => entry.resource) ?? []
}
