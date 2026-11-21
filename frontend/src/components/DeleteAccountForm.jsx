import { useState } from "react";
import FormField from "./FormField.jsx";

export default function DeleteAccountForm({ role, onConfirm, onCancel }) {
  const [password, setPassword] = useState('');
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [isDeleting, setIsDeleting] = useState(false);

  async function submit(event) {
    event.preventDefault();
    setIsDeleting(true);
    setError('');
    setErrors({});
    try {
      await onConfirm(password);
    } catch (failure) {
      setError(failure.message);
      setErrors(failure.errors || {});
      setPassword('');
    } finally { setIsDeleting(false); }
  }

  return <section id="delete-account" className="account-deletion-panel" aria-labelledby="delete-account-heading">
    <h3 id="delete-account-heading">{role === 'Company' ? 'Şirket hesabını kalıcı olarak sil' : 'Öğrenci hesabını kalıcı olarak sil'}</h3>
    <p>{role === 'Company'
      ? 'Hesabınız, şirket profiliniz, tüm ilanlarınız, nitelikleriniz ve bu ilanlara gelen başvurular kalıcı olarak silinecek. Bu işlem geri alınamaz.'
      : 'Hesabınız, öğrenci profiliniz, tüm CV kayıt ve dosyalarınız ile başvurularınız kalıcı olarak silinecek. Bu işlem geri alınamaz.'}</p>
    <form onSubmit={submit} aria-busy={isDeleting}>
      <fieldset disabled={isDeleting}>
        <FormField name="password" label="Mevcut parolanız" type="password" autoComplete="current-password"
          required maxLength={128} autoFocus value={password} onChange={(event) => setPassword(event.target.value)} errors={errors} />
        {error && <p className="form-alert" role="alert">{error}</p>}
        <div className="profile-form-actions">
          <button className="button danger-button" type="submit">{isDeleting ? 'Hesap siliniyor…' : 'Hesabımı kalıcı olarak sil'}</button>
          <button className="button secondary-button" type="button" onClick={onCancel}>Vazgeç</button>
        </div>
      </fieldset>
    </form>
  </section>;
}
