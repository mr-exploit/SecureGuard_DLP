"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import {
  LayoutDashboard,
  AlertTriangle,
  Monitor,
  ScrollText,
  Shield,
  ListFilter,
  Settings,
  ShieldAlert,
} from "lucide-react"
import { cn } from "@/lib/utils"

const navItems = [
  { href: "/", label: "Dashboard", icon: LayoutDashboard },
  { href: "/incidents", label: "Incidents", icon: AlertTriangle },
  { href: "/agents", label: "Agents", icon: Monitor },
  { href: "/logs", label: "Logs", icon: ScrollText },
  { href: "/policies", label: "Policies", icon: Shield },
  { href: "/whitelist", label: "Whitelist", icon: ListFilter },
  { href: "/settings", label: "Settings", icon: Settings },
]

export default function Sidebar() {
  const pathname = usePathname()

  return (
    <aside className="w-64 min-h-screen bg-gray-900 text-white flex flex-col shrink-0">
      <div className="p-6 flex items-center gap-3 border-b border-gray-800">
        <ShieldAlert className="h-8 w-8 text-blue-400 shrink-0" />
        <div>
          <h1 className="text-lg font-bold text-white leading-tight">SecureGuard</h1>
          <p className="text-xs text-gray-400">DLP Dashboard</p>
        </div>
      </div>

      <nav className="flex-1 p-4 space-y-1">
        {navItems.map((item) => {
          const Icon = item.icon
          const isActive =
            item.href === "/" ? pathname === "/" : pathname.startsWith(item.href)
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                "flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors",
                isActive
                  ? "bg-blue-600 text-white"
                  : "text-gray-300 hover:bg-gray-800 hover:text-white"
              )}
            >
              <Icon className="h-5 w-5 shrink-0" />
              {item.label}
            </Link>
          )
        })}
      </nav>

      <div className="p-4 border-t border-gray-800">
        <p className="text-xs text-gray-500 text-center">v1.0.0 — SecureGuard DLP</p>
      </div>
    </aside>
  )
}
