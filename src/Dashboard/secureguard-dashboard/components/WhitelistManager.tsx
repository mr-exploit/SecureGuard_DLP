"use client"

import { useState } from "react"
import type { WhitelistEntry } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { whitelistApi } from "@/lib/api"
import { formatDate } from "@/lib/utils"
import { Plus, Trash2, Shield } from "lucide-react"

interface WhitelistManagerProps {
  entries: WhitelistEntry[]
  onChanged: () => void
}

export default function WhitelistManager({ entries, onChanged }: WhitelistManagerProps) {
  const [open, setOpen] = useState(false)
  const [deleting, setDeleting] = useState<string | null>(null)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState("")
  const [form, setForm] = useState({ ipAddress: "", description: "" })

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")
    setSaving(true)
    try {
      await whitelistApi.create(form)
      setForm({ ipAddress: "", description: "" })
      setOpen(false)
      onChanged()
    } catch (err) {
      setError("Failed to add entry. Check the IP address format.")
      console.error("Failed to add whitelist entry", err)
    } finally {
      setSaving(false)
    }
  }

  const handleDelete = async (id: string) => {
    if (!confirm("Remove this IP from the whitelist?")) return
    setDeleting(id)
    try {
      await whitelistApi.delete(id)
      onChanged()
    } catch (err) {
      console.error("Failed to delete whitelist entry", err)
    } finally {
      setDeleting(null)
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={() => { setError(""); setOpen(true) }}>
          <Plus className="h-4 w-4 mr-2" />
          Add IP Address
        </Button>
      </div>

      {entries.length === 0 ? (
        <div className="text-center py-10 text-gray-500">
          <Shield className="h-10 w-10 mx-auto mb-2 text-gray-400" />
          <p>No whitelisted IP addresses</p>
          <p className="text-sm mt-1">Add trusted IPs to exclude them from DLP monitoring.</p>
        </div>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>IP Address</TableHead>
              <TableHead>Description</TableHead>
              <TableHead>Created By</TableHead>
              <TableHead>Created At</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {entries.map((entry) => (
              <TableRow key={entry.id}>
                <TableCell className="font-mono font-medium">{entry.ipAddress}</TableCell>
                <TableCell className="text-gray-600">{entry.description || "—"}</TableCell>
                <TableCell className="text-gray-600">{entry.createdBy}</TableCell>
                <TableCell className="text-gray-500 text-sm">{formatDate(entry.createdAt)}</TableCell>
                <TableCell className="text-right">
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => handleDelete(entry.id)}
                    disabled={deleting === entry.id}
                    className="text-red-500 hover:text-red-700 hover:bg-red-50"
                    title="Remove from whitelist"
                  >
                    <Trash2 className="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add IP to Whitelist</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleAdd} className="space-y-4">
            {error && (
              <div className="p-3 rounded-lg bg-red-50 text-red-700 text-sm">{error}</div>
            )}
            <div className="space-y-2">
              <Label htmlFor="wl-ip">IP Address</Label>
              <Input
                id="wl-ip"
                value={form.ipAddress}
                onChange={(e) => setForm({ ...form, ipAddress: e.target.value })}
                placeholder="192.168.1.100"
                required
                className="font-mono"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="wl-desc">Description</Label>
              <Input
                id="wl-desc"
                value={form.description}
                onChange={(e) => setForm({ ...form, description: e.target.value })}
                placeholder="e.g. Corporate printer, NAT gateway…"
              />
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setOpen(false)}>
                Cancel
              </Button>
              <Button type="submit" disabled={saving}>
                {saving ? "Adding…" : "Add to Whitelist"}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </div>
  )
}
