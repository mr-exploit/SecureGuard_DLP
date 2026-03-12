'use client'

import Link from 'next/link'
import { usePathname } from 'next/navigation'
import { cn } from '@/lib/utils'
import {
  LayoutDashboard,
  AlertTriangle,
  Monitor,
  FileText,
  Shield,
  Globe,
  Settings,
  ShieldCheck
} from 'lucide-react'

const navItems = [
  { href: '/', label: 'Dashboard', icon: LayoutDashboard },
  { href: '/incidents', label: 'Incidents', icon: AlertTriangle },
  { href: '/agents', label: 'Agents', icon: Monitor },
  { href: '/logs', label: 'Logs', icon: FileText },
  { href: '/policies', label: 'Policies', icon: Shield },
  { href: '/whitelist', label: 'Whitelist', icon: Globe },
  { href: '/settings', label: 'Settings', icon: Settings },
]

export default function Sidebar() {
  const pathname = usePathname()

  return (
    <aside className="w-64 bg-slate-900 text-white flex flex-col min-h-screen">
      <div className="flex items-center gap-3 px-6 py-5 border-b border-slate-700">
        <ShieldCheck className="h-8 w-8 text-blue-400" />
        <div>
          <h1 className="font-bold text-lg leading-none">SecureGuard</h1>
          <p className="text-xs text-slate-400">DLP System</p>
        </div>
      </div>
      <nav className="flex-1 px-3 py-4 space-y-1">
        {navItems.map((item) => {
          const Icon = item.icon
          const isActive = pathname === item.href ||
            (item.href !== '/' && pathname.startsWith(item.href))
          return (
            <Link
              key={item.href}
              href={item.href}
              className={cn(
                'flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors',
                isActive
                  ? 'bg-blue-600 text-white'
                  : 'text-slate-300 hover:bg-slate-800 hover:text-white'
              )}
            >
              <Icon className="h-4 w-4" />
              {item.label}
            </Link>
          )
        })}
      </nav>
      <div className="px-4 py-4 border-t border-slate-700">
        <p className="text-xs text-slate-500">v1.0.0 · SecureGuard DLP</p>
      </div>
    </aside>
  )
}
