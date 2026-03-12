"use client"

import { useRouter } from "next/navigation"
import type { Agent } from "@/types"
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
import { formatRelativeTime } from "@/lib/utils"
import { Monitor, Wifi, WifiOff } from "lucide-react"

interface AgentListProps {
  agents: Agent[]
}

export default function AgentList({ agents }: AgentListProps) {
  const router = useRouter()

  if (agents.length === 0) {
    return (
      <div className="text-center py-10 text-gray-500">
        <Monitor className="h-10 w-10 mx-auto mb-2 text-gray-400" />
        <p>No agents registered</p>
      </div>
    )
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Status</TableHead>
          <TableHead>Hostname</TableHead>
          <TableHead>Username</TableHead>
          <TableHead>IP Address</TableHead>
          <TableHead>OS Version</TableHead>
          <TableHead>Agent Version</TableHead>
          <TableHead>Last Seen</TableHead>
          <TableHead className="text-right">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {agents.map((agent) => (
          <TableRow key={agent.id}>
            <TableCell>
              {agent.isOnline ? (
                <div className="flex items-center gap-1.5">
                  <Wifi className="h-4 w-4 text-green-500" />
                  <Badge variant="success">Online</Badge>
                </div>
              ) : (
                <div className="flex items-center gap-1.5">
                  <WifiOff className="h-4 w-4 text-gray-400" />
                  <Badge variant="secondary">Offline</Badge>
                </div>
              )}
            </TableCell>
            <TableCell className="font-medium">{agent.hostname}</TableCell>
            <TableCell className="text-gray-600">{agent.username}</TableCell>
            <TableCell className="font-mono text-sm text-gray-600">{agent.ipAddress}</TableCell>
            <TableCell className="text-sm text-gray-600">{agent.osVersion}</TableCell>
            <TableCell className="text-sm text-gray-600">{agent.agentVersion}</TableCell>
            <TableCell className="text-sm text-gray-500 whitespace-nowrap">
              {formatRelativeTime(agent.lastSeen)}
            </TableCell>
            <TableCell className="text-right">
              <Button
                size="sm"
                variant="outline"
                onClick={() => router.push(`/agents/${agent.id}`)}
              >
                View Details
              </Button>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
