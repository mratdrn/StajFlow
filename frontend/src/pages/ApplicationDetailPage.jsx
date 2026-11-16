import { useEffect, useState } from "react";
import { Link, useLocation, useParams } from "react-router";
import { getMyApplication } from "../services/applicationService.js";
import { applicationStatusLabel, postingStatusLabel, locationLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function ApplicationDetailPage() {
  const { id } = useParams();
  const location = useLocation();
  const [application, setApplication] = useState(null);
  const [error, setError] = useState(null);
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    document.title = 'Başvuru ayrıntıları | StajFlow';
    const abort = new AbortController();
    setApplication(null);
    setError(null);
    if (!/^[1-9]\d*$/.test(id) || Number(id) > 2147483647) { setError({ status: 404 }); return () => abort.abort(); }
    getMyApplication(id, abort.signal).then((data) => { if (!abort.signal.aborted) setApplication(data); })
      .catch((failure) => { if (!abort.signal.aborted) setError(failure); });
    return () => abort.abort();
  }, [id, attempt]);

  return <div className="profile-page">
    <Link className="back-link" to="/ogrenci/basvurular">← Başvurularıma dön</Link>
    <h1 className="page-title">Başvuru ayrıntıları</h1>
    {error ? error.status === 404 ? <StatePanel title="Başvuru bulunamadı">Bu başvuru mevcut değil veya hesabınıza ait değil.</StatePanel> :
      <StatePanel title="Başvuru yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error.message}</StatePanel> :
      !application ? <LoadingState /> : <>
        {location.state?.success && <p className="success-notice" role="status">{location.state.success}</p>}
        <article className="profile-card">
          <span className="badge">{applicationStatusLabel(application.status)}</span>
          <h2>{application.title}</h2><p className="muted">{application.companyName}</p>
          <dl>
            <div><dt>Başvuru durumu</dt><dd>{applicationStatusLabel(application.status)}</dd></div>
            <div><dt>Başvuru zamanı</dt><dd>{new Date(application.appliedAt).toLocaleString('tr-TR')}</dd></div>
            <div><dt>Son durum güncellemesi</dt><dd>{application.statusUpdatedAt ? new Date(application.statusUpdatedAt).toLocaleString('tr-TR') : 'Henüz güncellenmedi'}</dd></div>
            <div><dt>Konum ve çalışma biçimi</dt><dd>{locationLabel(application)} · {application.isRemote ? 'Uzaktan' : 'İş yerinde'}</dd></div>
            <div><dt>Başvuruda kullanılan özgeçmiş</dt><dd><Link className="text-link" to={`/ogrenci/ozgecmisler/${application.resumeId}`}>{application.resumeFileName}</Link></dd></div>
            <div><dt>Ön yazı</dt><dd>{application.coverLetter || 'Eklenmedi'}</dd></div>
            <div><dt>İlanın güncel durumu</dt><dd>{application.postingStatus !== 'Closed' && application.applicationDeadline && new Date(application.applicationDeadline) <= new Date() ? 'Süresi doldu' : postingStatusLabel(application.postingStatus)}</dd></div>
          </dl>
          <div className="profile-form-actions"><button className="button secondary-button" type="button" onClick={() => setAttempt((value) => value + 1)}>Durumu yenile</button>
            {application.postingStatus === 'Published' && (!application.applicationDeadline || new Date(application.applicationDeadline) > new Date()) && <Link className="button" to={`/staj-ilanlari/${application.internshipPostingId}`}>İlanı görüntüle</Link>}</div>
        </article>
      </>}
  </div>;
}
