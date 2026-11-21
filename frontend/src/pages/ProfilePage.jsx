import { roleName } from "../components/formatters.js";
import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router";
import { useAuth } from "../components/AuthProvider.jsx";
import { getMyProfile } from "../services/authenticationService.js";
import StatePanel from "../components/StatePanel.jsx";
import ProfileEditorForm from "../components/ProfileEditorForm.jsx";
import DeleteAccountForm from "../components/DeleteAccountForm.jsx";
import ChangePasswordForm from "../components/ChangePasswordForm.jsx";
import Icon from "../components/Icon.jsx";
import CompanyLogoUpload from "../components/CompanyLogoUpload.jsx";

export default function ProfilePage({ role }) {
  const { user, refreshSession, deleteCompanyAccount, deleteStudentAccount } = useAuth();
  const navigate = useNavigate();
  const [isManagingAccount, setIsManagingAccount] = useState(false);
  const [accountAction, setAccountAction] = useState('');
  const location = useLocation();
  const [profile, setProfile] = useState(null);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [isEditing, setIsEditing] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  useEffect(() => {
    document.title = `${roleName(role)} profili | StajFlow`;
    const abortController = new AbortController();
    setProfile(null);
    setError('');
    getMyProfile(role, abortController.signal).then((data) => {
      if (!abortController.signal.aborted) setProfile(data);
    }).catch((failure) => { if (!abortController.signal.aborted) setError(failure.message); });
    return () => abortController.abort();
  }, [role, attempt]);

  async function handleProfileSaved(currentProfile) {
    setProfile(currentProfile);
    setIsEditing(false);
    setSuccessMessage('Profil bilgileriniz kaydedildi.');
    await refreshSession();
  }

  return <section className={`profile-page ${role === 'Company' ? 'company-profile-page' : 'student-profile-page'}`}>
    <div className="posting-management-heading"><div>
      <p className="eyebrow">{role === 'Company' ? 'ŞİRKET HESABI' : 'HESABIM'}</p>
      <h1 className="page-title">{roleName(role)} profili</h1>
      <p className="muted">{role === 'Company' ? 'Şirket bilgilerinizi ve hesap tercihlerinizi tek yerden yönetin.' : 'Kişisel bilgilerinizi, eğitiminizi ve hesap tercihlerinizi yönetin.'}</p>
    </div></div>
    {location.state?.signedIn && <p className="success-notice" role="status">Başarıyla giriş yaptınız.</p>}
    {successMessage && <p className="success-notice" role="status">{successMessage}</p>}
    {error ? <StatePanel title="Profiliniz yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> :
      !profile ? <p role="status">Profiliniz yükleniyor…</p> :
        <>
        <div className="profile-card">
          {role === 'Company' ? <CompanyLogoUpload profile={profile} onUploaded={(logoUrl) => {
            setProfile((current) => ({ ...current, logoUrl }));
            setSuccessMessage('Şirket logonuz güncellendi.');
          }} /> : <div className="student-profile-heading">
            <div className="student-profile-identity"><h2>{user.displayName}</h2><p>{profile.email}</p></div>
          </div>}
          {isEditing ? <ProfileEditorForm key={role} role={role} profile={profile} onSaved={handleProfileSaved}
            cancel={() => setIsEditing(false)} /> : null}
        </div>
        {!isEditing && <section className="profile-settings-card" aria-labelledby="profile-settings-heading">
          <h2 id="profile-settings-heading">Profil ve hesap ayarları</h2>
          <div className="profile-setting-row">
            <button className="button" type="button" onClick={() => {
              setIsManagingAccount(false);
              setIsEditing(true);
              setSuccessMessage('');
            }}><Icon name="edit" />Profili düzenle</button>
            <div><h3>{role === 'Company' ? 'Şirket bilgilerini güncelleyin' : 'Profil bilgilerinizi güncelleyin'}</h3><p>{role === 'Company' ? 'Şirket adı, sektör, konum, web adresi ve şirket hakkındaki detayları düzenleyin.' : 'Ad, soyad, telefon, üniversite, bölüm ve tanıtım bilgilerinizi düzenleyin.'}</p></div>
          </div>
          <div className="profile-setting-row">
            <button className="button" type="button" aria-expanded={isManagingAccount}
              aria-controls="profile-account-settings" onClick={() => {
                setIsManagingAccount((value) => !value);
                setAccountAction('');
                setSuccessMessage('');
              }}><Icon name="settings" />Hesabı yönet</button>
            <div><h3>Hesabınızı kolayca yönetin</h3><p>Şifrenizi değiştirin veya hesabınızı güvenli şekilde silin.</p></div>
          </div>
          <div id="profile-account-settings" hidden={!isManagingAccount}>
            {isManagingAccount && <>
              <div className="account-management-actions">
                <button className="button" type="button" aria-expanded={accountAction === 'password'}
                  aria-controls="account-action-panel" onClick={() => { setAccountAction('password'); setSuccessMessage(''); }}>Şifre değiştir</button>
                <button className="button danger-button" type="button" aria-expanded={accountAction === 'delete'}
                  aria-controls="account-action-panel" onClick={() => { setAccountAction('delete'); setSuccessMessage(''); }}>Hesabı sil</button>
              </div>
              <div id="account-action-panel">
              {accountAction === 'password' && <ChangePasswordForm onCancel={() => setAccountAction('')}
                onSaved={() => { setAccountAction(''); setSuccessMessage('Parolanız başarıyla değiştirildi.'); }} />}
              {accountAction === 'delete' && <DeleteAccountForm role={role}
                onCancel={() => setAccountAction('')}
                onConfirm={async (password) => {
                  await (role === 'Company' ? deleteCompanyAccount(password) : deleteStudentAccount(password));
                  navigate('/giris', { replace: true });
                }} />}
              </div>
            </>}
          </div>
        </section>}
        </>}
  </section>;
}

