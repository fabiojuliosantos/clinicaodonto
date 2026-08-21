const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

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
): Promise<TResponse> {
  if (!apiBaseUrl) {
    throw new Error('A URL da API não foi configurada.')
  }

  const response = await fetch(new URL(path, apiBaseUrl), {
    ...init,
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
      ...init?.headers,
    },
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

  return response.json() as Promise<TResponse>
}
