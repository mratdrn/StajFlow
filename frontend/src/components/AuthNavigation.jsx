import { useState } from "react";
import { NavLink, useNavigate } from "react-router";
import { profilePath, useAuth } from "./AuthProvider.jsx";

export default function AuthNavigation() {
  const { user, isLoading, logout } = useAuth();
  const [isProcessing, setIsProcessing] = useState(false);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  async function handleLogout() {
    setIsProcessing(true);
    setError('');
    try {
      await logout();
      navigate('/giris', { state: { loggedOut: true } });
    } catch (failure) {
      setError(failure.message);
    } finally { setIsProcessing(false); }
  }

  return <div className="navigation-group">
    <nav className="main-navigation" aria-label="Ana menü">
      {user?.role !== 'Company' && <NavLink className="nav-link" to="/staj-ilanlari">Staj ilanları</NavLink>}
      {isLoading ? <span className="nav-status" role="status">Oturum denetleniyor…</span> : user ? (
        <>
          {user.role === 'Company' && <NavLink className="nav-link" to="/sirket/ilanlar">İlanlarım</NavLink>}
          {user.role === 'Company' && <NavLink className="nav-link" to="/sirket/basvurular">Gelen başvurular</NavLink>}
          <NavLink className="nav-link" to={profilePath(user.role)}>{user.role === 'Student' ? 'Öğrenci profili' : 'Şirket profili'}</NavLink>
          {user.role === 'Company' && <NavLink className="nav-link" to="/sirket/gecmis-ilanlar">Geçmiş ilanlar</NavLink>}
          {user.role === 'Student' && <NavLink className="nav-link" to="/ogrenci/ozgecmisler">Özgeçmişlerim</NavLink>}
          {user.role === 'Student' && <NavLink className="nav-link" to="/ogrenci/basvurular">Başvurularım</NavLink>}
          <button className="nav-button" type="button" onClick={handleLogout} disabled={isProcessing}>{isProcessing ? 'Çıkış yapılıyor…' : 'Çıkış yap'}</button>
        </>
      ) : (
        <>
          <NavLink className="nav-link" to="/giris">Giriş yap</NavLink>
          <NavLink className="nav-button primary-action" to="/kayit/ogrenci">Hesap oluştur</NavLink>
        </>
      )}
    </nav>
    {error && <p className="nav-error" role="alert">{error}</p>}
  </div>;
}

