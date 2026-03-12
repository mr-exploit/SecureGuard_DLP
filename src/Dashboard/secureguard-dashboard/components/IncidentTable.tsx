"use client"

import { useState } from "react"
import type { Alert } from "@/types"
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
import { formatRelativeTime, getViolationTypeLabel } from "@/lib/utils"
import { alertsApi } from "@/lib/api"
import { CheckCircle } from "lucide-react"

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

interface IncidentTableProps {
  alerts: Alert[]
  onResolved?: () => void
}

export default function IncidentTable({ alerts, onResolved }: IncidentTableProps) {
  const [resolving, setResolving] = useState<string | null>(null)

  const handleResolve = async (id: string) => {
    setResolving(id)
    try {
      await alertsApi.resolve(id)
      onResolved?.()
    } catch (err) {
      console.error("Failed to resolve alert", err)
    } finally {
      setResolving(null)
    }
  }

  if (alerts.length === 0) {
    return (
      <div className="text-center py-10 text-gray-500">
        <CheckCircle className="h-10 w-10 mx-auto mb-2 text-green-400" />
        <p>No incidents found</p>
      </div>
    )
  }

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Violation Type</TableHead>
          <TableHead>Severity</TableHead>
          <TableHead>Hostname</TableHead>
          <TableHead>Message</TableHead>
          <TableHead>Time</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="text-right">Action</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {alerts.map((alert) => (
          <TableRow key={alert.id}>
            <TableCell className="font-medium">
              {getViolationTypeLabel(alert.violationType)}
            </TableCell>
            <TableCell>
              <Badge variant={getSeverityVariant(alert.severity)}>{alert.severity}</Badge>
            </TableCell>
            <TableCell className="text-gray-600">{alert.hostname}</TableCell>
            <TableCell className="max-w-xs truncate text-gray-600 text-sm">
              {alert.message}
            </TableCell>
            <TableCell className="text-gray-500 whitespace-nowrap text-sm">
              {formatRelativeTime(alert.timestamp)}
            </TableCell>
            <TableCell>
              {alert.isResolved ? (
                <Badge variant="success">Resolved</Badge>
              ) : (
                <Badge variant="danger">Active</Badge>
              )}
            </TableCell>
            <TableCell className="text-right">
              {!alert.isResolved && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => handleResolve(alert.id)}
                  disabled={resolving === alert.id}
                >
                  {resolving === alert.id ? "Resolving…" : "Resolve"}
                </Button>
              )}
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  )
}
