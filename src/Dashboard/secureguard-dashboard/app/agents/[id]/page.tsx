"use client"

import { useEffect, useState } from "react"
import { useRouter, useParams } from "next/navigation"
import type { Agent, LogEntry, PaginatedResponse } from "@/types"
import { agentsApi, logsApi } from "@/lib/api"
import DashboardLayout from "@/components/DashboardLayout"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { formatDate, formatRelativeTime, getViolationTypeLabel } from "@/lib/utils"
import { ArrowLeft, Wifi, WifiOff, Monitor, Clock, Hash } from "lucide-react"

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-center justify-between py-2.5 border-b border-gray-100 last:border-0">
      <span className="text-sm text-gray-500">{label}</span>
      <span className="text-sm font-medium text-gray-900 text-right max-w-xs truncate">
        {value}
      </span>
    </div>
  )
}

type SeverityVariant = "danger" | "warning" | "info" | "secondary"

function getSeverityVariant(severity: string): SeverityVariant {
  switch (severity.toUpperCase()) {
    case "CRITICAL":
    case "HIGH":
      return "danger"
    case "MEDIUM":
      return "warning"
    case "LOW":
      return "info"
    default:
      return "secondary"
  }
}

export default function AgentDetailPage() {
  const router = useRouter()
  const params = useParams()
  const id = params.id as string

  const [agent, setAgent] = useState<Agent | null>(null)
  const [logs, setLogs] = useState<PaginatedResponse<LogEntry> | null>(null)
  const [loading, setLoading] = useState(true)
  const [notFound, setNotFound] = useState(false)

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.replace("/login")
      return
    }
    fetchData()
  }, [id, router])

  const fetchData = async () => {
    setLoading(true)
    try {
      const [agentData, logsData] = await Promise.all([
        agentsApi.getById(id),
        logsApi.getAll({ agentId: id, pageSize: 15 }),
      ])
      setAgent(agentData)
      setLogs(logsData)
    } catch (err) {
      console.error(err)
      setNotFound(true)
    } finally {
      setLoading(false)
    }
  }

  if (loading) {
    return (
      <DashboardLayout title="Agent Details">
        <div className="text-center py-20 text-gray-500">Loading agent details…</div>
      </DashboardLayout>
    )
  }

  if (notFound || !agent) {
    return (
      <DashboardLayout title="Agent Details">
        <div className="text-center py-20 text-gray-500">
          <Monitor className="h-12 w-12 mx-auto mb-3 text-gray-300" />
          <p className="font-medium">Agent not found</p>
          <Button variant="outline" className="mt-4" onClick={() => router.push("/agents")}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Agents
          </Button>
        </div>
      </DashboardLayout>
    )
  }

  return (
    <DashboardLayout title={`Agent: ${agent.hostname}`}>
      <div className="space-y-6">
        <Button variant="outline" size="sm" onClick={() => router.push("/agents")}>
          <ArrowLeft className="h-4 w-4 mr-2" />
          Back to Agents
        </Button>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Agent Info */}
          <Card>
            <CardHeader>
              <div className="flex items-center gap-3">
                <div className="p-2 rounded-full bg-blue-50">
                  <Monitor className="h-5 w-5 text-blue-600" />
                </div>
                <CardTitle>{agent.hostname}</CardTitle>
                {agent.isOnline ? (
                  <Badge variant="success" className="ml-auto">
                    Online
                  </Badge>
                ) : (
                  <Badge variant="secondary" className="ml-auto">
                    Offline
                  </Badge>
                )}
              </div>
            </CardHeader>
            <CardContent>
              <InfoRow label="Agent ID" value={agent.agentId} />
              <InfoRow label="Username" value={agent.username} />
              <InfoRow label="IP Address" value={agent.ipAddress} />
              <InfoRow label="OS Version" value={agent.osVersion} />
              <InfoRow label="Agent Version" value={agent.agentVersion} />
              <InfoRow label="Last Seen" value={formatRelativeTime(agent.lastSeen)} />
              <InfoRow label="Registered At" value={formatDate(agent.registeredAt)} />
            </CardContent>
          </Card>

          {/* Connection Status */}
          <Card>
            <CardHeader>
              <CardTitle>Connection Status</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex items-center gap-4 py-4">
                {agent.isOnline ? (
                  <div className="p-4 rounded-full bg-green-100">
                    <Wifi className="h-8 w-8 text-green-600" />
                  </div>
                ) : (
                  <div className="p-4 rounded-full bg-gray-100">
                    <WifiOff className="h-8 w-8 text-gray-400" />
                  </div>
                )}
                <div>
                  <p className={`font-semibold ${agent.isOnline ? "text-green-700" : "text-gray-600"}`}>
                    {agent.isOnline ? "Connected" : "Disconnected"}
                  </p>
                  <p className="text-sm text-gray-500">
                    Last seen {formatRelativeTime(agent.lastSeen)}
                  </p>
                </div>
              </div>

              <div className="mt-4 space-y-3 border-t pt-4">
                <div className="flex items-center gap-3 text-sm text-gray-600">
                  <Hash className="h-4 w-4 text-gray-400" />
                  <span className="font-mono">{agent.agentId}</span>
                </div>
                <div className="flex items-center gap-3 text-sm text-gray-600">
                  <Clock className="h-4 w-4 text-gray-400" />
                  <span>Registered {formatDate(agent.registeredAt)}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Recent Logs */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Activity Logs</CardTitle>
          </CardHeader>
          <CardContent>
            {logs && logs.data.length > 0 ? (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Timestamp</TableHead>
                    <TableHead>Process</TableHead>
                    <TableHead>Violation Type</TableHead>
                    <TableHead>Severity</TableHead>
                    <TableHead>Details</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {logs.data.map((log) => (
                    <TableRow key={log.id}>
                      <TableCell className="text-sm text-gray-500 whitespace-nowrap">
                        {formatDate(log.timestamp)}
                      </TableCell>
                      <TableCell className="font-mono text-sm">{log.processName}</TableCell>
                      <TableCell className="text-sm">
                        {getViolationTypeLabel(log.violationType)}
                      </TableCell>
                      <TableCell>
                        <Badge variant={getSeverityVariant(log.severity)}>{log.severity}</Badge>
                      </TableCell>
                      <TableCell className="max-w-xs truncate text-gray-500 text-sm">
                        {log.details}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <div className="text-center py-8 text-gray-500">
                No recent logs for this agent
              </div>
            )}
          </CardContent>
        </Card>
      </div>
    </DashboardLayout>
  )
}
