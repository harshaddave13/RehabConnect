import { useState } from 'react'
import PainTrendChart from './components/PainTrendChart'

import {
  getPatient,
  searchPatients,
  getPatientObservations,
  type FhirPatient,
  type FhirObservation,
} from './services/fhirApi'

import './App.css'

function App() {
  const [token, setToken] = useState('')
  const [patient, setPatient] = useState<FhirPatient | null>(null)
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
const [family, setFamily] = useState('')
const [searchResults, setSearchResults] = useState<FhirPatient[]>([])
const [searching, setSearching] = useState(false)
const [observations, setObservations] = useState<FhirObservation[]>([])
const [loadingObservations, setLoadingObservations] = useState(false)


async function loadObservations() {
  setError('')
  setObservations([])
  setLoadingObservations(true)

  try {
    const results = await getPatientObservations(
      'patient-001',
      token.trim()
    )
    setObservations(results)
  } catch (err) {
    setError(
      err instanceof Error ? err.message : 'Failed to load observations'
    )
  } finally {
    setLoadingObservations(false)
  }
}


  async function loadPatient() {
    setError('')
    setPatient(null)
    setLoading(true)

    try {
      const result = await getPatient('patient-001', token.trim())
      setPatient(result)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Request failed')
    } finally {
      setLoading(false)
    }
  }

  
async function handleSearch() {
  setError('')
  setSearchResults([])
  setSearching(true)

  try {
    const results = await searchPatients(family.trim(), token.trim())
    setSearchResults(results)
  } catch (err) {
    setError(err instanceof Error ? err.message : 'Search failed')
  } finally {
    setSearching(false)
  }
}


  return (
    <div className="app-layout">
      <aside className="sidebar">
        <h2>RehabConnect</h2>
        <nav>
          <a href="#" className="active">Dashboard</a>
          <a href="#">Patients</a>
          <a href="#">Pain Tracking</a>
        </nav>
      </aside>

      <main className="main-content">
        <header className="topbar">
          <h1>Clinician Dashboard</h1>
          <span>RehabConnect MVP</span>
        </header>

        <section className="welcome">
          <h2>FHIR Patient Viewer</h2>
          <p>
            Load synthetic patient data from the secured .NET API.
          </p>

          <label htmlFor="token">Development JWT</label>
          <input
            id="token"
            type="password"
            value={token}
            onChange={e => setToken(e.target.value)}
            placeholder="Paste Clinician JWT"
            autoComplete="off"
            style={{
              display: 'block',
              width: '100%',
              padding: '12px',
              margin: '12px 0',
            }}
          />

          <button
            onClick={loadPatient}
            disabled={loading || !token.trim()}
          >
            {loading ? 'Loading...' : 'Load Patient'}
          </button>

          {error && <p role="alert">{error}</p>}
        </section>


<section className="dashboard-card" style={{ marginBottom: 24 }}>
  <h2>Search Patients</h2>

  <label htmlFor="family">Family name</label>
  <input
    id="family"
    type="text"
    value={family}
    onChange={e => setFamily(e.target.value)}
    placeholder="Enter family name, e.g. Smith"
    style={{
      display: 'block',
      padding: '12px',
      margin: '12px 0',
      width: '100%',
    }}
  />

  <button
    onClick={handleSearch}
    disabled={searching || !family.trim() || !token.trim()}
  >
    {searching ? 'Searching...' : 'Search'}
  </button>

  {searchResults.length > 0 && (
    <div style={{ marginTop: 20 }}>
      <h3>Results</h3>

      {searchResults.map(result => (
        <div key={result.id} style={{ marginBottom: 12 }}>
          <strong>
            {result.name?.[0]?.given?.join(' ')}{' '}
            {result.name?.[0]?.family}
          </strong>
          <p>FHIR ID: {result.id}</p>
        </div>
      ))}
    </div>
  )}
</section>


<section className="dashboard-card" style={{ marginTop: 24 }}>
  <h2>Pain Tracking</h2>
  <p>Patient: John Smith (synthetic test data)</p>

  <button
    onClick={loadObservations}
    disabled={loadingObservations || !token.trim()}
  >
    {loadingObservations ? 'Loading...' : 'Load Pain Observations'}
  </button>


{observations.length > 0 && (
  <PainTrendChart observations={observations} />
)}

  {observations.length > 0 && (
    <div style={{ marginTop: 20 }}>
      {observations.map(observation => (
        <div key={observation.id} className="pain-observation">
          <h3>
            {observation.code?.text ??
              observation.code?.coding?.[0]?.display ??
              'Pain Observation'}
          </h3>

          <p>
            <strong>Pain Score:</strong>{' '}
            {observation.valueQuantity?.value ?? 'Not recorded'}
            {observation.valueQuantity?.unit
              ? ` ${observation.valueQuantity.unit}`
              : ''}
          </p>


{typeof observation.valueQuantity?.value === 'number' &&
  observation.valueQuantity.value >= 0 &&
  observation.valueQuantity.value <= 10 && (
    <div className="pain-score-container">
      <div className="pain-score-label">
        <span>Pain severity</span>
        <strong>{observation.valueQuantity.value} / 10</strong>
      </div>

      <div
        className="pain-score-track"
        role="progressbar"
        aria-label="Reported pain severity"
        aria-valuemin={0}
        aria-valuemax={10}
        aria-valuenow={observation.valueQuantity.value}
      >
        <div
          className="pain-score-fill"
          style={{
            width: `${observation.valueQuantity.value * 10}%`,
          }}
        />
      </div>

      <div className="pain-score-scale">
        <span>0 — No pain</span>
        <span>10 — Worst pain</span>
      </div>
    </div>
  )}


          <p>
            <strong>Recorded:</strong>{' '}
            {observation.effectiveDateTime
              ? new Date(observation.effectiveDateTime).toLocaleString()
              : 'Date unavailable'}
          </p>

          <p>
            <strong>Status:</strong> {observation.status ?? 'Unknown'}
          </p>
        </div>
      ))}
    </div>
  )}
</section>

        {patient && (
          <section className="dashboard-card">
            <h2>Patient Details</h2>
            <p><strong>FHIR ID:</strong> {patient.id}</p>
            <p>
              <strong>Name:</strong>{' '}
              {patient.name?.[0]?.given?.join(' ')}{' '}
              {patient.name?.[0]?.family}
            </p>
          </section>
        )}
      </main>
    </div>
  )
}

export default App
