'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import StatsCard from '@/components/StatsCard'
import IncidentTable from '@/components/IncidentTable'
import { api } from '@/lib/api'
import { startSignalR, stopSignalR } from '@/lib/signalr'
import { DashboardStats, Alert } from '@/types'
import { Monitor, AlertTriangle, Bell, Shield } from 'lucide-react'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { BarChart, Bar, XAxis, YAxis, Tooltip, ResponsiveContainer, Cell } from 'recharts'

export default function DashboardPage() {
  const router = useRouter()
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [liveAlerts, setLiveAlerts] = useState<Alert[]>([])

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (!token) { router.push('/login'); return }

    loadStats()
    startSignalR((alert: Alert) => {
      setLiveAlerts(prev => [alert, ...prev].slice(0, 20))
    })

    const interval = setInterval(loadStats, 30000)
    return () => {
      clearInterval(interval)
      stopSignalR()
    }
  }, [router])

  const loadStats = async () => {
    try {
      const data = await api.getDashboardStats()
      setStats(data)
    } catch (err) {
      console.error('Failed to load stats', err)
    } finally {
      setLoading(false)
    }
  }

  const VIOLATION_COLORS: Record<string, string> = {
    IMAGE_UPLOAD: '#3b82f6',
    CREDENTIAL_FILE: '#ef4444',
    UNKNOWN_IP: '#f59e0b',
    CREDENTIAL_PATTERN: '#8b5cf6',
    SENSITIVE_FILE_ACCESS: '#ec4899',
  }

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title="Dashboard" alertCount={stats?.activeAlerts ?? 0} />
        <main className="flex-1 p-6 space-y-6">
          {loading ? (
            <div className="text-center py-20 text-muted-foreground">Loading dashboard...</div>
          ) : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                <StatsCard title="Total Agents" value={stats?.totalAgents ?? 0} icon={Monitor} color="text-blue-600" description={`${stats?.onlineAgents ?? 0} online`} />
                <StatsCard title="Incidents Today" value={stats?.incidentsToday ?? 0} icon={AlertTriangle} color="text-orange-600" />
                <StatsCard title="Active Alerts" value={stats?.activeAlerts ?? 0} icon={Bell} color="text-red-600" />
                <StatsCard title="Critical Alerts" value={stats?.criticalAlerts ?? 0} icon={Shield} color="text-purple-600" />
              </div>

              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <Card>
                  <CardHeader><CardTitle className="text-base">Incidents (Last 7 Days)</CardTitle></CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={200}>
                      <BarChart data={stats?.incidentTrend ?? []}>
                        <XAxis dataKey="date" tick={{ fontSize: 11 }} />
                        <YAxis tick={{ fontSize: 11 }} />
                        <Tooltip />
                        <Bar dataKey="count" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                      </BarChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>

                <Card>
                  <CardHeader><CardTitle className="text-base">Violation Breakdown (Today)</CardTitle></CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={200}>
                      <BarChart data={stats?.violationBreakdown ?? []} layout="vertical">
                        <XAxis type="number" tick={{ fontSize: 11 }} />
                        <YAxis dataKey="violationType" type="category" tick={{ fontSize: 10 }} width={120} />
                        <Tooltip />
                        <Bar dataKey="count" radius={[0, 4, 4, 0]}>
                          {(stats?.violationBreakdown ?? []).map((entry, index) => (
                            <Cell key={index} fill={VIOLATION_COLORS[entry.violationType] ?? '#6b7280'} />
                          ))}
                        </Bar>
                      </BarChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>
              </div>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Recent Alerts</CardTitle>
                </CardHeader>
                <CardContent>
                  <IncidentTable incidents={[...liveAlerts, ...(stats?.recentAlerts ?? [])].slice(0, 10)} />
                </CardContent>
              </Card>
            </>
          )}
        </main>
      </div>
    </div>
  )
}
