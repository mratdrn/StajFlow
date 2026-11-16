import { roleName } from "../components/formatters.js";
import { useEffect, useState } from "react";
import { Link, Navigate, useNavigate } from "react-router";
import { profilePath, useAuth } from "../components/AuthProvider.jsx";
import FormField from "../components/FormField.jsx";
import { registerCompany, registerStudent } from "../services/authenticationService.js";

export default function RegistrationPage({ role }) {
  const isCompany = role === 'Company';
  const { user, isLoading } = useAuth();
  const [fields, setFields] = useState({ fullName: '', companyName: '', email: '', password: '', confirmPassword: '' });
  const [errors, setErrors] = useState({});
  const [message, setMessage] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const navigate = useNavigate();
  useEffect(() => { document.title = `${roleName(role)} kaydı | StajFlow`; }, [role]);
  if (isLoading) return <p role="status">Oturum yükleniyor…</p>;
  if (user) return <Navigate to={profilePath(user.role)} replace />;

  const handleChange = (event) => setFields({ ...fields, [event.target.name]: event.target.value });

  async function submit(event) {
    event.preventDefault();
    setErrors({});
    setMessage('');
    if (fields.password !== fields.confirmPassword) {
      setErrors({ confirmPassword: ['Parolalar eşleşmiyor.'] });
      return;
    }
    setIsProcessing(true);
    try {
      const { email, password, confirmPassword } = fields;
      const body = { email, password, confirmPassword,
        [isCompany ? 'companyName' : 'fullName']: isCompany ? fields.companyName : fields.fullName };
      await (isCompany ? registerCompany(body) : registerStudent(body));
      navigate('/giris', { replace: true, state: { registered: true, email: fields.email.trim() } });
    } catch (failure) {
      setErrors(failure.errors || {});
      setMessage(failure.message);
    } finally { setIsProcessing(false); }
  }

  return <div className="auth-layout">
    <div className="auth-intro"><p className="eyebrow">YENİ BİR BAŞLANGIÇ</p><h1>{isCompany ? 'Yeni yeteneklere yer açın.' : 'Yolculuğunuza başlayın.'}</h1><p>{isCompany ? 'Bir şirket hesabı oluşturarak şirket profilinizi hazırlayın.' : 'Öğrenci hesabınızı oluşturarak iş deneyimine ilk adımınızı atın.'}</p><Link className="back-link" to="/staj-ilanlari">← Staj ilanları</Link></div>
    <section className="auth-card">
      <h2>Hesabınızı oluşturun</h2>
      <p className="auth-subtitle">{isCompany ? 'Şirket hesabı oluşturun.' : 'Öğrenci hesabı oluşturun.'}</p>
      <div className="account-switch" aria-label="Hesap türü">
        <Link className={!isCompany ? 'selected' : ''} to="/kayit/ogrenci">Öğrenci</Link>
        <Link className={isCompany ? 'selected' : ''} to="/kayit/sirket">Şirket</Link>
      </div>
      <form onSubmit={submit} aria-busy={isProcessing}>
        <fieldset disabled={isProcessing}>
          {isCompany ? <FormField name="companyName" label="Şirket adı" autoComplete="organization" maxLength={200} required value={fields.companyName} onChange={handleChange} errors={errors} /> :
            <FormField name="fullName" label="Ad soyad" autoComplete="name" maxLength={201} required value={fields.fullName} onChange={handleChange} errors={errors} />}
          <FormField name="email" label="E-posta" type="email" autoComplete="email" maxLength={254} required value={fields.email} onChange={handleChange} errors={errors} />
          <FormField name="password" label="Parola" type="password" autoComplete="new-password" minLength={10} maxLength={128} required value={fields.password} onChange={handleChange} errors={errors} hint="Büyük harf, küçük harf, rakam ve simge içeren 10–128 karakter kullanın." />
          <FormField name="confirmPassword" label="Parola tekrarı" type="password" autoComplete="new-password" maxLength={128} required value={fields.confirmPassword} onChange={handleChange} errors={errors} />
          {message && <p className="form-alert" role="alert">{message}</p>}
          <button className="button auth-submit" type="submit">{isProcessing ? 'Hesap oluşturuluyor…' : 'Hesap oluştur'}</button>
        </fieldset>
      </form>
      <p className="auth-footer">Zaten hesabınız var mı? <Link to="/giris">Giriş yap</Link></p>
    </section>
  </div>;
}

