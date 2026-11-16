import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";
import { getIncomingApplications } from "../services/companyApplicationService.js";
import { getMyPostings } from "../services/companyPostingService.js";
import { applicationStatusLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function IncomingApplicationsPage() {
  const [parameters, setParameters] = useSearchParams();
  const postingId = parameters.get('internshipPostingId') || '';
  const [applications, setApplications] = useState(null);
  const [postings, setPostings] = useState([]);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    document.title = 'Gelen başvurular | StajFlow';
    const abort = new AbortController();
    setApplications(null);
    setError('');
    Promise.all([getIncomingApplications(postingId, abort.signal), getMyPostings(abort.signal)])
      .then(([incomingApplications, companyPostings]) => {
        if (abort.signal.aborted) return;
        setApplications(incomingApplications);
        setPostings(companyPostings);
      }).catch((failure) => { if (!abort.signal.aborted) setError(failure.message); });
    return () => abort.abort();
  }, [postingId, attempt]);

  return <>
    <div className="posting-management-heading"><div><p className="eyebrow">ŞİRKET HESABI</p><h1 className="page-title">Gelen başvurular</h1>
      <p className="muted">İlanlarınıza başvuran öğrencileri ve başvuruda kullandıkları CV'leri inceleyin.</p></div>
      <button className="button secondary-button" type="button" onClick={() => setAttempt((value) => value + 1)}>Başvuruları yenile</button></div>
    <div className="application-posting-filter form-field"><label htmlFor="basvuru-ilani">İlana göre göster</label>
      <select id="basvuru-ilani" value={postingId} onChange={(event) => setParameters(event.target.value ? { internshipPostingId: event.target.value } : {})}>
        <option value="">Tüm ilanlarım</option>
        {postings.map((posting) => <option key={posting.id} value={posting.id}>{posting.title}</option>)}
      </select></div>
    {error ? <StatePanel title="Gelen başvurular yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> : !applications ? <LoadingState /> :
      !applications.length ? <StatePanel title="Henüz başvuru yok">{postingId ? 'Seçtiğiniz ilana henüz başvuru gelmedi.' : 'Öğrenciler ilanlarınıza başvurduğunda burada görünecek.'}</StatePanel> : <>
        <p className="muted" role="status">{applications.length} başvuru</p>
        <div className="company-posting-list">{applications.map((application) => <article className="company-posting-card application-card" key={application.id}>
          <span className="badge">{applicationStatusLabel(application.status)}</span>
          <h2><Link to={`/sirket/basvurular/${application.id}`}>{application.studentFullName}</Link></h2>
          <p>{application.title}</p><p className="muted">{[application.university, application.department].filter(Boolean).join(' · ') || 'Eğitim bilgisi belirtilmemiş'}</p>
          <p className="muted">Başvuru: {new Date(application.appliedAt).toLocaleString('tr-TR')}</p>
          <Link className="button secondary-button" aria-label={`Başvuruyu incele: ${application.studentFullName} - ${application.title}`} to={`/sirket/basvurular/${application.id}`}>Başvuruyu incele</Link>
        </article>)}</div>
      </>}
  </>;
}
