'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import LogViewer from '@/components/LogViewer'
import { api } from '@/lib/api'
import { LogEntry } from '@/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

export default function LogsPage() {
  const router = useRouter()
  const [logs, setLogs] = useState<LogEntry[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [loading, setLoading] = useState(true)
  const [search, setSearch] = useState({ violationType: '', severity: '' })

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    loadLogs()
  }, [page, search, router])

  const loadLogs = async () => {
    setLoading(true)
    try {
      const params: Record<string, string | number> = { page, pageSize: 100 }
      if (search.violationType) params.violationType = search.violationType
      if (search.severity) params.severity = search.severity
      const result = await api.getLogs(params)
      setLogs(result.data)
      setTotal(result.total)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title="Log Viewer" />
        <main className="flex-1 p-6">
          <Card>
            <CardHeader>
              <div className="flex items-center justify-between flex-wrap gap-2">
                <CardTitle className="text-base">Security Logs ({total})</CardTitle>
                <div className="flex gap-2">
                  <select
                    className="border rounded-md px-3 py-1.5 text-sm"
                    value={search.violationType}
                    onChange={e => { setSearch({ ...search, violationType: e.target.value }); setPage(1) }}
                  >
                    <option value="">All Violations</option>
                    <option value="IMAGE_UPLOAD">Image Upload</option>
                    <option value="CREDENTIAL_FILE">Credential File</option>
                    <option value="UNKNOWN_IP">Unknown IP</option>
                    <option value="CREDENTIAL_PATTERN">Credential Pattern</option>
                  </select>
                  <select
                    className="border rounded-md px-3 py-1.5 text-sm"
                    value={search.severity}
                    onChange={e => { setSearch({ ...search, severity: e.target.value }); setPage(1) }}
                  >
                    <option value="">All Severity</option>
                    <option value="CRITICAL">Critical</option>
                    <option value="HIGH">High</option>
                    <option value="MEDIUM">Medium</option>
                    <option value="LOW">Low</option>
                  </select>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <LogViewer logs={logs} loading={loading} />
              <div className="flex justify-between items-center mt-4">
                <p className="text-sm text-muted-foreground">Page {page} · {total} total records</p>
                <div className="flex gap-2">
                  <Button size="sm" variant="outline" disabled={page === 1} onClick={() => setPage(p => p - 1)}>Previous</Button>
                  <Button size="sm" variant="outline" disabled={page * 100 >= total} onClick={() => setPage(p => p + 1)}>Next</Button>
                </div>
              </div>
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
