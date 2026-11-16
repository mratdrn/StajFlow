import { useState } from "react";
import { updateMyProfile } from "../services/authenticationService.js";
import FormField from "./FormField.jsx";

const studentFields = [
  { name: 'firstName', label: 'Ad', required: true, maxLength: 100, autoComplete: 'given-name' },
  { name: 'lastName', label: 'Soyad', required: true, maxLength: 100, autoComplete: 'family-name' },
  { name: 'phoneNumber', label: 'Telefon numarası', maxLength: 32, type: 'tel', autoComplete: 'tel' },
  { name: 'university', label: 'Üniversite', maxLength: 200 },
  { name: 'department', label: 'Bölüm', maxLength: 200 },
  { name: 'biography', label: 'Kısa tanıtım', maxLength: 4000, multiline: true, rows: 5 },
];
const companyFields = [
  { name: 'name', label: 'Şirket adı', required: true, maxLength: 200, autoComplete: 'organization' },
  { name: 'industry', label: 'Sektör', maxLength: 150 },
  { name: 'location', label: 'Konum', maxLength: 250 },
  { name: 'websiteUrl', label: 'Web adresi', maxLength: 2048, type: 'url', placeholder: 'https://ornek.com', autoComplete: 'url' },
  { name: 'description', label: 'Şirket hakkında', maxLength: 6000, multiline: true, rows: 5 },
];

export default function ProfileEditorForm({ role, profile, onSaved, cancel }) {
  const definitions = role === 'Student' ? studentFields : companyFields;
  const [fields, setFields] = useState(() => Object.fromEntries(definitions.map((field) => [field.name, profile[field.name] || ''])));
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [isSaving, setIsSaving] = useState(false);

  async function save(event) {
    event.preventDefault();
    setIsSaving(true);
    setErrors({});
    setError('');
    const body = Object.fromEntries(definitions.map((field) => [field.name, fields[field.name].trim() || (field.required ? '' : null)]));
    try {
      const currentProfile = await updateMyProfile(role, body);
      await onSaved(currentProfile);
    } catch (failure) {
      setErrors(failure.errors || {});
      setError(failure.message);
    } finally { setIsSaving(false); }
  }

  return <form className="profile-form" onSubmit={save} aria-busy={isSaving}>
    <h2>Profil bilgilerini düzenle</h2>
    <fieldset disabled={isSaving}>
      {definitions.map((field) => <FormField key={field.name} {...field} errors={errors} value={fields[field.name]}
        onChange={(event) => setFields({ ...fields, [field.name]: event.target.value })} />)}
      {error && <p className="form-alert" role="alert">{error}</p>}
      <div className="profile-form-actions">
        <button className="button" type="submit">{isSaving ? 'Kaydediliyor…' : 'Değişiklikleri kaydet'}</button>
        <button className="button secondary-button" type="button" onClick={cancel}>Vazgeç</button>
      </div>
    </fieldset>
  </form>;
}
