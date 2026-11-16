import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router";
import { getMyPostings } from "../services/companyPostingService.js";
import { formatDate, locationLabel, postingStatusLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function CompanyPostingsPage({ archived = false }) {
  const [postings, setPostings] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const location = useLocation();

  useEffect(() => {
    document.title = `${archived ? 'Geçmiş ilanlar' : 'İlanlarım'} | StajFlow`;
    const abort = new AbortController();
    setIsLoading(true);
    setError('');
    getMyPostings(abort.signal, archived).then((data) => {
      if (!abort.signal.aborted) setPostings(data);
    }).catch((failure) => {
      if (!abort.signal.aborted) setError(failure.message);
    }).finally(() => { if (!abort.signal.aborted) setIsLoading(false); });
    return () => abort.abort();
  }, [attempt, archived]);

  return <>
    <div className="posting-management-heading">
      <div><p className="eyebrow">ŞİRKET HESABI</p><h1 className="page-title">{archived ? 'Geçmiş ilanlar' : 'İlanlarım'}</h1><p className="muted">{archived ? 'Süresi dolmuş veya kapatılmış ilanlarınız. Başvurular ve CV bağlantıları korunur.' : 'Aktif ve taslak staj ilanlarınızı yönetin.'}</p></div>
      {!archived && <Link className="button primary-action" to="/sirket/ilanlar/yeni">Yeni ilan oluştur</Link>}
    </div>
    {location.state?.success && <p className="success-notice" role="status">{location.state.success}</p>}
    {isLoading ? <LoadingState /> : error ?
      <StatePanel title="İlanlarınız yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> :
      postings.length === 0 ? <StatePanel title={archived ? 'Henüz geçmiş ilanınız yok' : 'Henüz ilanınız yok'}>{archived ? 'Süresi dolan veya kapattığınız ilanlar burada görünür.' : 'İlk staj ilanınızı oluşturarak aradığınız nitelikleri belirleyin.'}</StatePanel> :
        <div className="company-posting-list">{postings.map((posting) => <article className="company-posting-card" key={posting.id}>
          <div className="posting-card-header"><span className={`badge ${!archived && posting.status === 'Published' ? 'badge-published' : ''}`}>{posting.status !== 'Closed' && posting.applicationDeadline && new Date(posting.applicationDeadline) <= new Date() ? 'Süresi doldu' : postingStatusLabel(posting.status)}</span>
            <span className="muted">{formatDate(posting.createdAt)}</span></div>
          <h2><Link to={`/sirket/ilanlar/${posting.id}/duzenle`}>{posting.title}</Link></h2>
          {posting.shortDescription && <p className="muted">{posting.shortDescription}</p>}
          <p className="muted">{locationLabel(posting)} · {posting.isRemote ? 'Uzaktan' : 'İş yerinde'}</p>
          <p className="muted">Son başvuru: {formatDate(posting.applicationDeadline)}</p>
          <div className="profile-form-actions"><Link className="button" to={`/sirket/ilanlar/${posting.id}/duzenle`}>İlanı düzenle</Link>
            <Link className="button secondary-button" to={`/sirket/basvurular?internshipPostingId=${posting.id}`}>Başvuruları incele</Link>
          </div>
        </article>)}</div>}
  </>;
}
