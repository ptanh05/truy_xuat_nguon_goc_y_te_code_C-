export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL || "http://localhost:5196/api";

const toCamelCase = (key: string) =>
  key.replace(/[_-](\w)/g, (_, char: string) => char.toUpperCase());

const camelCaseKeys = <T>(value: T): T => {
  if (Array.isArray(value)) {
    // @ts-ignore - recursive conversion preserves structure
    return value.map((item) => camelCaseKeys(item)) as T;
  }

  if (value && typeof value === "object" && !(value instanceof FormData)) {
    return Object.entries(value as Record<string, unknown>).reduce(
      (acc, [key, val]) => {
        const normalizedKey = toCamelCase(key);
        // @ts-ignore - we know acc is same type
        acc[normalizedKey] = camelCaseKeys(val);
        return acc;
      },
      {} as Record<string, unknown>
    ) as T;
  }

  return value;
};

const withJsonBody = (body?: Record<string, unknown>) =>
  body === undefined ? undefined : JSON.stringify(camelCaseKeys(body));

export async function apiCall(endpoint: string, options?: RequestInit) {
  const url = `${API_BASE_URL}${endpoint}`;
  const isJsonBody =
    options?.body &&
    typeof options.body === "string" &&
    (options.headers as Record<string, string> | undefined)?.["Content-Type"] ===
      "application/json";

  const response = await fetch(url, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(options?.headers || {}),
    },
    body: isJsonBody ? options.body : options?.body,
  });

  if (!response.ok) {
    const text = await response.text().catch(() => "");
    const errorMessage =
      text.length > 200 ? text.substring(0, 200) + "..." : text;
    throw new Error(
      `API call failed: ${response.status} ${response.statusText}${
        errorMessage ? ` - ${errorMessage}` : ""
      }`.trim()
    );
  }
  const contentType = response.headers.get("content-type") || "";
  return contentType.includes("application/json")
    ? response.json()
    : response.text();
}

export const apiRequest = (
  endpoint: string,
  method: string,
  body?: Record<string, unknown>,
  extra?: RequestInit
) =>
  apiCall(endpoint, {
    method,
    body: withJsonBody(body),
    ...(extra || {}),
  });

export const api = {
  get: (endpoint: string) => apiCall(endpoint, { method: "GET" }),
  post: (endpoint: string, body: Record<string, unknown>, extra?: RequestInit) =>
    apiRequest(endpoint, "POST", body, extra),
  put: (endpoint: string, body: Record<string, unknown>) =>
    apiRequest(endpoint, "PUT", body),
  delete: (endpoint: string, body: Record<string, unknown>) =>
    apiRequest(endpoint, "DELETE", body),
};

