export interface PaginatedResult<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export interface Monitor {
  id: string
  name: string
  url: string
  port: number | null
  intervalSeconds: number
  isEnabled: boolean
  lastCheckedAt: string | null
  currentStatus: string
  lastResponseTimeMs: number | null
  uptimePercentage30Days: number
  avgResponseTime30Days: number
}

export interface MonitorCheck {
  id: string
  timestamp: string
  isSuccess: boolean
  statusCode: number | null
  responseTimeMs: number
  errorMessage: string | null
}

export interface MonitorDetail extends Monitor {
  checks: PaginatedResult<MonitorCheck>
}

export interface CreateMonitorRequest {
  name: string
  url: string
  port: number | null
  intervalSeconds: number
}

export interface UpdateMonitorRequest {
  name: string
  url: string
  port: number | null
  intervalSeconds: number
  isEnabled: boolean
}
