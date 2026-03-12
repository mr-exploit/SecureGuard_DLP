export interface Agent {
  id: string
  hostname: string
  ipAddress: string
  version: string
  status: 'online' | 'offline'
  lastSeen: string
  os: string
  username: string
}

export interface LogEntry {
  timestamp: string
  agentId: string
  hostname: string
  username: string
  processName: string
  destinationIp: string
  violationType: string
  severity: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL'
  details: string
}

export interface Alert {
  id?: number
  timestamp: string
  agentId: string
  hostname: string
  violationType: string
  severity: 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL'
  message: string
  details: string
  acknowledged: boolean
}

export interface Policy {
  id: number
  name: string
  ruleType: string
  pattern: string
  action: 'block' | 'flag' | 'allow'
  isEnabled: boolean
  severity: string
  createdAt: string
}

export interface WhitelistEntry {
  id: number
  ipAddress: string
  description: string
  addedAt: string
  isActive: boolean
}

export interface DashboardStats {
  totalAgents: number
  onlineAgents: number
  incidentsToday: number
  activeAlerts: number
  criticalAlerts: number
  violationBreakdown: { violationType: string; count: number }[]
  recentAlerts: Alert[]
  incidentTrend: { date: string; count: number }[]
}

export interface PaginatedResponse<T> {
  total: number
  page: number
  pageSize: number
  data: T[]
}
