import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router";
import { useAuth } from "./AuthProvider.jsx";
import { getResumes } from "../services/resumeService.js";
import { getMyApplications, submitApplication } from "../services/applicationService.js";
import FormField from "./FormField.jsx";

export default function ApplicationForm({ posting }) {
  const { user, isLoading } = useAuth();
  const [resumes, setResumes] = useState(null);
  const [existingApplication, setExistingApplication] = useState(null);
  const [error, setError] = useState('');
  const [submissionError, setSubmissionError] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [attempt, setAttempt] = useState(0);
  const submissionAbort = useRef(null);
  const navigate = useNavigate();

  useEffect(() => {
    const abort = new AbortController();
    setResumes(null);
    setExistingApplication(null);
    setError('');
    setSubmissionError('');
    if (user?.role === 'Student') {
      Promise.all([getResumes(abort.signal), getMyApplications(abort.signal)])
        .then(([files, applications]) => {
          if (abort.signal.aborted) return;
          setResumes(files.resumes);
          setExistingApplication(applications.find((application) => application.internshipPostingId === posting.id) || null);
        }).catch((failure) => { if (!abort.signal.aborted) setError(failure.message); });
    }
    return () => { abort.abort(); submissionAbort.current?.abort(); };
  }, [user?.id, user?.role, posting.id, attempt]);

  async function submit(event) {
    event.preventDefault();
    if (isSubmitting) return;
    const form = new FormData(event.currentTarget);
    const abort = new AbortController();
    submissionAbort.current = abort;
    setIsSubmitting(true);
    setSubmissionError('');
    try {
      const application = await submitApplication({ internshipPostingId: posting.id, resumeId: Number(form.get('resumeId')), coverLetter: form.get('coverLetter') }, abort.signal);
      if (!abort.signal.aborted) navigate(`/ogrenci/basvurular/${application.id}`, { state: { success: 'Başvurunuz başarıyla gönderildi.' } });
    } catch (failure) {
      if (!abort.signal.aborted) setSubmissionError(failure.message);
    } finally { if (!abort.signal.aborted) setIsSubmitting(false); }
  }

  const isExpired = posting.applicationDeadline && new Date(posting.applicationDeadline).getTime() <= Date.now();
  return <section className="application-section" aria-labelledby="basvuru-baslik">
    <h2 id="basvuru-baslik">Bu ilana başvur</h2>
    {isLoading ? <p role="status">Oturum denetleniyor…</p> : !user ?
      <><p className="muted">Başvurmak için öğrenci hesabınızla giriş yapın.</p><Link className="button primary-action" to="/giris" state={{ from: `/staj-ilanlari/${posting.id}` }}>Giriş yap ve başvur</Link></> :
      user.role !== 'Student' ? <p className="muted">İlanlara yalnızca öğrenci hesapları başvurabilir.</p> :
        error ? <><p className="form-alert" role="alert">{error}</p><button className="button secondary-button" type="button" onClick={() => setAttempt((value) => value + 1)}>Tekrar dene</button></> :
          !resumes ? <p role="status">Başvuru bilgileri yükleniyor…</p> : existingApplication ?
            <><p className="success-notice">Bu ilana zaten başvurdunuz.</p><Link className="button" to={`/ogrenci/basvurular/${existingApplication.id}`}>Başvurumu görüntüle</Link></> :
            isExpired ? <p className="form-alert">Bu ilanın son başvuru tarihi geçti.</p> : !resumes.length ?
              <><p className="muted">Başvurmadan önce PDF özgeçmişinizi yükleyin.</p><Link className="button" to="/ogrenci/ozgecmisler">Özgeçmiş yükle</Link></> :
              <form onSubmit={submit}>
                <fieldset disabled={isSubmitting}>
                  <div className="form-field"><label htmlFor="resumeId">Başvuru için özgeçmiş</label>
                    <select id="resumeId" name="resumeId" required defaultValue="" onInvalid={(event) => event.currentTarget.setCustomValidity('Başvuruda kullanılacak özgeçmişi seçin.')} onChange={(event) => event.currentTarget.setCustomValidity('')}>
                      <option value="" disabled>Özgeçmiş seçin</option>
                      {resumes.map((resume) => <option key={resume.id} value={resume.id}>{resume.originalFileName} · {new Date(resume.uploadedAt).toLocaleString('tr-TR')} · {(resume.fileSizeBytes / 1024).toLocaleString('tr-TR', { maximumFractionDigits: 1 })} KB</option>)}
                    </select><p className="field-hint">Seçtiğiniz PDF bu başvuruya bağlanır.</p></div>
                  <FormField name="coverLetter" label="Ön yazı (isteğe bağlı)" multiline rows={5} maxLength={6000} />
                </fieldset>
                {submissionError && <p className="form-alert" role="alert">{submissionError}</p>}
                <button className="button primary-action" type="submit" disabled={isSubmitting}>{isSubmitting ? 'Gönderiliyor…' : 'Başvuruyu gönder'}</button>
                <Link className="application-tracking-link" to="/ogrenci/basvurular">Başvurularım</Link>
              </form>}
  </section>;
}
