import { useRef, useState } from "react";
import { uploadCompanyLogo } from "../services/authenticationService.js";
import CompanyAvatar from "./CompanyAvatar.jsx";

export default function CompanyLogoUpload({ profile, onUploaded }) {
  const input = useRef(null);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState('');

  async function upload(event) {
    const file = event.target.files?.[0];
    if (!file) return;
    setError('');
    if (file.size === 0 || file.size > 2 * 1024 * 1024 || !['image/png', 'image/jpeg'].includes(file.type)) {
      setError('En fazla 2 MB boyutunda bir PNG veya JPG dosyası seçin.');
      event.target.value = '';
      return;
    }
    setIsUploading(true);
    try {
      const result = await uploadCompanyLogo(file);
      onUploaded(result.logoUrl);
    } catch (failure) {
      setError(failure.errors?.file?.[0] || failure.message);
    } finally {
      setIsUploading(false);
      if (input.current) input.current.value = '';
    }
  }

  return <div className="company-profile-brand" aria-busy={isUploading}>
    <div className="company-profile-identity"><CompanyAvatar name={profile.name} logoUrl={profile.logoUrl} className="company-profile-logo" />
      <div className="company-profile-text"><h2>{profile.name}</h2><p className="company-profile-email">{profile.contactEmail}</p></div></div>
    <div className="company-logo-controls">
      <input ref={input} id="company-logo-file" className="sr-only" type="file" accept="image/png,image/jpeg,.png,.jpg,.jpeg"
        aria-label="Şirket logosunu seçin" disabled={isUploading} onChange={upload} />
      <button className="button secondary-button" type="button" disabled={isUploading} onClick={() => input.current?.click()}>
        {isUploading ? 'Logo yükleniyor…' : profile.logoUrl ? 'Logoyu değiştir' : 'Logo yükle'}</button>
      <p className="field-hint">PNG veya JPG · En fazla 2 MB</p>
    </div>
    {error && <p className="form-alert company-logo-error" role="alert">{error}</p>}
  </div>;
}
