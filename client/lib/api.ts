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
    let errorMessage = "";
    let errorData: any = null;
    
    try {
      const contentType = response.headers.get("content-type") || "";
      if (contentType.includes("application/json")) {
        errorData = await response.json();
        errorMessage = errorData.error || errorData.message || JSON.stringify(errorData);
      } else {
        const text = await response.text();
        errorMessage = text.length > 200 ? text.substring(0, 200) + "..." : text;
      }
    } catch (e) {
      // Ignore error reading response text
      errorMessage = `HTTP ${response.status}: ${response.statusText}`;
    }
    
    const message = errorMessage
      ? `API call failed: ${response.status} ${response.statusText} - ${errorMessage}`
      : `API call failed: ${response.status} ${response.statusText}`;
    
    const error = new Error(message) as any;
    error.status = response.status;
    error.data = errorData;
    throw error;
  }
  
  // Kiểm tra content-type và xử lý response an toàn
  const contentType = response.headers.get("content-type") || "";
  const text = await response.text();
  
  // Nếu response rỗng hoặc không phải JSON, trả về null hoặc text
  if (!text || text.trim() === "" || text === "undefined") {
    return null;
  }
  
  if (contentType.includes("application/json")) {
    try {
      return JSON.parse(text);
    } catch (e) {
      // Nếu parse JSON thất bại, trả về text
      console.warn("Failed to parse JSON response:", text);
      return text;
    }
  }
  
  return text;
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

