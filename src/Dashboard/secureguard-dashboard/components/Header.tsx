"use client"

import { useRouter } from "next/navigation"
import { Bell, LogOut, User } from "lucide-react"
import { Button } from "@/components/ui/button"

interface HeaderProps {
  title: string
}

export default function Header({ title }: HeaderProps) {
  const router = useRouter()

  const handleLogout = () => {
    localStorage.removeItem("token")
    router.push("/login")
  }

  return (
    <header className="h-16 bg-white border-b border-gray-200 flex items-center justify-between px-6 shrink-0">
      <h2 className="text-xl font-semibold text-gray-800">{title}</h2>
      <div className="flex items-center gap-2">
        <Button variant="ghost" size="icon" title="Notifications">
          <Bell className="h-5 w-5 text-gray-500" />
        </Button>
        <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-gray-100 text-sm text-gray-700">
          <User className="h-4 w-4" />
          <span className="font-medium">Admin</span>
        </div>
        <Button variant="ghost" size="icon" onClick={handleLogout} title="Sign Out">
          <LogOut className="h-5 w-5 text-gray-500" />
        </Button>
      </div>
    </header>
  )
}
