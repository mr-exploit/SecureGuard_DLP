'use client'

import { Agent } from '@/types'
import { Badge } from '@/components/ui/badge'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate, getStatusColor } from '@/lib/utils'
import Link from 'next/link'

interface AgentListProps {
  agents: Agent[]
  loading?: boolean
}

export default function AgentList({ agents, loading }: AgentListProps) {
  if (loading) {
    return <div className="text-center py-8 text-muted-foreground">Loading agents...</div>
  }

  if (agents.length === 0) {
    return <div className="text-center py-8 text-muted-foreground">No agents registered.</div>
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Hostname</TableHead>
          <TableHead>IP Address</TableHead>
          <TableHead>Username</TableHead>
          <TableHead>OS</TableHead>
          <TableHead>Version</TableHead>
          <TableHead>Status</TableHead>
          <TableHead>Last Seen</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {agents.map((agent) => (
          <TableRow key={agent.id}>
            <TableCell>
              <Link href={`/agents/${agent.id}`} className="font-medium text-blue-600 hover:underline">
                {agent.hostname}
              </Link>
            </TableCell>
            <TableCell className="font-mono text-xs">{agent.ipAddress || '-'}</TableCell>
            <TableCell>{agent.username || '-'}</TableCell>
            <TableCell className="text-xs">{agent.os || 'Windows'}</TableCell>
            <TableCell className="text-xs">{agent.version || '-'}</TableCell>
            <TableCell>
              <Badge className={getStatusColor(agent.status)}>
                <span className={`mr-1.5 h-1.5 w-1.5 rounded-full inline-block ${agent.status === 'online' ? 'bg-green-500' : 'bg-gray-400'}`} />
                {agent.status}
              </Badge>
            </TableCell>
            <TableCell className="text-xs text-muted-foreground">{formatDate(agent.lastSeen)}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
