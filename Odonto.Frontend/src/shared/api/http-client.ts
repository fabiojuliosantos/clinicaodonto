const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

type ResponseType = 'json' | 'blob' | 'void'

export class ApiRequestError extends Error {
  constructor(
    message: string,
    public readonly status: number,
  ) {
    super(message)
    this.name = 'ApiRequestError'
  }
}

export async function apiRequest<TResponse>(
  path: string,
  init?: RequestInit,
  responseType: ResponseType = 'json',
): Promise<TResponse> {
  if (!apiBaseUrl) {
    throw new Error('A URL da API não foi configurada.')
  }

  const headers = new Headers(init?.headers)
  headers.set('Accept', responseType === 'blob' ? 'image/webp' : 'application/json')
  if (init?.body && !(init.body instanceof FormData) && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(new URL(path, apiBaseUrl), {
    ...init,
    headers,
  })

  if (!response.ok) {
    let message = `Não foi possível concluir a solicitação (${response.status}).`
    try {
      const problem = await response.json() as { detail?: string; title?: string }
      message = problem.detail ?? problem.title ?? message
    } catch {
      // Algumas respostas da API não possuem corpo JSON.
    }
    throw new ApiRequestError(message, response.status)
  }

  if (responseType === 'void' || response.status === 204) {
    return undefined as TResponse
  }

  if (responseType === 'blob') {
    return response.blob() as Promise<TResponse>
  }

  return response.json() as Promise<TResponse>
}
