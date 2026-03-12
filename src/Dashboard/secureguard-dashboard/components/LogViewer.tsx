'use client'

import { LogEntry } from '@/types'
import { Badge } from '@/components/ui/badge'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate, getSeverityColor } from '@/lib/utils'

interface LogViewerProps {
  logs: LogEntry[]
  loading?: boolean
}

export default function LogViewer({ logs, loading }: LogViewerProps) {
  if (loading) {
    return <div className="text-center py-8 text-muted-foreground">Loading logs...</div>
  }

  if (logs.length === 0) {
    return <div className="text-center py-8 text-muted-foreground">No logs found.</div>
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Timestamp</TableHead>
          <TableHead>Hostname</TableHead>
          <TableHead>Username</TableHead>
          <TableHead>Process</TableHead>
          <TableHead>Dest IP</TableHead>
          <TableHead>Violation</TableHead>
          <TableHead>Severity</TableHead>
          <TableHead>Details</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {logs.map((log, i) => (
          <TableRow key={i}>
            <TableCell className="text-xs text-muted-foreground whitespace-nowrap">{formatDate(log.timestamp)}</TableCell>
            <TableCell className="font-medium text-sm">{log.hostname}</TableCell>
            <TableCell className="text-sm">{log.username}</TableCell>
            <TableCell className="text-xs font-mono">{log.processName || '-'}</TableCell>
            <TableCell className="text-xs font-mono">{log.destinationIp || '-'}</TableCell>
            <TableCell>
              <span className="text-xs font-mono bg-gray-100 px-1.5 py-0.5 rounded">{log.violationType}</span>
            </TableCell>
            <TableCell>
              <Badge className={getSeverityColor(log.severity)}>{log.severity}</Badge>
            </TableCell>
            <TableCell className="text-xs max-w-xs truncate">{log.details}</TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
