'use client'

import { Alert } from '@/types'
import { Badge } from '@/components/ui/badge'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate, getSeverityColor } from '@/lib/utils'
import { Button } from '@/components/ui/button'

interface IncidentTableProps {
  incidents: Alert[]
  onAcknowledge?: (id: number) => void
  loading?: boolean
}

export default function IncidentTable({ incidents, onAcknowledge, loading }: IncidentTableProps) {
  if (loading) {
    return <div className="text-center py-8 text-muted-foreground">Loading incidents...</div>
  }

  if (incidents.length === 0) {
    return <div className="text-center py-8 text-muted-foreground">No incidents found.</div>
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Time</TableHead>
          <TableHead>Hostname</TableHead>
          <TableHead>Violation Type</TableHead>
          <TableHead>Severity</TableHead>
          <TableHead>Message</TableHead>
          <TableHead>Status</TableHead>
          {onAcknowledge && <TableHead>Action</TableHead>}
        </TableRow>
      </TableHeader>
      <TableBody>
        {incidents.map((incident, i) => (
          <TableRow key={incident.id ?? i}>
            <TableCell className="text-xs text-muted-foreground">{formatDate(incident.timestamp)}</TableCell>
            <TableCell className="font-medium">{incident.hostname}</TableCell>
            <TableCell>
              <span className="text-xs font-mono bg-gray-100 px-2 py-0.5 rounded">{incident.violationType}</span>
            </TableCell>
            <TableCell>
              <Badge className={getSeverityColor(incident.severity)}>{incident.severity}</Badge>
            </TableCell>
            <TableCell className="max-w-xs truncate text-sm">{incident.message}</TableCell>
            <TableCell>
              {incident.acknowledged ? (
                <Badge variant="secondary">Acknowledged</Badge>
              ) : (
                <Badge variant="destructive">Active</Badge>
              )}
            </TableCell>
            {onAcknowledge && incident.id && !incident.acknowledged && (
              <TableCell>
                <Button size="sm" variant="outline" onClick={() => onAcknowledge(incident.id!)}>
                  Acknowledge
                </Button>
              </TableCell>
            )}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
