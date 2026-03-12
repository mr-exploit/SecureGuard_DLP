'use client'

import { useState } from 'react'
import { Policy } from '@/types'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Badge } from '@/components/ui/badge'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { api } from '@/lib/api'

interface PolicyFormProps {
  policies: Policy[]
  onRefresh: () => void
}

export default function PolicyForm({ policies, onRefresh }: PolicyFormProps) {
  const [creating, setCreating] = useState(false)
  const [form, setForm] = useState({
    name: '', ruleType: 'IMAGE_UPLOAD', pattern: '', action: 'block', severity: 'HIGH', isEnabled: true
  })

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault()
    try {
      await api.createPolicy(form)
      setForm({ name: '', ruleType: 'IMAGE_UPLOAD', pattern: '', action: 'block', severity: 'HIGH', isEnabled: true })
      setCreating(false)
      onRefresh()
    } catch (err) {
      alert('Failed to create policy.')
    }
  }

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this policy?')) return
    try {
      await api.deletePolicy(id)
      onRefresh()
    } catch (err) {
      alert('Failed to delete policy.')
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={() => setCreating(!creating)}>
          {creating ? 'Cancel' : '+ Add Policy'}
        </Button>
      </div>

      {creating && (
        <form onSubmit={handleCreate} className="bg-gray-50 rounded-lg p-4 space-y-3">
          <div className="grid grid-cols-2 gap-3">
            <Input placeholder="Policy name" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
            <select className="border rounded-md px-3 py-2 text-sm" value={form.ruleType} onChange={e => setForm({ ...form, ruleType: e.target.value })}>
              <option value="IMAGE_UPLOAD">Image Upload</option>
              <option value="CREDENTIAL_FILE">Credential File</option>
              <option value="UNKNOWN_IP">Unknown IP</option>
              <option value="CREDENTIAL_PATTERN">Credential Pattern</option>
            </select>
            <Input placeholder="Pattern" value={form.pattern} onChange={e => setForm({ ...form, pattern: e.target.value })} />
            <select className="border rounded-md px-3 py-2 text-sm" value={form.action} onChange={e => setForm({ ...form, action: e.target.value })}>
              <option value="block">Block</option>
              <option value="flag">Flag</option>
              <option value="allow">Allow</option>
            </select>
            <select className="border rounded-md px-3 py-2 text-sm" value={form.severity} onChange={e => setForm({ ...form, severity: e.target.value })}>
              <option value="LOW">Low</option>
              <option value="MEDIUM">Medium</option>
              <option value="HIGH">High</option>
              <option value="CRITICAL">Critical</option>
            </select>
          </div>
          <Button type="submit">Create Policy</Button>
        </form>
      )}

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Name</TableHead>
            <TableHead>Rule Type</TableHead>
            <TableHead>Action</TableHead>
            <TableHead>Severity</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Action</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {policies.map((policy) => (
            <TableRow key={policy.id}>
              <TableCell className="font-medium">{policy.name}</TableCell>
              <TableCell><span className="text-xs font-mono bg-gray-100 px-2 py-0.5 rounded">{policy.ruleType}</span></TableCell>
              <TableCell><Badge variant="outline">{policy.action}</Badge></TableCell>
              <TableCell>{policy.severity}</TableCell>
              <TableCell>
                <Badge variant={policy.isEnabled ? 'default' : 'secondary'}>
                  {policy.isEnabled ? 'Active' : 'Disabled'}
                </Badge>
              </TableCell>
              <TableCell>
                <Button size="sm" variant="destructive" onClick={() => handleDelete(policy.id)}>Delete</Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}
