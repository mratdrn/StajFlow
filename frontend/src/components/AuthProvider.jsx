import { createContext, useCallback, useContext, useEffect, useState } from "react";
import * as authentication from "../services/authenticationService.js";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [notice, setNotice] = useState('');

  const refreshSession = useCallback(async (signal) => {
    setError(null);
    try {
      const current = await authentication.getCurrentUser(signal);
      if (!signal?.aborted) setUser(current);
      return current;
    } catch (failure) {
      if (signal?.aborted) return null;
      setUser(null);
      if (failure.status !== 401) setError(failure.message);
      return null;
    } finally {
      if (!signal?.aborted) setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    const abortController = new AbortController();
    refreshSession(abortController.signal);
    const handleSessionExpired = () => setUser(null);
    window.addEventListener('auth:unauthorized', handleSessionExpired);
    return () => {
      abortController.abort();
      window.removeEventListener('auth:unauthorized', handleSessionExpired);
    };
  }, [refreshSession]);

  async function login(fields) {
    await authentication.login(fields);
    const current = await authentication.getCurrentUser();
    setUser(current);
    setError(null);
    setNotice('');
    return current;
  }

  async function logout() {
    await authentication.logout();
    setNotice('Oturumunuz kapatıldı.');
    setUser(null);
    setError(null);
  }

  return <AuthContext.Provider value={{ user, isLoading, error, notice, refreshSession, login, logout }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth, AuthProvider içinde kullanılmalıdır.');
  return value;
}

export function profilePath(role) {
  return role === 'Company' ? '/sirket/profil' : '/ogrenci/profil';
}

