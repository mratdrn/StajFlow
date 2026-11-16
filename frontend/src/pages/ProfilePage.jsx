import { roleName } from "../components/formatters.js";
import { useEffect, useState } from "react";
import { useLocation } from "react-router";
import { useAuth } from "../components/AuthProvider.jsx";
import { getMyProfile } from "../services/authenticationService.js";
import StatePanel from "../components/StatePanel.jsx";
import ProfileEditorForm from "../components/ProfileEditorForm.jsx";

export default function ProfilePage({ role }) {
  const { user, refreshSession } = useAuth();
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

  return <section className="profile-page">
    <p className="eyebrow">HESABIM</p>
    <h1 className="page-title">{roleName(role)} profili</h1>
    {location.state?.signedIn && <p className="success-notice" role="status">Başarıyla giriş yaptınız.</p>}
    {successMessage && <p className="success-notice" role="status">{successMessage}</p>}
    {error ? <StatePanel title="Profiliniz yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> :
      !profile ? <p role="status">Profiliniz yükleniyor…</p> :
        <div className="profile-card"><span className="badge badge-published">{roleName(user.role)}</span><h2>{user.displayName}</h2>
          {isEditing ? <ProfileEditorForm key={role} role={role} profile={profile} onSaved={handleProfileSaved}
            cancel={() => setIsEditing(false)} /> : <>
          <dl>
            <div><dt>E-posta</dt><dd>{role === 'Student' ? profile.email : profile.contactEmail}</dd></div>
            {role === 'Student' ? <><div><dt>Telefon numarası</dt><dd>{profile.phoneNumber || 'Belirtilmemiş'}</dd></div><div><dt>Üniversite</dt><dd>{profile.university || 'Belirtilmemiş'}</dd></div><div><dt>Bölüm</dt><dd>{profile.department || 'Belirtilmemiş'}</dd></div><div><dt>Kısa tanıtım</dt><dd>{profile.biography || 'Belirtilmemiş'}</dd></div></> :
              <><div><dt>Sektör</dt><dd>{profile.industry || 'Belirtilmemiş'}</dd></div><div><dt>Konum</dt><dd>{profile.location || 'Belirtilmemiş'}</dd></div><div><dt>Web adresi</dt><dd>{profile.websiteUrl || 'Belirtilmemiş'}</dd></div><div><dt>Şirket hakkında</dt><dd>{profile.description || 'Belirtilmemiş'}</dd></div></>}
          </dl>
          <button className="button" type="button" onClick={() => { setIsEditing(true); setSuccessMessage(''); }}>Profili düzenle</button>
          </>}
        </div>}
  </section>;
}

