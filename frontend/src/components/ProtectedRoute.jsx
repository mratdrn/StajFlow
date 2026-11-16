import { roleName } from "../components/formatters.js";
import { Link, Navigate, Outlet, useLocation } from "react-router";
import { profilePath, useAuth } from "./AuthProvider.jsx";
import StatePanel from "./StatePanel.jsx";

export default function ProtectedRoute({ role }) {
  const { user, isLoading, error, refreshSession } = useAuth();
  const location = useLocation();
  if (isLoading) return <p role="status">Oturum yükleniyor…</p>;
  if (error) return <StatePanel title="Oturum yüklenemedi" isError onRetry={() => refreshSession()}>{error}</StatePanel>;
  if (!user) return <Navigate to="/giris" replace state={{ from: location.pathname }} />;
  if (role && user.role !== role) {
    return <section className="state-panel" role="alert"><h1 className="page-title">Erişim yetkiniz yok</h1><p>Bu sayfaya yalnızca {roleName(role).toLocaleLowerCase('tr-TR')} hesabıyla erişebilirsiniz.</p><Link className="button" to={profilePath(user.role)}>Profilime git</Link></section>;
  }
  return <Outlet />;
}

