'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import WhitelistManager from '@/components/WhitelistManager'
import { api } from '@/lib/api'
import { WhitelistEntry } from '@/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function WhitelistPage() {
  const router = useRouter()
  const [entries, setEntries] = useState<WhitelistEntry[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    loadWhitelist()
  }, [router])

  const loadWhitelist = async () => {
    try {
      const data = await api.getWhitelist()
      setEntries(data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="flex min-h-screen bg-gray-50">
      <Sidebar />
      <div className="flex-1 flex flex-col">
        <Header title="IP Whitelist" />
        <main className="flex-1 p-6">
          <Card>
            <CardHeader><CardTitle className="text-base">Whitelisted IP Addresses ({entries.length})</CardTitle></CardHeader>
            <CardContent>
              {loading ? (
                <div className="text-center py-8 text-muted-foreground">Loading whitelist...</div>
              ) : (
                <WhitelistManager entries={entries} onRefresh={loadWhitelist} />
              )}
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
