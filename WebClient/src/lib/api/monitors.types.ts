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
  recentChecks: MonitorCheck[]
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
