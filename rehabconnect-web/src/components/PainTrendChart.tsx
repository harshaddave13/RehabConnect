
import {
  CartesianGrid,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

import type { FhirObservation } from '../services/fhirApi'

interface PainTrendChartProps {
  observations: FhirObservation[]
}

function PainTrendChart({ observations }: PainTrendChartProps) {
  const chartData = observations
    .filter(
      observation =>
        observation.effectiveDateTime &&
        typeof observation.valueQuantity?.value === 'number'
    )
    .map(observation => ({
      date: observation.effectiveDateTime!,
      score: observation.valueQuantity!.value!,
    }))
    .sort((a, b) => a.date.localeCompare(b.date))

  if (chartData.length < 2) {
    return <p>At least two observations are needed to show a trend.</p>
  }

  return (
    <div className="pain-trend">
      <h3>Pain Score History</h3>
      <p>Synthetic patient-reported pain scores over time.</p>

      <div style={{ width: '100%', height: 300 }}>
        <ResponsiveContainer width="100%" height="100%">
          <LineChart
            data={chartData}
            margin={{ top: 10, right: 20, left: 0, bottom: 10 }}
          >
            <CartesianGrid strokeDasharray="3 3" />

            <XAxis
              dataKey="date"
              tickFormatter={value =>
                new Date(value).toLocaleDateString('en-GB', {
                  day: 'numeric',
                  month: 'short',
                })
              }
            />

            <YAxis domain={[0, 10]} ticks={[0, 2, 4, 6, 8, 10]} />

            <Tooltip
              labelFormatter={label => `Date: ${String(label)}`}
              formatter={value => [`${value} / 10`, 'Pain score']}
            />

            <Line
              type="linear"
              dataKey="score"
              stroke="#2878b5"
              strokeWidth={3}
              dot={{ r: 5 }}
              activeDot={{ r: 7 }}
            />
          </LineChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}

export default PainTrendChart
