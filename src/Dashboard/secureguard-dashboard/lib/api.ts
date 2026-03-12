import type {
  Agent,
  Alert,
  DashboardStats,
  LogEntry,
  LoginResponse,
  PaginatedResponse,
  Policy,
  WhitelistEntry,
} from "@/types";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

function getAuthHeaders(): HeadersInit {
  const token =
    typeof window !== "undefined" ? localStorage.getItem("token") : null;
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function fetchApi<T>(
  path: string,
  options?: RequestInit
): Promise<T> {
  const response = await fetch(`${API_URL}${path}`, {
    ...options,
    headers: {
      ...getAuthHeaders(),
      ...options?.headers,
    },
  });

  if (response.status === 401) {
    if (typeof window !== "undefined") {
      localStorage.removeItem("token");
      window.location.href = "/login";
    }
    throw new Error("Unauthorized");
  }

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || `HTTP ${response.status}`);
  }

  return response.json();
}

// Auth
export const authApi = {
  login: (email: string, password: string): Promise<LoginResponse> =>
    fetchApi("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    }),
};

// Dashboard
export const dashboardApi = {
  getStats: (): Promise<DashboardStats> =>
    fetchApi("/api/dashboard/stats"),
};

// Logs
export const logsApi = {
  getAll: (params?: {
    page?: number;
    pageSize?: number;
    agentId?: string;
    violationType?: string;
    severity?: string;
    from?: string;
    to?: string;
  }): Promise<PaginatedResponse<LogEntry>> => {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", params.page.toString());
    if (params?.pageSize) query.set("pageSize", params.pageSize.toString());
    if (params?.agentId) query.set("agentId", params.agentId);
    if (params?.violationType) query.set("violationType", params.violationType);
    if (params?.severity) query.set("severity", params.severity);
    if (params?.from) query.set("from", params.from);
    if (params?.to) query.set("to", params.to);
    return fetchApi(`/api/logs?${query.toString()}`);
  },
};

// Alerts
export const alertsApi = {
  getAll: (params?: {
    page?: number;
    pageSize?: number;
    isResolved?: boolean;
    severity?: string;
  }): Promise<PaginatedResponse<Alert>> => {
    const query = new URLSearchParams();
    if (params?.page) query.set("page", params.page.toString());
    if (params?.pageSize) query.set("pageSize", params.pageSize.toString());
    if (params?.isResolved !== undefined)
      query.set("isResolved", params.isResolved.toString());
    if (params?.severity) query.set("severity", params.severity);
    return fetchApi(`/api/alerts?${query.toString()}`);
  },
  resolve: (id: string): Promise<Alert> =>
    fetchApi(`/api/alerts/${id}/resolve`, { method: "POST" }),
};

// Agents
export const agentsApi = {
  getAll: (): Promise<Agent[]> => fetchApi("/api/agents"),
  getById: (id: string): Promise<Agent> => fetchApi(`/api/agents/${id}`),
  updateConfig: (id: string, config: object): Promise<Agent> =>
    fetchApi(`/api/agents/${id}`, {
      method: "PUT",
      body: JSON.stringify(config),
    }),
};

// Policies
export const policiesApi = {
  getAll: (): Promise<Policy[]> => fetchApi("/api/policies"),
  getById: (id: string): Promise<Policy> => fetchApi(`/api/policies/${id}`),
  create: (policy: Omit<Policy, "id" | "createdAt" | "updatedAt">): Promise<Policy> =>
    fetchApi("/api/policies", {
      method: "POST",
      body: JSON.stringify(policy),
    }),
  update: (id: string, policy: Partial<Policy>): Promise<Policy> =>
    fetchApi(`/api/policies/${id}`, {
      method: "PUT",
      body: JSON.stringify(policy),
    }),
  delete: (id: string): Promise<void> =>
    fetchApi(`/api/policies/${id}`, { method: "DELETE" }),
};

// Whitelist
export const whitelistApi = {
  getAll: (): Promise<WhitelistEntry[]> => fetchApi("/api/whitelist"),
  create: (entry: { ipAddress: string; description: string }): Promise<WhitelistEntry> =>
    fetchApi("/api/whitelist", {
      method: "POST",
      body: JSON.stringify({ ...entry, createdBy: "admin" }),
    }),
  delete: (id: string): Promise<void> =>
    fetchApi(`/api/whitelist/${id}`, { method: "DELETE" }),
};
