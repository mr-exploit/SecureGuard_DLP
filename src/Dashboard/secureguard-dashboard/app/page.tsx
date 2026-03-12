"use client"

import { useEffect, useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import type { DashboardStats, Alert } from "@/types"
import { dashboardApi } from "@/lib/api"
import { startSignalR, stopSignalR } from "@/lib/signalr"
import DashboardLayout from "@/components/DashboardLayout"
import StatsCard from "@/components/StatsCard"
import IncidentTable from "@/components/IncidentTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Monitor, Wifi, AlertTriangle, Bell } from "lucide-react"
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts"

export default function DashboardPage() {
  const router = useRouter()
  const [stats, setStats] = useState<DashboardStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [realtimeAlerts, setRealtimeAlerts] = useState<Alert[]>([])

  const fetchStats = useCallback(async () => {
    try {
      const data = await dashboardApi.getStats()
      setStats(data)
    } catch (err) {
      console.error("Failed to fetch stats", err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    const token = localStorage.getItem("token")
    if (!token) {
      router.replace("/login")
      return
    }

    fetchStats()

    let mounted = true
    const initSignalR = async () => {
      try {
        const conn = await startSignalR()
        conn.on("NewAlert", (alert: Alert) => {
          if (!mounted) return
          setRealtimeAlerts((prev) => [alert, ...prev].slice(0, 10))
          fetchStats()
        })
      } catch (err) {
        console.error("SignalR connection failed", err)
      }
    }
    initSignalR()

    return () => {
      mounted = false
      stopSignalR()
    }
  }, [router, fetchStats])

  const combinedAlerts = [...realtimeAlerts, ...(stats?.recentAlerts ?? [])].slice(0, 10)

  const chartData =
    stats?.dailyStats.map((d) => ({
      date: new Date(d.date).toLocaleDateString("en-US", { month: "short", day: "numeric" }),
      violations: d.count,
    })) ?? []

  if (loading) {
    return (
      <DashboardLayout title="Dashboard">
        <div className="flex items-center justify-center h-64">
          <div className="text-gray-500">Loading dashboard…</div>
        </div>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout title="Dashboard">
      <div className="space-y-6">
        {/* Stats Cards */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
          <StatsCard
            title="Total Agents"
            value={stats?.totalAgents ?? 0}
            icon={Monitor}
            colorClass="text-blue-600 bg-blue-50"
            description={`${stats?.offlineAgents ?? 0} offline`}
          />
          <StatsCard
            title="Online Agents"
            value={stats?.onlineAgents ?? 0}
            icon={Wifi}
            colorClass="text-green-600 bg-green-50"
            description="Currently connected"
          />
          <StatsCard
            title="Incidents Today"
            value={stats?.incidentsToday ?? 0}
            icon={AlertTriangle}
            colorClass="text-orange-600 bg-orange-50"
            description={`${stats?.logsToday ?? 0} log entries today`}
          />
          <StatsCard
            title="Active Alerts"
            value={stats?.activeAlerts ?? 0}
            icon={Bell}
            colorClass="text-red-600 bg-red-50"
            description="Awaiting resolution"
          />
        </div>

        {/* Chart + Breakdown */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          <Card className="lg:col-span-2">
            <CardHeader>
              <CardTitle>Daily Violations — Last 7 Days</CardTitle>
            </CardHeader>
            <CardContent>
              {chartData.length > 0 ? (
                <ResponsiveContainer width="100%" height={260}>
                  <BarChart data={chartData} margin={{ top: 4, right: 4, left: -16, bottom: 0 }}>
                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                    <XAxis dataKey="date" tick={{ fontSize: 12 }} />
                    <YAxis tick={{ fontSize: 12 }} allowDecimals={false} />
                    <Tooltip
                      contentStyle={{ fontSize: 12, borderRadius: 8 }}
                      formatter={(v) => [v, "Violations"]}
                    />
                    <Bar dataKey="violations" fill="#3b82f6" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              ) : (
                <div className="flex items-center justify-center h-64 text-gray-400 text-sm">
                  No daily stats available
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Severity Breakdown</CardTitle>
            </CardHeader>
            <CardContent>
              {stats?.severityBreakdown && stats.severityBreakdown.length > 0 ? (
                <div className="space-y-3 pt-2">
                  {stats.severityBreakdown.map((item) => {
                    const colors: Record<string, string> = {
                      CRITICAL: "bg-red-500",
                      HIGH: "bg-orange-500",
                      MEDIUM: "bg-yellow-500",
                      LOW: "bg-blue-400",
                    }
                    const total = stats.severityBreakdown.reduce((s, x) => s + x.count, 0)
                    const pct = total > 0 ? Math.round((item.count / total) * 100) : 0
                    return (
                      <div key={item.severity}>
                        <div className="flex items-center justify-between mb-1">
                          <span className="text-sm text-gray-600">{item.severity}</span>
                          <span className="text-sm font-semibold">{item.count}</span>
                        </div>
                        <div className="h-2 bg-gray-100 rounded-full overflow-hidden">
                          <div
                            className={`h-full rounded-full ${colors[item.severity] ?? "bg-gray-400"}`}
                            style={{ width: `${pct}%` }}
                          />
                        </div>
                      </div>
                    )
                  })}
                </div>
              ) : (
                <div className="text-center py-8 text-gray-400 text-sm">No data available</div>
              )}
            </CardContent>
          </Card>
        </div>

        {/* Violation Type Breakdown */}
        {stats?.violationBreakdown && stats.violationBreakdown.length > 0 && (
          <Card>
            <CardHeader>
              <CardTitle>Violation Types</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
                {stats.violationBreakdown.map((item) => (
                  <div
                    key={item.violationType}
                    className="text-center p-3 bg-gray-50 rounded-lg border"
                  >
                    <p className="text-2xl font-bold text-gray-900">{item.count}</p>
                    <p className="text-xs text-gray-500 mt-1 leading-tight">
                      {item.violationType.replace(/_/g, " ")}
                    </p>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        {/* Recent Alerts */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Alerts</CardTitle>
          </CardHeader>
          <CardContent>
            <IncidentTable alerts={combinedAlerts} onResolved={fetchStats} />
          </CardContent>
        </Card>
      </div>
    </DashboardLayout>
  )
}
