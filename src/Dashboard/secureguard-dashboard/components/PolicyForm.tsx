"use client"

import { useState } from "react"
import type { Policy } from "@/types"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { policiesApi } from "@/lib/api"

interface PolicyFormProps {
  policy?: Policy
  open: boolean
  onClose: () => void
  onSaved: () => void
}

const RULE_TYPES = [
  "IMAGE_UPLOAD",
  "CREDENTIAL_FILE",
  "CREDENTIAL_PATTERN",
  "UNKNOWN_IP",
  "SENSITIVE_FILE_ACCESS",
  "UNAUTHORIZED_PROCESS",
]

const ACTIONS = ["BLOCK", "ALERT", "LOG"]

export default function PolicyForm({ policy, open, onClose, onSaved }: PolicyFormProps) {
  const isEdit = !!policy
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState("")
  const [form, setForm] = useState({
    name: policy?.name ?? "",
    description: policy?.description ?? "",
    ruleType: policy?.ruleType ?? RULE_TYPES[0],
    action: policy?.action ?? ACTIONS[0],
    isEnabled: policy?.isEnabled ?? true,
    parameters: policy?.parameters ?? "{}",
  })

  const handleClose = () => {
    setError("")
    onClose()
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError("")

    // Validate JSON parameters
    try {
      JSON.parse(form.parameters)
    } catch {
      setError("Parameters must be valid JSON")
      return
    }

    setSaving(true)
    try {
      if (isEdit && policy) {
        await policiesApi.update(policy.id, form)
      } else {
        await policiesApi.create(form)
      }
      onSaved()
      handleClose()
    } catch (err) {
      setError("Failed to save policy. Please try again.")
      console.error("Failed to save policy", err)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Policy" : "Create Policy"}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit} className="space-y-4">
          {error && (
            <div className="p-3 rounded-lg bg-red-50 text-red-700 text-sm">{error}</div>
          )}

          <div className="space-y-2">
            <Label htmlFor="policy-name">Name</Label>
            <Input
              id="policy-name"
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              placeholder="Policy name"
              required
            />
          </div>

          <div className="space-y-2">
            <Label htmlFor="policy-desc">Description</Label>
            <Input
              id="policy-desc"
              value={form.description}
              onChange={(e) => setForm({ ...form, description: e.target.value })}
              placeholder="What does this policy do?"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label>Rule Type</Label>
              <Select
                value={form.ruleType}
                onValueChange={(v) => setForm({ ...form, ruleType: v })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {RULE_TYPES.map((type) => (
                    <SelectItem key={type} value={type}>
                      {type}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Action</Label>
              <Select
                value={form.action}
                onValueChange={(v) => setForm({ ...form, action: v })}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {ACTIONS.map((action) => (
                    <SelectItem key={action} value={action}>
                      {action}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="policy-params">Parameters (JSON)</Label>
            <Input
              id="policy-params"
              value={form.parameters}
              onChange={(e) => setForm({ ...form, parameters: e.target.value })}
              placeholder='{"key": "value"}'
              className="font-mono text-sm"
            />
          </div>

          <div className="flex items-center gap-3 py-1">
            <Switch
              id="policy-enabled"
              checked={form.isEnabled}
              onCheckedChange={(checked) => setForm({ ...form, isEnabled: checked })}
            />
            <Label htmlFor="policy-enabled">Enabled</Label>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={saving}>
              {saving ? "Saving…" : isEdit ? "Update Policy" : "Create Policy"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
