import { type ClassValue, clsx } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function formatDate(dateStr: string): string {
  if (!dateStr) return '-'
  const date = new Date(dateStr)
  return date.toLocaleString('en-US', {
    year: 'numeric', month: 'short', day: '2-digit',
    hour: '2-digit', minute: '2-digit', second: '2-digit'
  })
}

export function getSeverityColor(severity: string): string {
  switch (severity?.toUpperCase()) {
    case 'CRITICAL': return 'text-red-700 bg-red-100'
    case 'HIGH': return 'text-orange-700 bg-orange-100'
    case 'MEDIUM': return 'text-yellow-700 bg-yellow-100'
    case 'LOW': return 'text-green-700 bg-green-100'
    default: return 'text-gray-700 bg-gray-100'
  }
}

export function getStatusColor(status: string): string {
  return status === 'online'
    ? 'text-green-700 bg-green-100'
    : 'text-gray-600 bg-gray-100'
}
