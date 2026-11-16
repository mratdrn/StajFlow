import { useEffect, useRef, useState } from "react";
import { Link, useParams } from "react-router";
import { getIncomingApplication, updateApplicationStatus } from "../services/companyApplicationService.js";
import { applicationStatusLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

const statuses = ['UnderReview', 'Accepted', 'Rejected'];

export default function CompanyApplicationDetailPage() {
  const { id } = useParams();
  const [application, setApplication] = useState(null);
  const [error, setError] = useState(null);
  const [newStatus, setNewStatus] = useState('');
  const [registrationError, setRegistrationError] = useState('');
  const [success, setSuccess] = useState('');
  const [hasConflict, setHasConflict] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [attempt, setAttempt] = useState(0);
  const registrationAbort = useRef(null);

  useEffect(() => {
    document.title = 'Başvuruyu incele | StajFlow';
    const abort = new AbortController();
    setApplication(null); setError(null); setRegistrationError(''); setSuccess(''); setHasConflict(false);
    if (!/^[1-9]\d*$/.test(id) || Number(id) > 2147483647) { setError({ status: 404 }); return () => abort.abort(); }
    getIncomingApplication(id, abort.signal).then((data) => {
      if (abort.signal.aborted) return;
      setApplication(data);
      setNewStatus(statuses.includes(data.status) ? data.status : '');
    }).catch((failure) => { if (!abort.signal.aborted) setError(failure); });
    return () => { abort.abort(); registrationAbort.current?.abort(); };
  }, [id, attempt]);

  async function save(event) {
    event.preventDefault();
    if (isSaving || hasConflict) return;
    const abort = new AbortController();
    registrationAbort.current = abort;
    setIsSaving(true); setRegistrationError(''); setSuccess('');
    try {
      const current = await updateApplicationStatus(id, newStatus, application.rowVersion, abort.signal);
      if (abort.signal.aborted) return;
      setApplication(current); setNewStatus(current.status); setSuccess('Başvuru durumu güncellendi.');
    } catch (failure) {
      if (abort.signal.aborted) return;
      setRegistrationError(failure.message);
      setHasConflict(failure.status === 409);
    } finally { if (!abort.signal.aborted) setIsSaving(false); }
  }

  return <div className="profile-page">
    <Link className="back-link" to="/sirket/basvurular">← Gelen başvurulara dön</Link>
    <h1 className="page-title">Başvuruyu incele</h1>
    {error ? error.status === 404 ? <StatePanel title="Başvuru bulunamadı">Bu başvuru mevcut değil veya şirketinize ait bir ilana yapılmamış.</StatePanel> :
      <StatePanel title="Başvuru yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error.message}</StatePanel> : !application ? <LoadingState /> : <>
        <article className="profile-card">
          <span className="badge">{applicationStatusLabel(application.status)}</span>
          <h2>{application.studentFullName}</h2><p className="muted">{application.title}</p>
          <dl>
            <div><dt>E-posta</dt><dd>{application.studentEmail}</dd></div>
            <div><dt>Üniversite ve bölüm</dt><dd>{[application.university, application.department].filter(Boolean).join(' · ') || 'Belirtilmemiş'}</dd></div>
            <div><dt>Başvuru zamanı</dt><dd>{new Date(application.appliedAt).toLocaleString('tr-TR')}</dd></div>
            <div><dt>Son durum güncellemesi</dt><dd>{application.statusUpdatedAt ? new Date(application.statusUpdatedAt).toLocaleString('tr-TR') : 'Henüz güncellenmedi'}</dd></div>
            <div><dt>Ön yazı</dt><dd>{application.coverLetter || 'Eklenmedi'}</dd></div>
            <div><dt>Başvuruda kullanılan özgeçmiş</dt><dd><Link className="text-link" to={`/sirket/basvurular/${application.id}/ozgecmis`}>{application.resumeFileName}</Link></dd></div>
          </dl>
          <div className="profile-form-actions"><Link className="button secondary-button" to={`/sirket/basvurular/${application.id}/ozgecmis`}>CV'yi görüntüle</Link>
            <Link className="button secondary-button" to={`/sirket/ilanlar/${application.internshipPostingId}/duzenle`}>İlanı görüntüle</Link></div>
          <section className="application-section" aria-labelledby="durum-baslik"><h2 id="durum-baslik">Başvuru durumunu güncelle</h2>
            {application.status === 'Withdrawn' ? <p className="muted">Başvuru geri çekilmiş; durumu değiştirilemez.</p> : <form onSubmit={save}>
              <div className="form-field"><label htmlFor="application-status">Yeni başvuru durumu</label>
                <select id="application-status" value={newStatus} onChange={(event) => { setNewStatus(event.target.value); event.target.setCustomValidity(''); }} required disabled={isSaving || hasConflict}
                  onInvalid={(event) => event.currentTarget.setCustomValidity('Yeni başvuru durumunu seçin.')}>
                  <option value="" disabled>Durum seçin</option>{statuses.map((status) => <option key={status} value={status}>{applicationStatusLabel(status)}</option>)}
                </select></div>
              {registrationError && <p className="form-alert" role="alert">{registrationError}</p>}
              {success && <p className="success-notice" role="status">{success}</p>}
              <div className="profile-form-actions"><button className="button" type="submit" disabled={isSaving || hasConflict || !newStatus || newStatus === application.status}>{isSaving ? 'Kaydediliyor…' : 'Durumu kaydet'}</button>
                <button className="button secondary-button" type="button" disabled={isSaving} onClick={() => setAttempt((value) => value + 1)}>{hasConflict ? 'Güncel başvuruyu yükle' : 'Başvuruyu yenile'}</button></div>
            </form>}
          </section>
        </article>
      </>}
  </div>;
}
