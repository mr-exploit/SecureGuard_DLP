"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import DashboardLayout from "@/components/DashboardLayout"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Switch } from "@/components/ui/switch"
import { Badge } from "@/components/ui/badge"
import { Save, ShieldAlert, User, Server, Bell, RefreshCw } from "lucide-react"

interface DashboardSettings {
  alertEmailEnabled: boolean
  alertEmail: string
  signalrEnabled: boolean
  logRetentionDays: string
  autoResolveAlerts: boolean
  autoResolveDays: string
}

const DEFAULT_SETTINGS: DashboardSettings = {
  alertEmailEnabled: false,
  alertEmail: "",
  signalrEnabled: true,
  logRetentionDays: "30",
  autoResolveAlerts: false,
  autoResolveDays: "7",
}

const SETTINGS_KEY = "sg_dashboard_settings"

export default function SettingsPage() {
  const router = useRouter()
  const [saving, setSaving] = useState(false)
  const [saved, setSaved] = useState(false)
  const [settings, setSettings] = useState<DashboardSettings>(DEFAULT_SETTINGS)
  const [userInfo, setUserInfo] = useState<{ email?: string; role?: string; name?: string } | null>(
    null
  )

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.replace("/login")
      return
    }

    // Load persisted settings
    try {
      const raw = localStorage.getItem(SETTINGS_KEY)
      if (raw) setSettings(JSON.parse(raw))
    } catch {}

    // Decode JWT payload for user info
    try {
      const token = localStorage.getItem("token")
      if (token) {
        const payload = JSON.parse(atob(token.split(".")[1]))
        setUserInfo({
          email: payload.email || payload.sub,
          role: payload.role,
          name: payload.fullName || payload.name,
        })
      }
    } catch {}
  }, [router])

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault()
    setSaving(true)
    try {
      localStorage.setItem(SETTINGS_KEY, JSON.stringify(settings))
      setSaved(true)
      setTimeout(() => setSaved(false), 3000)
    } finally {
      setSaving(false)
    }
  }

  const handleReset = () => {
    setSettings(DEFAULT_SETTINGS)
    localStorage.removeItem(SETTINGS_KEY)
    setSaved(false)
  }

  return (
    <DashboardLayout title="Settings">
      <div className="max-w-2xl space-y-6">
        {/* Account Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5 text-blue-500" />
              Account
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div>
                <p className="font-medium text-gray-900">
                  {userInfo?.name || userInfo?.email || "Admin User"}
                </p>
                <p className="text-sm text-gray-500">{userInfo?.email || "—"}</p>
              </div>
              <Badge variant="info">{userInfo?.role || "Administrator"}</Badge>
            </div>
            <Button
              variant="outline"
              className="text-red-600 border-red-200 hover:bg-red-50 hover:border-red-300"
              onClick={() => {
                localStorage.removeItem("token")
                router.push("/login")
              }}
            >
              Sign Out
            </Button>
          </CardContent>
        </Card>

        {/* API / Server Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Server className="h-5 w-5 text-gray-500" />
              Server Configuration
            </CardTitle>
            <CardDescription>Read-only — configured via environment variables</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between py-2 border-b">
              <span className="text-sm text-gray-500">API URL</span>
              <span className="text-sm font-mono text-gray-700">
                {process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000"}
              </span>
            </div>
            <div className="flex items-center justify-between py-2">
              <span className="text-sm text-gray-500">SignalR Hub</span>
              <span className="text-sm font-mono text-gray-700">
                {process.env.NEXT_PUBLIC_SIGNALR_URL || "http://localhost:5000"}/hubs/alerts
              </span>
            </div>
          </CardContent>
        </Card>

        {/* Dashboard Preferences */}
        <form onSubmit={handleSave}>
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <ShieldAlert className="h-5 w-5 text-blue-500" />
                Dashboard Preferences
              </CardTitle>
              <CardDescription>
                These settings are stored locally in your browser.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Notifications */}
              <div className="space-y-4">
                <h3 className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                  <Bell className="h-4 w-4" />
                  Notifications
                </h3>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium">Email Alerts</p>
                    <p className="text-xs text-gray-500">
                      Receive email notifications for new critical alerts
                    </p>
                  </div>
                  <Switch
                    checked={settings.alertEmailEnabled}
                    onCheckedChange={(v) => setSettings({ ...settings, alertEmailEnabled: v })}
                  />
                </div>

                {settings.alertEmailEnabled && (
                  <div className="space-y-2 pl-4 border-l-2 border-blue-100">
                    <Label htmlFor="alert-email">Alert Email Address</Label>
                    <Input
                      id="alert-email"
                      type="email"
                      value={settings.alertEmail}
                      onChange={(e) => setSettings({ ...settings, alertEmail: e.target.value })}
                      placeholder="security@company.com"
                    />
                  </div>
                )}

                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium">Real-time Updates (SignalR)</p>
                    <p className="text-xs text-gray-500">
                      Live alert notifications via WebSocket connection
                    </p>
                  </div>
                  <Switch
                    checked={settings.signalrEnabled}
                    onCheckedChange={(v) => setSettings({ ...settings, signalrEnabled: v })}
                  />
                </div>
              </div>

              {/* Log Retention */}
              <div className="space-y-4 border-t pt-4">
                <h3 className="text-sm font-semibold text-gray-700">Data Retention</h3>
                <div className="space-y-2">
                  <Label htmlFor="retention">Log Retention (days)</Label>
                  <Input
                    id="retention"
                    type="number"
                    min="1"
                    max="365"
                    value={settings.logRetentionDays}
                    onChange={(e) => setSettings({ ...settings, logRetentionDays: e.target.value })}
                    className="w-32"
                  />
                  <p className="text-xs text-gray-500">
                    Logs older than this will be automatically deleted.
                  </p>
                </div>
              </div>

              {/* Automation */}
              <div className="space-y-4 border-t pt-4">
                <h3 className="text-sm font-semibold text-gray-700 flex items-center gap-2">
                  <RefreshCw className="h-4 w-4" />
                  Automation
                </h3>

                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium">Auto-resolve Alerts</p>
                    <p className="text-xs text-gray-500">
                      Automatically mark old alerts as resolved
                    </p>
                  </div>
                  <Switch
                    checked={settings.autoResolveAlerts}
                    onCheckedChange={(v) => setSettings({ ...settings, autoResolveAlerts: v })}
                  />
                </div>

                {settings.autoResolveAlerts && (
                  <div className="space-y-2 pl-4 border-l-2 border-blue-100">
                    <Label htmlFor="auto-resolve-days">Auto-resolve after (days)</Label>
                    <Input
                      id="auto-resolve-days"
                      type="number"
                      min="1"
                      max="90"
                      value={settings.autoResolveDays}
                      onChange={(e) =>
                        setSettings({ ...settings, autoResolveDays: e.target.value })
                      }
                      className="w-32"
                    />
                  </div>
                )}
              </div>

              {/* Actions */}
              <div className="flex items-center gap-3 pt-4 border-t">
                <Button type="submit" disabled={saving}>
                  <Save className="h-4 w-4 mr-2" />
                  {saving ? "Saving…" : "Save Settings"}
                </Button>
                <Button type="button" variant="outline" onClick={handleReset}>
                  Reset to Defaults
                </Button>
                {saved && (
                  <span className="text-sm text-green-600 font-medium">✓ Settings saved</span>
                )}
              </div>
            </CardContent>
          </Card>
        </form>
      </div>
    </DashboardLayout>
  )
}
