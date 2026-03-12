"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import type { Policy } from "@/types"
import { policiesApi } from "@/lib/api"
import DashboardLayout from "@/components/DashboardLayout"
import PolicyForm from "@/components/PolicyForm"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Switch } from "@/components/ui/switch"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Plus, Pencil, Trash2, RefreshCw, Shield } from "lucide-react"
import { formatDate } from "@/lib/utils"

type ActionVariant = "danger" | "warning" | "info"

function getActionVariant(action: string): ActionVariant {
  switch (action) {
    case "BLOCK":
      return "danger"
    case "ALERT":
      return "warning"
    default:
      return "info"
  }
}

export default function PoliciesPage() {
  const router = useRouter()
  const [policies, setPolicies] = useState<Policy[]>([])
  const [loading, setLoading] = useState(true)
  const [formOpen, setFormOpen] = useState(false)
  const [editingPolicy, setEditingPolicy] = useState<Policy | undefined>()
  const [deleting, setDeleting] = useState<string | null>(null)
  const [toggling, setToggling] = useState<string | null>(null)

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.replace("/login")
      return
    }
    fetchPolicies()
  }, [router])

  const fetchPolicies = async () => {
    setLoading(true)
    try {
      const data = await policiesApi.getAll()
      setPolicies(data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm("Are you sure you want to delete this policy?")) return
    setDeleting(id)
    try {
      await policiesApi.delete(id)
      await fetchPolicies()
    } catch (err) {
      console.error(err)
    } finally {
      setDeleting(null)
    }
  }

  const handleToggle = async (policy: Policy) => {
    setToggling(policy.id)
    try {
      await policiesApi.update(policy.id, { isEnabled: !policy.isEnabled })
      await fetchPolicies()
    } catch (err) {
      console.error(err)
    } finally {
      setToggling(null)
    }
  }

  const handleEdit = (policy: Policy) => {
    setEditingPolicy(policy)
    setFormOpen(true)
  }

  const handleCreateNew = () => {
    setEditingPolicy(undefined)
    setFormOpen(true)
  }

  const enabledCount = policies.filter((p) => p.isEnabled).length

  return (
    <DashboardLayout title="Policies">
      <div className="space-y-4">
        {/* Summary */}
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <Card>
            <CardContent className="p-4 flex items-center gap-3">
              <div className="p-2 rounded-full bg-blue-50">
                <Shield className="h-5 w-5 text-blue-600" />
              </div>
              <div>
                <p className="text-sm text-gray-500">Total Policies</p>
                <p className="text-2xl font-bold">{policies.length}</p>
              </div>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <p className="text-sm text-gray-500">Enabled</p>
              <p className="text-2xl font-bold text-green-600">{enabledCount}</p>
            </CardContent>
          </Card>
          <Card>
            <CardContent className="p-4">
              <p className="text-sm text-gray-500">Disabled</p>
              <p className="text-2xl font-bold text-gray-500">{policies.length - enabledCount}</p>
            </CardContent>
          </Card>
        </div>

        {/* Policies Table */}
        <Card>
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle>DLP Policies</CardTitle>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={fetchPolicies} disabled={loading}>
                <RefreshCw className={`h-4 w-4 ${loading ? "animate-spin" : ""}`} />
              </Button>
              <Button size="sm" onClick={handleCreateNew}>
                <Plus className="h-4 w-4 mr-2" />
                New Policy
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            {loading ? (
              <div className="text-center py-10 text-gray-500">Loading policies…</div>
            ) : policies.length === 0 ? (
              <div className="text-center py-10 text-gray-500">
                <Shield className="h-10 w-10 mx-auto mb-2 text-gray-300" />
                <p>No policies configured</p>
                <Button className="mt-3" size="sm" onClick={handleCreateNew}>
                  Create your first policy
                </Button>
              </div>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Rule Type</TableHead>
                    <TableHead>Action</TableHead>
                    <TableHead>Enabled</TableHead>
                    <TableHead>Last Updated</TableHead>
                    <TableHead className="text-right">Actions</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {policies.map((policy) => (
                    <TableRow key={policy.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{policy.name}</p>
                          {policy.description && (
                            <p className="text-xs text-gray-500 mt-0.5">{policy.description}</p>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge variant="secondary" className="text-xs">
                          {policy.ruleType}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <Badge variant={getActionVariant(policy.action)}>{policy.action}</Badge>
                      </TableCell>
                      <TableCell>
                        <Switch
                          checked={policy.isEnabled}
                          onCheckedChange={() => handleToggle(policy)}
                          disabled={toggling === policy.id}
                        />
                      </TableCell>
                      <TableCell className="text-gray-500 text-sm">
                        {formatDate(policy.updatedAt)}
                      </TableCell>
                      <TableCell className="text-right">
                        <div className="flex justify-end gap-1">
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleEdit(policy)}
                            title="Edit policy"
                          >
                            <Pencil className="h-4 w-4 text-gray-500" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="icon"
                            onClick={() => handleDelete(policy.id)}
                            disabled={deleting === policy.id}
                            className="text-red-500 hover:text-red-700 hover:bg-red-50"
                            title="Delete policy"
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>
      </div>

      <PolicyForm
        policy={editingPolicy}
        open={formOpen}
        onClose={() => {
          setFormOpen(false)
          setEditingPolicy(undefined)
        }}
        onSaved={fetchPolicies}
      />
    </DashboardLayout>
  )
}
