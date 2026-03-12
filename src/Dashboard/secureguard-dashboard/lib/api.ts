const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'

function getToken(): string | null {
  if (typeof window === 'undefined') return null
  return localStorage.getItem('token')
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = getToken()
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
    ...options.headers,
  }
  const res = await fetch(`${API_URL}${path}`, { ...options, headers })
  if (res.status === 401) {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token')
      window.location.href = '/login'
    }
    throw new Error('Unauthorized')
  }
  if (!res.ok) {
    const error = await res.text()
    throw new Error(error || `Request failed: ${res.status}`)
  }
  return res.json()
}

export const api = {
  login: (username: string, password: string) =>
    request<{ token: string; username: string; role: string }>('/api/auth/login', {
      method: 'POST',
      body: JSON.stringify({ username, password }),
    }),

  getDashboardStats: () => request<any>('/api/dashboard/stats'),

  getLogs: (params: Record<string, string | number> = {}) => {
    const qs = new URLSearchParams(params as Record<string, string>).toString()
    return request<any>(`/api/logs?${qs}`)
  },

  getAlerts: (params: Record<string, string | number> = {}) => {
    const qs = new URLSearchParams(params as Record<string, string>).toString()
    return request<any>(`/api/alerts?${qs}`)
  },

  acknowledgeAlert: (id: number) =>
    request<void>(`/api/alerts/${id}/acknowledge`, { method: 'PUT' }),

  getAgents: () => request<any[]>('/api/agents'),
  getAgent: (id: string) => request<any>(`/api/agents/${id}`),

  getPolicies: () => request<any[]>('/api/policies'),
  createPolicy: (data: any) =>
    request<any>('/api/policies', { method: 'POST', body: JSON.stringify(data) }),
  updatePolicy: (id: number, data: any) =>
    request<void>(`/api/policies/${id}`, { method: 'PUT', body: JSON.stringify(data) }),
  deletePolicy: (id: number) =>
    request<void>(`/api/policies/${id}`, { method: 'DELETE' }),

  getWhitelist: () => request<any[]>('/api/whitelist'),
  addToWhitelist: (data: any) =>
    request<any>('/api/whitelist', { method: 'POST', body: JSON.stringify(data) }),
  removeFromWhitelist: (id: number) =>
    request<void>(`/api/whitelist/${id}`, { method: 'DELETE' }),
}
