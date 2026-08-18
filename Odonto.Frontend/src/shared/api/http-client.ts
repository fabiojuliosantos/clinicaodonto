const apiBaseUrl = import.meta.env.VITE_API_BASE_URL

export async function apiRequest<TResponse>(
  path: string,
  init?: RequestInit,
): Promise<TResponse> {
  const response = await fetch(new URL(path, apiBaseUrl), {
    ...init,
    headers: {
      Accept: 'application/json',
      ...init?.headers,
    },
  })

  if (!response.ok) {
    throw new Error(`A API respondeu com o status ${response.status}.`)
  }

  return response.json() as Promise<TResponse>
}
