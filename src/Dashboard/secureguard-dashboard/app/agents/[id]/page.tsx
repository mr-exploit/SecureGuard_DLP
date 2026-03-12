'use client'

import { useEffect, useState } from 'react'
import { useRouter, useParams } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import { api } from '@/lib/api'
import { Agent, LogEntry } from '@/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import LogViewer from '@/components/LogViewer'
import { formatDate, getStatusColor } from '@/lib/utils'
import { Badge } from '@/components/ui/badge'

export default function AgentDetailPage() {
  const router = useRouter()
  const params = useParams()
  const id = params?.id as string
  const [agent, setAgent] = useState<Agent | null>(null)
  const [logs, setLogs] = useState<LogEntry[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    if (id) {
      Promise.all([
        api.getAgent(id),
        api.getLogs({ agentId: id, pageSize: 50 })
      ]).then(([agentData, logsData]) => {
        setAgent(agentData)
        setLogs(logsData.data)
      }).catch(console.error).finally(() => setLoading(false))
    }
  }, [id, router])

  if (loading) return <div className="flex min-h-screen bg-gray-50"><Sidebar /><div className="flex-1 flex items-center justify-center">Loading...</div></div>
  if (!agent) return <div className="flex min-h-screen bg-gray-50"><Sidebar /><div className="flex-1 flex items-center justify-center">Agent not found.</div></div>

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title={`Agent: ${agent.hostname}`} />
        <main className="flex-1 p-6 space-y-6">
          <Card>
            <CardHeader><CardTitle className="text-base">Agent Details</CardTitle></CardHeader>
            <CardContent>
              <dl className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {[
                  { label: 'Hostname', value: agent.hostname },
                  { label: 'IP Address', value: agent.ipAddress || '-' },
                  { label: 'Username', value: agent.username || '-' },
                  { label: 'OS', value: agent.os || 'Windows' },
                  { label: 'Version', value: agent.version || '-' },
                  { label: 'Last Seen', value: formatDate(agent.lastSeen) },
                ].map(({ label, value }) => (
                  <div key={label}>
                    <dt className="text-xs text-muted-foreground">{label}</dt>
                    <dd className="font-medium text-sm mt-0.5">{value}</dd>
                  </div>
                ))}
                <div>
                  <dt className="text-xs text-muted-foreground">Status</dt>
                  <dd className="mt-0.5">
                    <Badge className={getStatusColor(agent.status)}>{agent.status}</Badge>
                  </dd>
                </div>
              </dl>
            </CardContent>
          </Card>
          <Card>
            <CardHeader><CardTitle className="text-base">Recent Logs</CardTitle></CardHeader>
            <CardContent>
              <LogViewer logs={logs} />
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
