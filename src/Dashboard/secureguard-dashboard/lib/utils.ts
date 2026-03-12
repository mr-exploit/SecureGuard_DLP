import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatDate(date: string | Date): string {
  return new Date(date).toLocaleString("id-ID", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
}

export function formatRelativeTime(date: string | Date): string {
  const now = new Date();
  const then = new Date(date);
  const diff = now.getTime() - then.getTime();
  const seconds = Math.floor(diff / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  const days = Math.floor(hours / 24);

  if (seconds < 60) return `${seconds}s ago`;
  if (minutes < 60) return `${minutes}m ago`;
  if (hours < 24) return `${hours}h ago`;
  return `${days}d ago`;
}

export function getSeverityColor(severity: string): string {
  switch (severity.toUpperCase()) {
    case "CRITICAL": return "text-red-600 bg-red-50 border-red-200";
    case "HIGH": return "text-orange-600 bg-orange-50 border-orange-200";
    case "MEDIUM": return "text-yellow-600 bg-yellow-50 border-yellow-200";
    case "LOW": return "text-blue-600 bg-blue-50 border-blue-200";
    default: return "text-gray-600 bg-gray-50 border-gray-200";
  }
}

export function getViolationTypeLabel(type: string): string {
  const labels: Record<string, string> = {
    IMAGE_UPLOAD: "Image Upload",
    CREDENTIAL_FILE: "Credential File",
    CREDENTIAL_PATTERN: "Credential Pattern",
    UNKNOWN_IP: "Unknown IP",
    SENSITIVE_FILE_ACCESS: "Sensitive File Access",
    UNAUTHORIZED_PROCESS: "Unauthorized Process",
  };
  return labels[type] || type;
}
