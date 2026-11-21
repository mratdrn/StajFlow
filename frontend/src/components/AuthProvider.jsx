import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import * as authentication from "../services/authenticationService.js";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isCheckingSession, setIsCheckingSession] = useState(false);
  const [error, setError] = useState(null);
  const [notice, setNotice] = useState('');
  const activeUser = useRef(null);
  const sessionRequest = useRef(null);
  const sessionVersion = useRef(0);
  const channel = useRef(null);

  const cancelSessionCheck = useCallback(() => {
    sessionVersion.current++;
    sessionRequest.current?.controller.abort();
    sessionRequest.current = null;
    setIsCheckingSession(false);
  }, []);

  const refreshSession = useCallback((signal) => {
    if (signal?.aborted) return Promise.resolve(null);
    if (sessionRequest.current) return sessionRequest.current.promise;
    const controller = new AbortController();
    const version = ++sessionVersion.current;
    const abort = () => controller.abort();
    signal?.addEventListener('abort', abort, { once: true });
    setIsCheckingSession(true);
    const promise = (async () => {
      try {
        const current = await authentication.getCurrentUser(controller.signal);
        if (!controller.signal.aborted && version === sessionVersion.current) {
          if (activeUser.current && activeUser.current.id !== current.id)
            setNotice('Başka bir sekmede aktif hesap değişti. Oturum bilgileriniz yenilendi.');
          activeUser.current = current;
          setUser(current);
          setError(null);
        }
        return current;
      } catch (failure) {
        if (controller.signal.aborted || version !== sessionVersion.current) return null;
        if (failure.status === 401) {
          activeUser.current = null;
          setUser(null);
          setError(null);
        } else setError(failure.message);
        return null;
      } finally {
        signal?.removeEventListener('abort', abort);
        if (version === sessionVersion.current) {
          sessionRequest.current = null;
          setIsCheckingSession(false);
          setIsLoading(false);
        }
      }
    })();
    sessionRequest.current = { controller, promise };
    return promise;
  }, []);

  useEffect(() => {
    const checkSession = () => {
      if (document.visibilityState === 'visible') refreshSession();
    };
    const handleSessionExpired = () => {
      cancelSessionCheck();
      if (activeUser.current) setNotice('Oturumunuz kapatıldı. Yeniden giriş yapın.');
      activeUser.current = null;
      setUser(null);
      setError(null);
      setIsLoading(false);
    };
    if ('BroadcastChannel' in window) {
      channel.current = new BroadcastChannel('stajflow-session');
      channel.current.onmessage = (event) => {
        if (event.data?.type === 'session-changed') checkSession();
      };
    }
    window.addEventListener('focus', checkSession);
    document.addEventListener('visibilitychange', checkSession);
    window.addEventListener('auth:unauthorized', handleSessionExpired);
    refreshSession();
    return () => {
      cancelSessionCheck();
      channel.current?.close();
      channel.current = null;
      window.removeEventListener('focus', checkSession);
      document.removeEventListener('visibilitychange', checkSession);
      window.removeEventListener('auth:unauthorized', handleSessionExpired);
    };
  }, [refreshSession, cancelSessionCheck]);

  function notifySessionChanged() {
    channel.current?.postMessage({ type: 'session-changed' });
  }

  async function login(fields) {
    cancelSessionCheck();
    await authentication.login(fields);
    cancelSessionCheck();
    const current = await authentication.getCurrentUser();
    activeUser.current = current;
    setUser(current);
    setError(null);
    setNotice('');
    notifySessionChanged();
    return current;
  }

  async function logout() {
    cancelSessionCheck();
    await authentication.logout();
    cancelSessionCheck();
    activeUser.current = null;
    setNotice('Oturumunuz kapatıldı.');
    setUser(null);
    setError(null);
    notifySessionChanged();
  }

  async function deleteCompanyAccount(password) {
    cancelSessionCheck();
    await authentication.deleteCompanyAccount(password);
    cancelSessionCheck();
    activeUser.current = null;
    setNotice('Şirket hesabınız ve şirkete bağlı ilanlar ile başvurular silindi.');
    setUser(null);
    setError(null);
    notifySessionChanged();
  }

  async function deleteStudentAccount(password) {
    cancelSessionCheck();
    await authentication.deleteStudentAccount(password);
    cancelSessionCheck();
    activeUser.current = null;
    setNotice('Öğrenci hesabınız, CV dosyalarınız ve başvurularınız silindi.');
    setUser(null);
    setError(null);
    notifySessionChanged();
  }

  return <AuthContext.Provider value={{ user, isLoading, isCheckingSession, error, notice, refreshSession, login, logout, deleteCompanyAccount, deleteStudentAccount }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error('useAuth, AuthProvider içinde kullanılmalıdır.');
  return value;
}

export function profilePath(role) {
  return role === 'Company' ? '/sirket/profil' : '/ogrenci/profil';
}
