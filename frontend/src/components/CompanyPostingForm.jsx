import { useState } from "react";
import { Link } from "react-router";
import { createCompanyPosting, updateCompanyPosting } from "../services/companyPostingService.js";
import FormField from "./FormField.jsx";
import { postingStatusLabel } from "./formatters.js";

function toLocalDateTime(isoDate) {
  if (!isoDate) return '';
  const date = new Date(isoDate);
  date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
  return date.toISOString().slice(0, 16);
}

export default function CompanyPostingForm({ posting, onSaved, reload }) {
  const [fields, setFields] = useState({
    title: posting?.title || '', shortDescription: posting?.shortDescription || '', description: posting?.description || '', location: posting?.location || '',
    workArrangement: posting?.workArrangement || 'OnSite', status: posting?.status === 'Closed' ? 'Draft' : posting?.status || 'Published',
    applicationDeadline: toLocalDateTime(posting?.applicationDeadline),
    startDate: posting?.startDate || '', endDate: posting?.endDate || '',
  });
  const originalDeadline = toLocalDateTime(posting?.applicationDeadline);
  const requiresFutureDeadline = !posting || fields.applicationDeadline !== originalDeadline ||
    (fields.status === 'Published' && posting.status !== 'Published');
  const minimumDeadline = toLocalDateTime(new Date(Math.ceil(Date.now() / 60000) * 60000));
  const [requirements, setRequirements] = useState(() =>
    (posting?.requirements || []).map((item) => ({ ...item, key: crypto.randomUUID() })));
  const [errors, setErrors] = useState({});
  const [error, setError] = useState('');
  const [hasConflict, setHasConflict] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const handleChange = (event) => setFields({ ...fields, [event.target.name]: event.target.value });

  function updateRequirement(key, field, value) {
    setRequirements((previous) => previous.map((item) => item.key === key ? { ...item, [field]: value } : item));
  }

  async function save(event) {
    event.preventDefault();
    setError('');
    setErrors({});
    setHasConflict(false);
    if (fields.startDate && fields.endDate && fields.endDate < fields.startDate) {
      setErrors({ endDate: ['Bitiş tarihi başlangıç tarihinden önce olamaz.'] });
      return;
    }
    if (requiresFutureDeadline && fields.applicationDeadline && new Date(fields.applicationDeadline).getTime() <= Date.now()) {
      setErrors({ applicationDeadline: ['Son başvuru için bugün veya sonraki bir tarih ve henüz geçmemiş bir saat seçin.'] });
      return;
    }
    setIsSaving(true);
    try {
      const body = {
        ...fields, title: fields.title.trim(), shortDescription: fields.shortDescription.trim(), description: fields.description.trim(),
        location: fields.location.trim() || null,
        applicationDeadline: fields.applicationDeadline ? new Date(fields.applicationDeadline).toISOString() : null,
        startDate: fields.startDate || null, endDate: fields.endDate || null,
        requirements: requirements.map((item) => ({
          ...(item.id ? { id: item.id } : {}), description: item.description.trim(), type: item.type,
        })),
        ...(posting ? { rowVersion: posting.rowVersion } : {}),
      };
      if (posting) await updateCompanyPosting(posting.id, body);
      else await createCompanyPosting(body);
      onSaved(body.status);
    } catch (failure) {
      setError(failure.message);
      setErrors(failure.errors || {});
      setHasConflict(failure.status === 409);
    } finally { setIsSaving(false); }
  }

  return <form className="posting-form" onSubmit={save} aria-busy={isSaving}>
    <fieldset disabled={isSaving}>
      <section className="posting-form-section">
        <h2>İlan bilgileri</h2>
        <FormField name="title" label="İlan başlığı" required maxLength={200} value={fields.title} onChange={handleChange} errors={errors} />
        <FormField name="shortDescription" label="Kısa açıklama" required={!posting || !!posting.shortDescription} maxLength={1000} multiline rows={3} value={fields.shortDescription} onChange={handleChange} errors={errors} hint="İlan kartında gösterilecek kısa tanıtım." />
      </section>
      <section className="posting-form-section">
        <h2>Aranan nitelikler</h2>
        <p className="muted">Her niteliğin zorunlu mu yoksa tercih edilen mi olduğunu seçin. Değişiklikler ilanı kaydettiğinizde uygulanır.</p>
        {requirements.length === 0 && <p className="muted empty-requirements">Henüz nitelik eklenmedi.</p>}
        {requirements.map((item, index) => {
          const descriptionFieldName = `requirements[${index}].description`;
          const fieldErrors = { [descriptionFieldName]: errors[descriptionFieldName] || errors[`Requirements[${index}].Description`] || [] };
          return <fieldset className="requirement-row" key={item.key}>
            <legend>{index + 1}. nitelik</legend>
            <div className="form-field"><label htmlFor={`type-${item.key}`}>{index + 1}. nitelik türü</label>
              <select id={`type-${item.key}`} value={item.type} onChange={(event) => updateRequirement(item.key, 'type', event.target.value)}>
                <option value="Required">Zorunlu</option><option value="Preferred">Tercih edilen</option>
              </select>
              {(errors[`Requirements[${index}].Type`] || []).map((message) => <p className="field-errors" key={message}>{message}</p>)}
            </div>
            <FormField name={descriptionFieldName} label={`${index + 1}. nitelik açıklaması`} required maxLength={1000}
              multiline rows={2} value={item.description} errors={fieldErrors} onChange={(event) => updateRequirement(item.key, 'description', event.target.value)} />
            <button className="text-button" type="button" aria-label={`${index + 1}. niteliği kaldır`}
              onClick={() => setRequirements((previous) => previous.filter((record) => record.key !== item.key))}>Kaldır</button>
          </fieldset>;
        })}
        {(errors.requirements || errors.Requirements || []).map((message) => <p className="field-errors" key={message}>{message}</p>)}
        <button className="button secondary-button" type="button" onClick={() => setRequirements((previous) => [...previous,
          { key: crypto.randomUUID(), description: '', type: 'Required' }])}>Nitelik ekle</button>
      </section>
      <section className="posting-form-section">
        <h2>İş tanımı ve konum</h2>
        <FormField name="description" label="İş tanımı" required maxLength={10000} multiline rows={7} value={fields.description} onChange={handleChange} errors={errors} />
        <div className="posting-form-grid">
          <FormField name="location" label="Konum" maxLength={250} value={fields.location} onChange={handleChange} errors={errors} />
          <FormField name="applicationDeadline" label="Son başvuru tarihi" type="datetime-local" min={requiresFutureDeadline ? minimumDeadline : undefined} value={fields.applicationDeadline} onChange={handleChange} errors={errors} />
        </div>
        <div className="form-field"><label htmlFor="posting-work-arrangement">Çalışma biçimi</label>
          <select id="posting-work-arrangement" name="workArrangement" value={fields.workArrangement} onChange={handleChange}>
            <option value="OnSite">İş yerinde</option><option value="Hybrid">Hibrit</option><option value="Remote">Uzaktan</option>
          </select>
          {(errors.WorkArrangement || errors.workArrangement || []).map((message) => <p className="field-errors" key={message}>{message}</p>)}
        </div>
        <div className="form-field"><label htmlFor="posting-status">İlan durumu</label>
          <select id="posting-status" name="status" value={fields.status} onChange={handleChange}>
            {['Published', 'Draft'].map((status) => <option key={status} value={status}>{postingStatusLabel(status)}</option>)}
          </select>
          {(errors.Status || errors.status || []).map((message) => <p className="field-errors" key={message}>{message}</p>)}
        </div>
      </section>
      {error && <p className="form-alert" role="alert">{error}</p>}
      {hasConflict && <><p className="muted">Güncel sürümü yüklediğinizde kaydedilmemiş düzenlemeleriniz yenilenir.</p>
        <button className="button secondary-button" type="button" onClick={reload}>Güncel ilanı yükle</button></>}
      <div className="profile-form-actions">
        <button className="button primary-action" type="submit">{isSaving ? 'Kaydediliyor…' : posting ? 'Değişiklikleri kaydet' : 'İlanı oluştur'}</button>
        <Link className={`button secondary-button ${isSaving ? 'disabled-link' : ''}`} aria-disabled={isSaving} to="/sirket/ilanlar"
          onClick={(event) => { if (isSaving) event.preventDefault(); }}>Vazgeç</Link>
      </div>
    </fieldset>
  </form>;
}
