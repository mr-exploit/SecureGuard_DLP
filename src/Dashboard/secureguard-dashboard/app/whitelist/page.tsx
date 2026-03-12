"use client"

import { useEffect, useState } from "react"
import { useRouter } from "next/navigation"
import type { WhitelistEntry } from "@/types"
import { whitelistApi } from "@/lib/api"
import DashboardLayout from "@/components/DashboardLayout"
import WhitelistManager from "@/components/WhitelistManager"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"

export default function WhitelistPage() {
  const router = useRouter()
  const [entries, setEntries] = useState<WhitelistEntry[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!localStorage.getItem("token")) {
      router.replace("/login")
      return
    }
    fetchEntries()
  }, [router])

  const fetchEntries = async () => {
    setLoading(true)
    try {
      const data = await whitelistApi.getAll()
      setEntries(data)
    } catch (err) {
      console.error(err)
    } finally {
      setLoading(false)
    }
  }

  return (
    <DashboardLayout title="IP Whitelist">
      <Card>
        <CardHeader>
          <CardTitle>IP Whitelist</CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            <div className="text-center py-10 text-gray-500">Loading whitelist…</div>
          ) : (
            <WhitelistManager entries={entries} onChanged={fetchEntries} />
          )}
        </CardContent>
      </Card>
    </DashboardLayout>
  )
}
