import { Card, CardContent } from "@/components/ui/card"
import { cn } from "@/lib/utils"
import type { LucideIcon } from "lucide-react"

interface StatsCardProps {
  title: string
  value: number | string
  icon: LucideIcon
  description?: string
  colorClass?: string
}

export default function StatsCard({
  title,
  value,
  icon: Icon,
  description,
  colorClass = "text-blue-600 bg-blue-50",
}: StatsCardProps) {
  return (
    <Card>
      <CardContent className="p-6">
        <div className="flex items-center justify-between">
          <div className="min-w-0">
            <p className="text-sm font-medium text-gray-500 truncate">{title}</p>
            <p className="text-3xl font-bold text-gray-900 mt-1">{value}</p>
            {description && <p className="text-xs text-gray-500 mt-1">{description}</p>}
          </div>
          <div className={cn("p-3 rounded-full shrink-0 ml-4", colorClass)}>
            <Icon className="h-6 w-6" />
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
