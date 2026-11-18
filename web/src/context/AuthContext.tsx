import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode
} from "react";
import {
  api,
  clearSession,
  readSession,
  UNAUTHORIZED_EVENT,
  writeSession,
  type StoredSession
} from "../api/client";
import type { UserInfo } from "../types";

interface AuthContextValue {
  user: UserInfo | null;
  token: string | null;
  initializing: boolean;
  authError: string | null;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const EMPTY_USER: UserInfo = { userId: 0, email: "", fullName: "" };

function consumeAuthCallback(): { token: string | null; error: string | null } {
  const hash = window.location.hash.replace(/^#/, "");
  if (!hash) {
    return { token: null, error: null };
  }

  const params = new URLSearchParams(hash);
  const token = params.get("access_token");
  const error = params.get("error");

  if (token || error) {
    window.history.replaceState(null, "", window.location.pathname + window.location.search);
  }

  return { token, error };
}

function friendlyAuthError(error: string): string {
  if (error === "access_denied") {
    return "Google access was denied. JobMailer needs the gmail.send permission to send applications.";
  }

  return error;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<StoredSession | null>(() => readSession());
  const [initializing, setInitializing] = useState(true);
  const [authError, setAuthError] = useState<string | null>(null);

  useEffect(() => {
    const callback = consumeAuthCallback();

    if (callback.error) {
      clearSession();
      setSession(null);
      setAuthError(friendlyAuthError(callback.error));
      setInitializing(false);
      return;
    }

    const existing = readSession();
    const accessToken = callback.token ?? existing?.accessToken ?? null;

    if (!accessToken) {
      setInitializing(false);
      return;
    }

    const expiresAt = existing?.expiresAt;
    const provisional: StoredSession = {
      accessToken,
      expiresAt,
      user: existing?.user ?? EMPTY_USER
    };

    // Store the token before calling /auth/me so the axios interceptor can attach it.
    writeSession(provisional);

    let cancelled = false;

    api
      .get<UserInfo>("/auth/me")
      .then(({ data }) => {
        if (cancelled) {
          return;
        }

        const resolved: StoredSession = { accessToken, expiresAt, user: data };
        writeSession(resolved);
        setSession(resolved);
      })
      .catch(() => {
        if (cancelled) {
          return;
        }

        clearSession();
        setSession(null);
      })
      .finally(() => {
        if (!cancelled) {
          setInitializing(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    const handleUnauthorized = () => setSession(null);
    window.addEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, handleUnauthorized);
  }, []);

  const logout = useCallback(async () => {
    try {
      await api.post("/auth/logout");
    } catch {
      // Even if revocation fails, the local session must be cleared.
    } finally {
      clearSession();
      setSession(null);
      setAuthError(null);
    }
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user: session?.user ?? null,
      token: session?.accessToken ?? null,
      initializing,
      authError,
      logout
    }),
    [session, initializing, authError, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used inside an AuthProvider.");
  }
  return context;
}
