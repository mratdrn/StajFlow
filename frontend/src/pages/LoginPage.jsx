import { useEffect, useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router";
import { profilePath, useAuth } from "../components/AuthProvider.jsx";
import FormField from "../components/FormField.jsx";

export default function LoginPage() {
  const { user, isLoading, notice, login } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [fields, setFields] = useState({ email: location.state?.email || '', password: '' });
  const [errors, setErrors] = useState({});
  const [message, setMessage] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const target = location.state?.from;
  const safeTarget = typeof target === 'string' && target.startsWith('/') && !target.startsWith('//') && !target.includes('\\') ? target : null;
  function postLoginPath(role) {
    if (!safeTarget || (safeTarget.startsWith('/ogrenci/') && role !== 'Student') ||
        (safeTarget.startsWith('/sirket/') && role !== 'Company')) return profilePath(role);
    return safeTarget;
  }
  useEffect(() => { document.title = 'Giriş yap | StajFlow'; }, []);
  if (isLoading) return <p role="status">Oturum yükleniyor…</p>;
  if (user) return <Navigate to={postLoginPath(user.role)} replace />;

  async function submit(event) {
    event.preventDefault();
    setIsProcessing(true);
    setErrors({});
    setMessage('');
    try {
      const current = await login(fields);
      navigate(postLoginPath(current.role),
        { replace: true, state: { signedIn: true } });
    } catch (failure) {
      setErrors(failure.errors || {});
      setMessage(failure.message);
    } finally { setIsProcessing(false); }
  }

  return <div className="auth-layout">
    <div className="auth-intro"><p className="eyebrow">TEKRAR HOŞ GELDİNİZ</p><h1>Yeni başlangıçlar<br />burada başlar.</h1><p>Öğrenci veya şirket hesabınızla giriş yapın.</p><Link className="back-link" to="/staj-ilanlari">← Staj ilanları</Link></div>
    <section className="auth-card">
      <h2>Giriş yap</h2><p className="auth-subtitle">Sizi yeniden görmek güzel.</p>
      {location.state?.registered && <p className="success-notice" role="status">Hesabınız oluşturuldu. Devam etmek için giriş yapın.</p>}
      {(notice || location.state?.loggedOut) && <p className="success-notice" role="status">Oturumunuz kapatıldı.</p>}
      <form onSubmit={submit} aria-busy={isProcessing}>
        <fieldset disabled={isProcessing}>
          <FormField name="email" label="E-posta" type="email" autoComplete="username" maxLength={254} required value={fields.email} onChange={(event) => setFields({ ...fields, email: event.target.value })} errors={errors} />
          <FormField name="password" label="Parola" type="password" autoComplete="current-password" maxLength={128} required value={fields.password} onChange={(event) => setFields({ ...fields, password: event.target.value })} errors={errors} />
          {message && <p className="form-alert" role="alert">{message}</p>}
          <button className="button auth-submit" type="submit">{isProcessing ? 'Giriş yapılıyor…' : 'Giriş yap'}</button>
        </fieldset>
      </form>
      <p className="auth-footer">Hesabınız yok mu? <Link to="/kayit/ogrenci">Öğrenci hesabı oluşturun</Link><br />Bir şirketi mi temsil ediyorsunuz? <Link to="/kayit/sirket">Şirket hesabı oluşturun</Link></p>
    </section>
  </div>;
}

