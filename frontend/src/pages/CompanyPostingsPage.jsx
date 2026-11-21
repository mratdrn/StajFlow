import { useEffect, useState } from "react";
import { Link, useLocation } from "react-router";
import { getMyPostings } from "../services/companyPostingService.js";
import { formatDate, locationLabel, postingStatusLabel, workArrangementLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";
import Icon from "../components/Icon.jsx";

export default function CompanyPostingsPage({ archived = false, drafts = false }) {
  const [postings, setPostings] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');
  const [attempt, setAttempt] = useState(0);
  const location = useLocation();
  const title = drafts ? 'Taslak ilanlar' : archived ? 'Geçmiş ilanlar' : 'İlanlarım';
  const description = drafts ? 'Henüz yayımlanmamış ilanlarınızı düzenleyin ve hazır olduğunuzda yayımlayın.' :
    archived ? 'Süresi dolmuş veya kapatılmış ilanlarınız. Başvurular ve CV bağlantıları korunur.' : 'Yayında olan güncel staj ilanlarınızı yönetin.';

  useEffect(() => {
    document.title = `${title} | StajFlow`;
    const abort = new AbortController();
    setIsLoading(true);
    setError('');
    getMyPostings(abort.signal, drafts ? undefined : archived).then((data) => {
      if (!abort.signal.aborted) setPostings(data.filter((posting) =>
        drafts ? posting.status === 'Draft' : archived ? posting.status !== 'Draft' : posting.status === 'Published'));
    }).catch((failure) => {
      if (!abort.signal.aborted) setError(failure.message);
    }).finally(() => { if (!abort.signal.aborted) setIsLoading(false); });
    return () => abort.abort();
  }, [attempt, archived, drafts, title]);

  return <>
    <div className="posting-management-heading">
      <div><p className="eyebrow">ŞİRKET HESABI</p><h1 className="page-title">{title}</h1><p className="muted">{description}</p></div>
      {!archived && <Link className="button primary-action" to="/sirket/ilanlar/yeni"><Icon name="briefcase" />Yeni ilan oluştur</Link>}
    </div>
    {location.state?.success && <p className="success-notice" role="status">{location.state.success}</p>}
    {isLoading ? <LoadingState /> : error ?
      <StatePanel title="İlanlarınız yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> :
      postings.length === 0 ? <StatePanel title={drafts ? 'Henüz taslak ilanınız yok' : archived ? 'Henüz geçmiş ilanınız yok' : 'Henüz ilanınız yok'}>{drafts ? 'Taslak olarak kaydettiğiniz ilanlar burada görünecek.' : archived ? 'Süresi dolan veya kapattığınız ilanlar burada görünür.' : 'İlk staj ilanınızı oluşturarak aradığınız nitelikleri belirleyin.'}</StatePanel> :
        <>
        <div className="company-list-heading"><h2>{drafts ? 'Taslaklarınız' : archived ? 'İlan arşiviniz' : 'İlan listeniz'}</h2><span className="muted">{postings.length} ilan</span></div>
        <div className="company-posting-list">{postings.map((posting) => <article className="company-posting-card" key={posting.id}>
          <div className="posting-card-header"><span className={`badge ${!archived && posting.status === 'Published' ? 'badge-published' : ''}`}>{posting.status === 'Published' && posting.applicationDeadline && new Date(posting.applicationDeadline) <= new Date() ? 'Süresi doldu' : postingStatusLabel(posting.status)}</span>
            <span className="muted">{formatDate(posting.createdAt)}</span></div>
          <div className="company-posting-body"><div className="company-posting-copy"><h2><Link to={`/sirket/ilanlar/${posting.id}/duzenle`}>{posting.title}</Link></h2>
          {posting.shortDescription && <p className="muted">{posting.shortDescription}</p>}
          </div><dl className="company-posting-meta">
            <div><dt><Icon name="pin" />Konum</dt><dd>{locationLabel(posting)}</dd></div>
            <div><dt><Icon name="globe" />Çalışma biçimi</dt><dd>{workArrangementLabel(posting.workArrangement)}</dd></div>
            <div><dt><Icon name="calendar" />Son başvuru</dt><dd>{formatDate(posting.applicationDeadline)}</dd></div>
          </dl></div>
          <div className="profile-form-actions company-card-actions"><Link className="button" to={`/sirket/ilanlar/${posting.id}/duzenle`}><Icon name="edit" />İlanı düzenle</Link>
            <Link className="button secondary-button" to={`/sirket/basvurular?internshipPostingId=${posting.id}`}>Başvuruları incele<Icon name="arrow" /></Link>
          </div>
        </article>)}</div></>}
  </>;
}
