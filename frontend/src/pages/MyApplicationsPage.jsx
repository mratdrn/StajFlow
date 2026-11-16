import { useEffect, useState } from "react";
import { Link } from "react-router";
import { getMyApplications } from "../services/applicationService.js";
import { applicationStatusLabel, locationLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function MyApplicationsPage() {
  const [applications, setApplications] = useState(null);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    document.title = 'Başvurularım | StajFlow';
    const abort = new AbortController();
    setApplications(null);
    setError('');
    getMyApplications(abort.signal).then((data) => { if (!abort.signal.aborted) setApplications(data); })
      .catch((failure) => { if (!abort.signal.aborted) setError(failure.message); });
    return () => abort.abort();
  }, [attempt]);

  return <>
    <div className="posting-management-heading"><div><p className="eyebrow">ÖĞRENCİ HESABI</p><h1 className="page-title">Başvurularım</h1>
      <p className="muted">Gönderdiğiniz başvuruları ve güncel durumlarını takip edin.</p></div>
      <div className="profile-form-actions"><button className="button secondary-button" type="button" onClick={() => setAttempt((value) => value + 1)}>Durumları yenile</button><Link className="button" to="/staj-ilanlari">İlanları incele</Link></div></div>
    {error ? <StatePanel title="Başvurularınız yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> :
      !applications ? <LoadingState /> : !applications.length ? <StatePanel title="Henüz başvurunuz yok">Bir staj ilanını açıp özgeçmişinizi seçerek ilk başvurunuzu yapabilirsiniz.</StatePanel> :
        <div className="company-posting-list">{applications.map((application) => <article className="company-posting-card application-card" key={application.id}>
          <span className="badge">{applicationStatusLabel(application.status)}</span>
          <h2><Link to={`/ogrenci/basvurular/${application.id}`}>{application.title}</Link></h2>
          <p>{application.companyName}</p><p className="muted">{locationLabel(application)} · {application.isRemote ? 'Uzaktan' : 'İş yerinde'}</p>
          <p className="muted">Başvuru: {new Date(application.appliedAt).toLocaleString('tr-TR')}</p>
          <p className="muted">Özgeçmiş: {application.resumeFileName}</p>
          {application.statusUpdatedAt && <p className="muted">Durum güncellemesi: {new Date(application.statusUpdatedAt).toLocaleString('tr-TR')}</p>}
          <Link className="button secondary-button" aria-label={`Başvuru ayrıntıları: ${application.title}`} to={`/ogrenci/basvurular/${application.id}`}>Başvuruyu görüntüle</Link>
        </article>)}</div>}
  </>;
}
