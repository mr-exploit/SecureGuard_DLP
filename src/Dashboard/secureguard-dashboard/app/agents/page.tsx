"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import type { Agent } from "@/types"
import { agentsApi } from "@/lib/api"
import DashboardLayout from "@/components/DashboardLayout"
import AgentList from "@/components/AgentList"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { RefreshCw, Monitor, Wifi, WifiOff } from "lucide-react"

export default function AgentsPage() {
  const router = useRouter()
  const [agents, setAgents] = useState<Agent[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.replace("/login")
      return
    }
    fetchAgents()
  }, [router])

  const fetchAgents = async () => {
    setLoading(true)
    try {
      const data = await agentsApi.getAll()
      setAgents(data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const online = agents.filter((a) => a.isOnline).length
  const offline = agents.filter((a) => !a.isOnline).length

  return (
    <DashboardLayout title="Agents">
      <div className="space-y-4">
        {/* Summary cards */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <div className="p-2 rounded-full bg-blue-50">
                <Monitor className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Total Agents</p>
                <p className="text-2xl font-bold">{agents.length}</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <div className="p-2 rounded-full bg-green-50">
                <Wifi className="h-5 w-5 text-green-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Online</p>
                <p className="text-2xl font-bold text-green-600">{online}</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <div className="p-2 rounded-full bg-gray-100">
                <WifiOff className="h-5 w-5 text-gray-500" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Offline</p>
                <p className="text-2xl font-bold text-gray-500">{offline}</p>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Agents table */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>All Agents</CardTitle>
            <Button variant="outline" size="sm" onClick={fetchAgents} disabled={loading}>
              <RefreshCw className={`h-4 w-4 mr-2 ${loading ? "animate-spin" : ""}`} />
              Refresh
            </Button>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="text-center py-10 text-gray-500">Loading agents…</div>
            ) : (
              <AgentList agents={agents} />
            )}
          </CardContent>
        </Card>
      </div>
    </DashboardLayout>
  )
}
