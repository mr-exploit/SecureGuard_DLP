export interface Agent {
  id: string;
  agentId: string;
  hostname: string;
  ipAddress: string;
  username: string;
  osVersion: string;
  agentVersion: string;
  isOnline: boolean;
  lastSeen: string;
  registeredAt: string;
}

export interface LogEntry {
  id: string;
  timestamp: string;
  agentId: string;
  hostname: string;
  username: string;
  processName: string;
  destinationIp: string;
  violationType: string;
  severity: string;
  details: string;
  filePath: string;
  url: string;
}

export interface Alert {
  id: string;
  timestamp: string;
  agentId: string;
  hostname: string;
  violationType: string;
  severity: string;
  message: string;
  details: string;
  isResolved: boolean;
  resolvedAt?: string;
}

export interface Policy {
  id: string;
  name: string;
  description: string;
  ruleType: string;
  action: string;
  isEnabled: boolean;
  parameters: string;
  createdAt: string;
  updatedAt: string;
}

export interface WhitelistEntry {
  id: string;
  ipAddress: string;
  description: string;
  createdAt: string;
  createdBy: string;
}

export interface DashboardStats {
  totalAgents: number;
  onlineAgents: number;
  offlineAgents: number;
  incidentsToday: number;
  activeAlerts: number;
  totalLogs: number;
  logsToday: number;
  violationBreakdown: { violationType: string; count: number }[];
  severityBreakdown: { severity: string; count: number }[];
  recentAlerts: Alert[];
  dailyStats: { date: string; count: number }[];
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    fullName: string;
    role: string;
  };
}

export const VIOLATION_TYPES = {
  IMAGE_UPLOAD: 'IMAGE_UPLOAD',
  CREDENTIAL_FILE: 'CREDENTIAL_FILE',
  CREDENTIAL_PATTERN: 'CREDENTIAL_PATTERN',
  UNKNOWN_IP: 'UNKNOWN_IP',
  SENSITIVE_FILE_ACCESS: 'SENSITIVE_FILE_ACCESS',
  UNAUTHORIZED_PROCESS: 'UNAUTHORIZED_PROCESS',
} as const;

export const SEVERITY_LEVELS = {
  LOW: 'LOW',
  MEDIUM: 'MEDIUM',
  HIGH: 'HIGH',
  CRITICAL: 'CRITICAL',
} as const;
