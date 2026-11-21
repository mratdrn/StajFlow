import { useEffect, useState } from "react";
import { Link, useSearchParams } from "react-router";
import { getIncomingApplications } from "../services/companyApplicationService.js";
import { getMyPostings } from "../services/companyPostingService.js";
import { applicationStatusLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import Pagination from "../components/Pagination.jsx";
import LoadingState from "../components/LoadingState.jsx";
import Icon from "../components/Icon.jsx";

const statusFilters = ['', 'Submitted', 'UnderReview', 'Accepted', 'Rejected'];

export default function IncomingApplicationsPage() {
  const [parameters, setParameters] = useSearchParams();
  const postingId = parameters.get('internshipPostingId') || '';
  const status = statusFilters.includes(parameters.get('status')) ? parameters.get('status') : '';
  const requestedPage = Number(parameters.get('page') || 1);
  const page = Number.isSafeInteger(requestedPage) && requestedPage > 0 ? requestedPage : 1;
  const [pagination, setPagination] = useState({ page: 1, totalPages: 0, totalCount: 0 });
  const [applications, setApplications] = useState(null);
  const [postings, setPostings] = useState([]);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  useEffect(() => {
    document.title = 'Gelen başvurular | StajFlow';
    const abort = new AbortController();
    setApplications(null);
    setError('');
    Promise.all([getIncomingApplications(postingId, abort.signal, page, status), getMyPostings(abort.signal)])
      .then(([incomingApplications, companyPostings]) => {
        if (abort.signal.aborted) return;
        setApplications(incomingApplications.items);
        setPagination(incomingApplications);
        if (incomingApplications.page !== page)
          setParameters({ ...(postingId ? { internshipPostingId: postingId } : {}), ...(status ? { status } : {}), page: String(incomingApplications.page) }, { replace: true });
        setPostings(companyPostings);
      }).catch((failure) => { if (!abort.signal.aborted) setError(failure.message); });
    return () => abort.abort();
  }, [postingId, page, status, attempt, setParameters]);

  function changePage(nextPage) {
    setParameters({ ...(postingId ? { internshipPostingId: postingId } : {}), ...(status ? { status } : {}), page: String(nextPage) });
    document.getElementById('incoming-applications-heading')?.scrollIntoView({ block: 'start' });
  }

  return <>
    <div className="posting-management-heading"><div><p className="eyebrow">ŞİRKET HESABI</p><h1 className="page-title" id="incoming-applications-heading">Gelen başvurular</h1>
      <p className="muted">İlanlarınıza başvuran öğrencileri ve başvuruda kullandıkları CV'leri inceleyin.</p></div>
      <button className="button secondary-button" type="button" onClick={() => setAttempt((value) => value + 1)}>Başvuruları yenile</button></div>
    <div className="company-filter-toolbar"><div className="application-posting-filter form-field"><label htmlFor="basvuru-ilani">İlana göre göster</label>
      <select id="basvuru-ilani" value={postingId} onChange={(event) => setParameters({ ...(event.target.value ? { internshipPostingId: event.target.value } : {}), ...(status ? { status } : {}) })}>
        <option value="">Tüm ilanlarım</option>
        {postings.map((posting) => <option key={posting.id} value={posting.id}>{posting.title}</option>)}
      </select></div><p className="muted" role="status">{applications && !error ? `${pagination.totalCount} başvuru · Bu sayfada ${applications.length} kayıt` : 'Başvurularınız'}</p>
      <div className="application-status-filters company-application-status-filters" role="group" aria-label="Başvuru durumuna göre filtrele">
        {statusFilters.map((value) => <button className="button secondary-button application-status-filter" key={value || 'All'}
          type="button" aria-pressed={status === value} onClick={() => {
            if (status !== value) setParameters({ ...(postingId ? { internshipPostingId: postingId } : {}), ...(value ? { status: value } : {}) });
          }}>{value ? applicationStatusLabel(value, 'Company') : 'Tümü'}<span className="application-filter-count">{pagination.statusCounts ? pagination.statusCounts[value || 'All'] || 0 : '—'}</span></button>)}
      </div>
    </div>
    {error ? <StatePanel title="Gelen başvurular yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> : !applications ? <LoadingState /> :
      !applications.length && status ? <StatePanel title="Bu durumda başvuru yok">Seçili ilan kapsamında bu duruma ait başvuru bulunmuyor. Diğer durumları veya Tümü filtresini seçebilirsiniz.</StatePanel> : !applications.length ? <StatePanel title="Henüz başvuru yok">{postingId ? 'Seçtiğiniz ilana henüz başvuru gelmedi.' : 'Öğrenciler ilanlarınıza başvurduğunda burada görünecek.'}</StatePanel> : <>
        <div className="incoming-applications-table-wrapper" role="region" aria-label="Gelen başvurular tablosu" tabIndex={0}>
          <table className="incoming-applications-table">
            <caption className="sr-only">İlanlarınıza yapılan başvurular</caption>
            <thead><tr><th scope="col">Aday</th><th scope="col">Başvurulan ilan</th><th scope="col">Başvuru tarihi</th><th scope="col">Durum</th><th scope="col">İşlem</th></tr></thead>
            <tbody>{applications.map((application) => <tr key={application.id}>
              <td><Link className="incoming-candidate-name" to={`/sirket/basvurular/${application.id}`}>{application.studentFullName}</Link>
                <p className="muted">{[application.university, application.department].filter(Boolean).join(' · ') || 'Eğitim bilgisi belirtilmemiş'}</p></td>
              <td>{application.title}</td>
              <td><time dateTime={application.appliedAt}>{new Date(application.appliedAt).toLocaleDateString('tr-TR')}<span className="muted">{new Date(application.appliedAt).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' })}</span></time></td>
              <td><span className={`badge${application.status === 'Accepted' ? ' badge-accepted' : ''}`}>{applicationStatusLabel(application.status, 'Company')}</span></td>
              <td><Link className="button secondary-button" aria-label={`Başvuruyu incele: ${application.studentFullName} - ${application.title}`} to={`/sirket/basvurular/${application.id}`}>İncele<Icon name="arrow" /></Link></td>
            </tr>)}</tbody>
          </table>
        </div>
        <Pagination page={pagination.page} totalPages={pagination.totalPages} onPageChange={changePage} />
      </>}
  </>;
}
