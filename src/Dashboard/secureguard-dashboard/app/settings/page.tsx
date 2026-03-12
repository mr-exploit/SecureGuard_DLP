'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Button } from '@/components/ui/button'

export default function SettingsPage() {
  const router = useRouter()
  const [apiUrl, setApiUrl] = useState('')
  const [saved, setSaved] = useState(false)

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    setApiUrl(process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000')
  }, [router])

  const handleSave = () => {
    setSaved(true)
    setTimeout(() => setSaved(false), 2000)
  }

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title="Settings" />
        <main className="flex-1 p-6 space-y-6">
          <Card>
            <CardHeader><CardTitle className="text-base">System Configuration</CardTitle></CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-1">
                <label className="text-sm font-medium">API Server URL</label>
                <Input
                  value={apiUrl}
                  onChange={e => setApiUrl(e.target.value)}
                  placeholder="http://localhost:5000"
                />
                <p className="text-xs text-muted-foreground">Configured via NEXT_PUBLIC_API_URL environment variable</p>
              </div>
              <Button onClick={handleSave}>
                {saved ? 'Saved!' : 'Save Settings'}
              </Button>
            </CardContent>
          </Card>

          <Card>
            <CardHeader><CardTitle className="text-base">About SecureGuard</CardTitle></CardHeader>
            <CardContent className="space-y-2 text-sm text-muted-foreground">
              <p><strong>Version:</strong> 1.0.0</p>
              <p><strong>Stack:</strong> .NET 8 Agent + ASP.NET Core 8 API + Next.js 14 Dashboard</p>
              <p><strong>Database:</strong> PostgreSQL 16 / SQL Server 2022</p>
              <p><strong>Proxy:</strong> Titanium.Web.Proxy 3.2 on 127.0.0.1:8877</p>
              <p><strong>Realtime:</strong> SignalR</p>
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
