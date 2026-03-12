'use client'

import { useEffect, useState } from 'react'
import { useRouter } from 'next/navigation'
import Sidebar from '@/components/Sidebar'
import Header from '@/components/Header'
import PolicyForm from '@/components/PolicyForm'
import { api } from '@/lib/api'
import { Policy } from '@/types'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function PoliciesPage() {
  const router = useRouter()
  const [policies, setPolicies] = useState<Policy[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem('token')) { router.push('/login'); return }
    loadPolicies()
  }, [router])

  const loadPolicies = async () => {
    try {
      const data = await api.getPolicies()
      setPolicies(data)
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
        <Header title="Policies" />
        <main className="flex-1 p-6">
          <Card>
            <CardHeader><CardTitle className="text-base">Detection Policies</CardTitle></CardHeader>
            <CardContent>
              {loading ? (
                <div className="text-center py-8 text-muted-foreground">Loading policies...</div>
              ) : (
                <PolicyForm policies={policies} onRefresh={loadPolicies} />
              )}
            </CardContent>
          </Card>
        </main>
      </div>
    </div>
  )
}
