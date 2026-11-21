import { useState } from "react";
import { changePassword } from "../services/authenticationService.js";
import FormField from "./FormField.jsx";

export default function ChangePasswordForm({ onSaved, onCancel }) {
  const [fields, setFields] = useState({ currentPassword: '', newPassword: '', confirmPassword: '' });
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  async function save(event) {
    event.preventDefault();
    setErrors({});
    setError('');
    if (fields.newPassword !== fields.confirmPassword) {
      setErrors({ confirmPassword: ['Parolalar eşleşmiyor.'] });
      return;
    }
    setIsSaving(true);
    try {
      await changePassword(fields);
      onSaved();
    } catch (failure) {
      setErrors(failure.errors || {});
      setError(failure.message);
      setFields({ currentPassword: '', newPassword: '', confirmPassword: '' });
    } finally { setIsSaving(false); }
  }

  return <section className="password-change-panel" aria-labelledby="change-password-heading">
    <h3 id="change-password-heading">Şifrenizi değiştirin</h3>
    <p>Yeni parolanız en az 10 karakter olmalı, en az bir küçük harf ve bir rakam içermelidir.</p>
    <form onSubmit={save} aria-busy={isSaving}>
      <fieldset disabled={isSaving}>
        {[
          { name: 'currentPassword', label: 'Mevcut parolanız', autoComplete: 'current-password', autoFocus: true },
          { name: 'newPassword', label: 'Yeni parolanız', autoComplete: 'new-password', minLength: 10 },
          { name: 'confirmPassword', label: 'Yeni parola tekrarı', autoComplete: 'new-password', minLength: 10 },
        ].map((field) => <FormField key={field.name} {...field} type="password" required maxLength={128}
          value={fields[field.name]} errors={errors}
          onChange={(event) => setFields({ ...fields, [field.name]: event.target.value })} />)}
        {error && <p className="form-alert" role="alert">{error}</p>}
        <div className="profile-form-actions">
          <button className="button" type="submit">{isSaving ? 'Kaydediliyor…' : 'Şifreyi değiştir'}</button>
          <button className="button secondary-button" type="button" onClick={onCancel}>Vazgeç</button>
        </div>
      </fieldset>
    </form>
  </section>;
}
