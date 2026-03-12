import * as signalR from '@microsoft/signalr'

const SIGNALR_URL = process.env.NEXT_PUBLIC_SIGNALR_URL || 'http://localhost:5000'

let connection: signalR.HubConnection | null = null

export function getSignalRConnection(): signalR.HubConnection {
  if (connection) return connection

  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null

  connection = new signalR.HubConnectionBuilder()
    .withUrl(`${SIGNALR_URL}/hubs/alerts`, {
      accessTokenFactory: () => token || '',
      skipNegotiation: false,
      transport: signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.ServerSentEvents |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Warning)
    .build()

  return connection
}

export async function startSignalR(
  onAlert: (alert: any) => void,
  onConnected?: () => void
): Promise<void> {
  const conn = getSignalRConnection()
  conn.on('NewAlert', onAlert)
  conn.on('Connected', (msg: string) => {
    if (onConnected) onConnected()
  })

  if (conn.state === signalR.HubConnectionState.Disconnected) {
    try {
      await conn.start()
    } catch (err) {
      console.warn('SignalR connection failed:', err)
    }
  }
}

export async function stopSignalR(): Promise<void> {
  if (connection && connection.state !== signalR.HubConnectionState.Disconnected) {
    await connection.stop()
  }
  connection = null
}
