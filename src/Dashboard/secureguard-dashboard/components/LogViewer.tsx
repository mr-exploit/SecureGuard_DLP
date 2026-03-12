"use client"

import { useState } from "react"
import type { LogEntry, PaginatedResponse } from "@/types"
import { VIOLATION_TYPES, SEVERITY_LEVELS } from "@/types"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { formatDate, getViolationTypeLabel } from "@/lib/utils"
import { logsApi } from "@/lib/api"
import { RefreshCw, ScrollText } from "lucide-react"

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

interface LogViewerProps {
  initialData: PaginatedResponse<LogEntry>
}

export default function LogViewer({ initialData }: LogViewerProps) {
  const [data, setData] = useState(initialData)
  const [loading, setLoading] = useState(false)
  const [filters, setFilters] = useState({
    violationType: "",
    severity: "",
    agentId: "",
    page: 1,
    pageSize: 20,
  })

  const fetchLogs = async (newFilters = filters) => {
    setLoading(true)
    try {
      const result = await logsApi.getAll({
        ...newFilters,
        violationType: newFilters.violationType || undefined,
        severity: newFilters.severity || undefined,
        agentId: newFilters.agentId || undefined,
      })
      setData(result)
    } catch (err) {
      console.error("Failed to fetch logs", err)
    } finally {
      setLoading(false)
    }
  }

  const handleFilterChange = (key: string, value: string) => {
    const updated = { ...filters, [key]: value, page: 1 }
    setFilters(updated)
    fetchLogs(updated)
  }

  const handlePageChange = (newPage: number) => {
    const updated = { ...filters, page: newPage }
    setFilters(updated)
    fetchLogs(updated)
  }

  return (
    <div className="space-y-4">
      {/* Filters */}
      <div className="flex gap-3 flex-wrap items-center">
        <Input
          className="flex-1 min-w-48 max-w-xs"
          placeholder="Filter by Agent ID…"
          value={filters.agentId}
          onChange={(e) => handleFilterChange("agentId", e.target.value)}
        />

        <Select
          value={filters.violationType || "all"}
          onValueChange={(v) => handleFilterChange("violationType", v === "all" ? "" : v)}
        >
          <SelectTrigger className="w-52">
            <SelectValue placeholder="All Violation Types" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Violation Types</SelectItem>
            {Object.values(VIOLATION_TYPES).map((type) => (
              <SelectItem key={type} value={type}>
                {getViolationTypeLabel(type)}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          value={filters.severity || "all"}
          onValueChange={(v) => handleFilterChange("severity", v === "all" ? "" : v)}
        >
          <SelectTrigger className="w-40">
            <SelectValue placeholder="All Severities" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Severities</SelectItem>
            {Object.values(SEVERITY_LEVELS).map((sev) => (
              <SelectItem key={sev} value={sev}>
                {sev}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Button variant="outline" onClick={() => fetchLogs()} disabled={loading}>
          <RefreshCw className={`h-4 w-4 mr-2 ${loading ? "animate-spin" : ""}`} />
          Refresh
        </Button>
      </div>

      {/* Table */}
      {data.data.length === 0 ? (
        <div className="text-center py-10 text-gray-500">
          <ScrollText className="h-10 w-10 mx-auto mb-2 text-gray-400" />
          <p>No log entries found</p>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Timestamp</TableHead>
              <TableHead>Hostname</TableHead>
              <TableHead>Username</TableHead>
              <TableHead>Process</TableHead>
              <TableHead>Violation</TableHead>
              <TableHead>Severity</TableHead>
              <TableHead>Details</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.data.map((log) => (
              <TableRow key={log.id}>
                <TableCell className="text-sm text-gray-500 whitespace-nowrap">
                  {formatDate(log.timestamp)}
                </TableCell>
                <TableCell className="font-medium">{log.hostname}</TableCell>
                <TableCell className="text-gray-600">{log.username}</TableCell>
                <TableCell className="font-mono text-sm text-gray-600">{log.processName}</TableCell>
                <TableCell className="text-sm">{getViolationTypeLabel(log.violationType)}</TableCell>
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
      )}

      {/* Pagination */}
      <div className="flex items-center justify-between pt-2">
        <p className="text-sm text-gray-500">
          {data.total === 0
            ? "No entries"
            : `Showing ${(filters.page - 1) * filters.pageSize + 1}–${Math.min(
                filters.page * filters.pageSize,
                data.total
              )} of ${data.total} entries`}
        </p>
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            disabled={filters.page <= 1}
            onClick={() => handlePageChange(filters.page - 1)}
          >
            Previous
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={filters.page >= data.totalPages}
            onClick={() => handlePageChange(filters.page + 1)}
          >
            Next
          </Button>
        </div>
      </div>
    </div>
  )
}
