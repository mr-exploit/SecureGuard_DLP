'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import AgentList from '@/components/AgentList'
import { api } from '@/lib/api'
import { Agent } from '@/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function AgentsPage() {
  const router = useRouter()
  const [agents, setAgents] = useState<Agent[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    loadAgents()
    const interval = setInterval(loadAgents, 30000)
    return () => clearInterval(interval)
  }, [router])

  const loadAgents = async () => {
    try {
      const data = await api.getAgents()
      setAgents(data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const online = agents.filter(a => a.status === 'online').length

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title="Agents" />
        <main className="flex-1 p-6">
          <div className="mb-4 flex gap-4">
            <div className="bg-white rounded-lg border px-4 py-3 flex items-center gap-2">
              <span className="h-2 w-2 rounded-full bg-green-500 inline-block" />
              <span className="text-sm font-medium">{online} Online</span>
            </div>
            <div className="bg-white rounded-lg border px-4 py-3 flex items-center gap-2">
              <span className="h-2 w-2 rounded-full bg-gray-400 inline-block" />
              <span className="text-sm font-medium">{agents.length - online} Offline</span>
            </div>
          </div>
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Registered Agents ({agents.length})</CardTitle>
            </CardHeader>
            <CardContent>
              <AgentList agents={agents} loading={loading} />
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
