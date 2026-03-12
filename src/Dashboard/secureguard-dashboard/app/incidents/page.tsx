'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import IncidentTable from '@/components/IncidentTable'
import { api } from '@/lib/api'
import { Alert } from '@/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

export default function IncidentsPage() {
  const router = useRouter()
  const [alerts, setAlerts] = useState<Alert[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [filter, setFilter] = useState({ severity: '', acknowledged: '' })

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    loadAlerts()
  }, [page, filter, router])

  const loadAlerts = async () => {
    setLoading(true)
    try {
      const params: Record<string, string | number> = { page, pageSize: 50 }
      if (filter.severity) params.severity = filter.severity
      if (filter.acknowledged !== '') params.acknowledged = filter.acknowledged
      const result = await api.getAlerts(params)
      setAlerts(result.data)
      setTotal(result.total)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleAcknowledge = async (id: number) => {
    await api.acknowledgeAlert(id)
    loadAlerts()
  }

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title="Incidents" />
        <main className="flex-1 p-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle className="text-base">Alerts & Incidents ({total})</CardTitle>
                <div className="flex gap-2">
                  <select
                    className="border rounded-md px-3 py-1.5 text-sm"
                    value={filter.severity}
                    onChange={e => { setFilter({ ...filter, severity: e.target.value }); setPage(1) }}
                  >
                    <option value="">All Severity</option>
                    <option value="CRITICAL">Critical</option>
                    <option value="HIGH">High</option>
                    <option value="MEDIUM">Medium</option>
                    <option value="LOW">Low</option>
                  </select>
                  <select
                    className="border rounded-md px-3 py-1.5 text-sm"
                    value={filter.acknowledged}
                    onChange={e => { setFilter({ ...filter, acknowledged: e.target.value }); setPage(1) }}
                  >
                    <option value="">All Status</option>
                    <option value="false">Active</option>
                    <option value="true">Acknowledged</option>
                  </select>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <IncidentTable incidents={alerts} onAcknowledge={handleAcknowledge} loading={loading} />
              <div className="flex justify-between items-center mt-4">
                <p className="text-sm text-muted-foreground">Page {page} · {total} total</p>
                <div className="flex gap-2">
                  <Button size="sm" variant="outline" disabled={page === 1} onClick={() => setPage(p => p - 1)}>Previous</Button>
                  <Button size="sm" variant="outline" disabled={page * 50 >= total} onClick={() => setPage(p => p + 1)}>Next</Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
