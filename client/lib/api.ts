export const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5196/api";

export async function apiCall(endpoint: string, options?: RequestInit) {
  const url = `${API_BASE_URL}${endpoint}`;
  const response = await fetch(url, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(options?.headers || {}),
    },
  });
  if (!response.ok) {
    const text = await response.text().catch(() => '');
    throw new Error(`API call failed: ${response.status} ${response.statusText} ${text}`.trim());
  }
  const contentType = response.headers.get('content-type') || '';
  return contentType.includes('application/json') ? response.json() : response.text();
}

export const api = {
  get: (endpoint: string) => apiCall(endpoint, { method: 'GET' }),
  post: (endpoint: string, body: any, extra?: RequestInit) =>
    apiCall(endpoint, { method: 'POST', body: JSON.stringify(body), ...(extra || {}) }),
  put: (endpoint: string, body: any) =>
    apiCall(endpoint, { method: 'PUT', body: JSON.stringify(body) }),
  delete: (endpoint: string, body: any) =>
    apiCall(endpoint, { method: 'DELETE', body: JSON.stringify(body) }),
};


