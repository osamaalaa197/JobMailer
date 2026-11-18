import axios, { type AxiosError } from "axios";
import type { UserInfo } from "../types";

export const SESSION_STORAGE_KEY = "jobmailer.session";
export const UNAUTHORIZED_EVENT = "jobmailer:unauthorized";
export const API_BASE_URL = import.meta.env.VITE_API_URL ?? "http://localhost:5080/api";

export interface StoredSession {
  accessToken: string;
  expiresAt?: string;
  user: UserInfo;
}

function isStoredSession(value: unknown): value is StoredSession {
  if (!value || typeof value !== "object") {
    return false;
  }

  const candidate = value as Partial<StoredSession>;
  return (
    typeof candidate.accessToken === "string" &&
    candidate.accessToken.length > 0 &&
    !!candidate.user &&
    typeof candidate.user.userId === "number" &&
    typeof candidate.user.email === "string" &&
    typeof candidate.user.fullName === "string"
  );
}

export function readSession(): StoredSession | null {
  const raw = sessionStorage.getItem(SESSION_STORAGE_KEY);
  if (!raw) {
    return null;
  }

  try {
    const parsed: unknown = JSON.parse(raw);
    return isStoredSession(parsed) ? parsed : null;
  } catch {
    return null;
  }
}

export function writeSession(session: StoredSession): void {
  sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session));
}

export function clearSession(): void {
  sessionStorage.removeItem(SESSION_STORAGE_KEY);
}

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

api.interceptors.request.use((config) => {
  const session = readSession();
  if (session) {
    config.headers.Authorization = `Bearer ${session.accessToken}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      clearSession();
      window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
    }
    return Promise.reject(error);
  }
);

export function getApiErrorMessage(error: unknown): string {
  if (!axios.isAxiosError(error)) {
    return error instanceof Error ? error.message : "Something went wrong.";
  }

  const data: unknown = error.response?.data;

  if (typeof data === "string" && data.trim()) {
    return data;
  }

  if (data && typeof data === "object") {
    const payload = data as {
      title?: unknown;
      detail?: unknown;
      errors?: Record<string, string[]>;
    };

    if (payload.errors) {
      const first = Object.values(payload.errors).flat().find(Boolean);
      if (first) {
        return first;
      }
    }

    if (typeof payload.detail === "string" && payload.detail) {
      return payload.detail;
    }

    if (typeof payload.title === "string" && payload.title) {
      return payload.title;
    }
  }

  return error.message || "The request failed.";
}
