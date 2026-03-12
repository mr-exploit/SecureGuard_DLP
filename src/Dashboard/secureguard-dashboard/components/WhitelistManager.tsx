'use client'

import { useState } from 'react'
import { WhitelistEntry } from '@/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'
import { formatDate } from '@/lib/utils'

interface WhitelistManagerProps {
  entries: WhitelistEntry[]
  onRefresh: () => void
}

export default function WhitelistManager({ entries, onRefresh }: WhitelistManagerProps) {
  const [adding, setAdding] = useState(false)
  const [form, setForm] = useState({ ipAddress: '', description: '' })

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await api.addToWhitelist({ ...form, isActive: true })
      setForm({ ipAddress: '', description: '' })
      setAdding(false)
      onRefresh()
    } catch (err: any) {
      alert(err.message || 'Failed to add IP to whitelist.')
    }
  }

  const handleRemove = async (id: number) => {
    if (!confirm('Remove this IP from whitelist?')) return
    try {
      await api.removeFromWhitelist(id)
      onRefresh()
    } catch (err) {
      alert('Failed to remove IP.')
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={() => setAdding(!adding)}>
          {adding ? 'Cancel' : '+ Add IP'}
        </Button>
      </div>

      {adding && (
        <form onSubmit={handleAdd} className="bg-gray-50 rounded-lg p-4 flex gap-3 items-end">
          <div className="space-y-1 flex-1">
            <label className="text-xs font-medium">IP Address</label>
            <Input
              placeholder="e.g. 192.168.1.100"
              value={form.ipAddress}
              onChange={e => setForm({ ...form, ipAddress: e.target.value })}
              required
            />
          </div>
          <div className="space-y-1 flex-1">
            <label className="text-xs font-medium">Description</label>
            <Input
              placeholder="Description"
              value={form.description}
              onChange={e => setForm({ ...form, description: e.target.value })}
            />
          </div>
          <Button type="submit">Add</Button>
        </form>
      )}

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>IP Address</TableHead>
            <TableHead>Description</TableHead>
            <TableHead>Added At</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Action</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {entries.map((entry) => (
            <TableRow key={entry.id}>
              <TableCell className="font-mono font-medium">{entry.ipAddress}</TableCell>
              <TableCell>{entry.description || '-'}</TableCell>
              <TableCell className="text-xs text-muted-foreground">{formatDate(entry.addedAt)}</TableCell>
              <TableCell>
                <span className={`text-xs px-2 py-0.5 rounded-full ${entry.isActive ? 'bg-green-100 text-green-700' : 'bg-gray-100 text-gray-600'}`}>
                  {entry.isActive ? 'Active' : 'Inactive'}
                </span>
              </TableCell>
              <TableCell>
                <Button size="sm" variant="destructive" onClick={() => handleRemove(entry.id)}>Remove</Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
