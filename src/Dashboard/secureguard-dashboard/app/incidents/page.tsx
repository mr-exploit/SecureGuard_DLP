"use client"

import { useEffect, useState, useCallback } from "react"
import { useRouter } from "next/navigation"
import type { Alert, PaginatedResponse } from "@/types"
import { SEVERITY_LEVELS } from "@/types"
import { alertsApi } from "@/lib/api"
import DashboardLayout from "@/components/DashboardLayout"
import IncidentTable from "@/components/IncidentTable"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { RefreshCw } from "lucide-react"

export default function IncidentsPage() {
  const router = useRouter()
  const [data, setData] = useState<PaginatedResponse<Alert> | null>(null)
  const [loading, setLoading] = useState(true)
  const [filters, setFilters] = useState({
    page: 1,
    pageSize: 20,
    isResolved: undefined as boolean | undefined,
    severity: "",
  })

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.replace("/login")
      return
    }
    fetchAlerts(filters)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const fetchAlerts = useCallback(
    async (f = filters) => {
      setLoading(true)
      try {
        const result = await alertsApi.getAll({
          page: f.page,
          pageSize: f.pageSize,
          isResolved: f.isResolved,
          severity: f.severity || undefined,
        })
        setData(result)
      } catch (err) {
        console.error(err)
      } finally {
        setLoading(false)
      }
    },
    [filters]
  )

  const updateFilter = (key: string, value: unknown) => {
    const updated = { ...filters, [key]: value, page: 1 }
    setFilters(updated)
    fetchAlerts(updated as typeof filters)
  }

  const handlePageChange = (newPage: number) => {
    const updated = { ...filters, page: newPage }
    setFilters(updated)
    fetchAlerts(updated)
  }

  return (
    <DashboardLayout title="Incidents">
      <Card>
        <CardHeader className="flex flex-row items-center justify-between flex-wrap gap-3">
          <CardTitle>All Incidents</CardTitle>
          <div className="flex gap-3 flex-wrap">
            <Select
              value={filters.severity || "all"}
              onValueChange={(v) => updateFilter("severity", v === "all" ? "" : v)}
            >
              <SelectTrigger className="w-40">
                <SelectValue placeholder="All Severities" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Severities</SelectItem>
                {Object.values(SEVERITY_LEVELS).map((s) => (
                  <SelectItem key={s} value={s}>
                    {s}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select
              value={
                filters.isResolved === undefined
                  ? "all"
                  : filters.isResolved
                  ? "resolved"
                  : "active"
              }
              onValueChange={(v) =>
                updateFilter("isResolved", v === "all" ? undefined : v === "resolved")
              }
            >
              <SelectTrigger className="w-36">
                <SelectValue placeholder="All Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="resolved">Resolved</SelectItem>
              </SelectContent>
            </Select>

            <Button variant="outline" onClick={() => fetchAlerts()} disabled={loading}>
              <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
            </Button>
          </div>
        </CardHeader>

        <CardContent>
          {loading ? (
            <div className="text-center py-10 text-gray-500">Loading incidents…</div>
          ) : (
            <>
              <IncidentTable alerts={data?.data ?? []} onResolved={() => fetchAlerts()} />

              {data && data.totalPages > 1 && (
                <div className="flex items-center justify-between mt-4 pt-4 border-t">
                  <p className="text-sm text-gray-500">
                    Page {data.page} of {data.totalPages} ({data.total} total)
                  </p>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={data.page <= 1}
                      onClick={() => handlePageChange(data.page - 1)}
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={data.page >= data.totalPages}
                      onClick={() => handlePageChange(data.page + 1)}
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </>
          )}
        </CardContent>
      </Card>
    </DashboardLayout>
  )
}
