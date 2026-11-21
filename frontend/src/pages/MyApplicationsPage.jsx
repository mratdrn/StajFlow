import { useEffect, useState } from "react";
import { Link, useLocation, useSearchParams } from "react-router";
import { getMyApplications } from "../services/applicationService.js";
import { applicationStatusLabel, locationLabel, workArrangementLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import Pagination from "../components/Pagination.jsx";
import LoadingState from "../components/LoadingState.jsx";

const statusFilters = [
  { value: '', label: 'Tümü' },
  { value: 'Submitted', label: 'Gönderildi' },
  { value: 'UnderReview', label: 'İnceleniyor' },
  { value: 'Accepted', label: 'Kabul edildi' },
  { value: 'Rejected', label: 'Reddedildi' },
];

export default function MyApplicationsPage() {
  const location = useLocation();
  const [parameters, setParameters] = useSearchParams();
  const status = statusFilters.some((filter) => filter.value === parameters.get('status')) ? parameters.get('status') : '';
  const requestedPage = Number(parameters.get('page') || 1);
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const [pagination, setPagination] = useState({ page: 1, totalPages: 0 });
  const [applications, setApplications] = useState(null);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    document.title = 'Başvurularım | StajFlow';
    const abort = new AbortController();
    setApplications(null);
    setError('');
    let isFetching = false;
    async function loadApplications() {
      if (isFetching) return;
      isFetching = true;
      setError('');
      try {
        const data = await getMyApplications(abort.signal, page, status);
        if (!abort.signal.aborted) {
          setApplications(data.items);
          setPagination(data);
          if (data.page !== page) setParameters({ ...(status ? { status } : {}), page: String(data.page) }, { replace: true });
        }
      } catch (failure) {
        if (!abort.signal.aborted) setError(failure.message);
      } finally { isFetching = false; }
    }
    const refreshOnReturn = () => {
      if (document.visibilityState === 'visible') loadApplications();
    };
    loadApplications();
    window.addEventListener('focus', refreshOnReturn);
    document.addEventListener('visibilitychange', refreshOnReturn);
    return () => {
      abort.abort();
      window.removeEventListener('focus', refreshOnReturn);
      document.removeEventListener('visibilitychange', refreshOnReturn);
    };
  }, [attempt, page, status, setParameters]);

  function changePage(nextPage) {
    setParameters({ ...(status ? { status } : {}), page: String(nextPage) });
    document.getElementById('applications-heading')?.scrollIntoView({ block: 'start' });
  }

  return <>
    <div className="posting-management-heading"><div><p className="eyebrow">ÖĞRENCİ HESABI</p><h1 className="page-title" id="applications-heading">Başvurularım</h1>
      <p className="muted">Gönderdiğiniz başvuruları ve güncel durumlarını takip edin.</p></div>
      <div className="profile-form-actions"><button className="button secondary-button" type="button" onClick={() => setAttempt((value) => value + 1)}>Durumları yenile</button><Link className="button" to="/staj-ilanlari">İlanları incele</Link></div></div>
    {location.state?.success && <p className="success-notice" role="status">{location.state.success}</p>}
    <div className="application-status-filters" role="group" aria-label="Başvuru durumuna göre filtrele">
      {statusFilters.map((filter) => <button className="button secondary-button application-status-filter" key={filter.value || 'All'}
        type="button" aria-pressed={status === filter.value} onClick={() => {
          if (status !== filter.value) setParameters(filter.value ? { status: filter.value } : {});
        }}>{filter.label}<span className="application-filter-count">{pagination.statusCounts ? pagination.statusCounts[filter.value || 'All'] || 0 : '—'}</span></button>)}
    </div>
    {error && applications !== null && <p className="form-alert" role="alert">Başvuru durumları yenilenemedi. {error}</p>}
    {error && applications === null ? <StatePanel title="Başvurularınız yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> :
      !applications ? <LoadingState /> : !applications.length && status ? <StatePanel title="Bu durumda başvurunuz yok">Diğer durumları veya Tümü filtresini seçerek başvurularınızı inceleyebilirsiniz.</StatePanel> : !applications.length ? <StatePanel title="Henüz başvurunuz yok">Bir staj ilanını açıp özgeçmişinizi seçerek ilk başvurunuzu yapabilirsiniz.</StatePanel> :
        <><div className="company-posting-list student-application-list">{applications.map((application) => <article className="company-posting-card application-card" key={application.id}>
          <span className={`badge${application.status === 'Accepted' ? ' badge-accepted' : ''}`}>{applicationStatusLabel(application.status)}</span>
          <h2><Link to={`/ogrenci/basvurular/${application.id}`}>{application.title}</Link></h2>
          <p>{application.companyName}</p><p className="muted">{locationLabel(application)} · {workArrangementLabel(application.workArrangement)}</p>
          <p className="muted">Başvuru: {new Date(application.appliedAt).toLocaleString('tr-TR')}</p>
          <p className="muted">Özgeçmiş: {application.resumeFileName}</p>
          {application.statusUpdatedAt && <p className="muted">Durum güncellemesi: {new Date(application.statusUpdatedAt).toLocaleString('tr-TR')}</p>}
          <Link className="button secondary-button" aria-label={`Başvuru ayrıntıları: ${application.title}`} to={`/ogrenci/basvurular/${application.id}`}>Başvuruyu görüntüle</Link>
        </article>)}</div>
        <Pagination page={pagination.page} totalPages={pagination.totalPages} onPageChange={changePage} /></>}
  </>;
}
