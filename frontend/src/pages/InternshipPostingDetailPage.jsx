import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { getInternshipPosting } from "../services/internshipPostingService.js";
import { formatDate, companyInitials, locationLabel } from "../components/formatters.js";
import Icon from "../components/Icon.jsx";
import LoadingState from "../components/LoadingState.jsx";
import StatePanel from "../components/StatePanel.jsx";
import ApplicationForm from "../components/ApplicationForm.jsx";

function Requirements({ title, requirements, type }) {
  const items = requirements.filter((item) => item.type === type);
  return (
    <section className="qualifications">
      <h2>{title}</h2>
      {items.length ? (
        <ul>{items.map((item) => <li key={item.id}><Icon name="check" /><span>{item.description}</span></li>)}</ul>
      ) : <p className="muted">{type === 'Required' ? 'Zorunlu nitelik belirtilmemiş.' : 'Tercih edilen nitelik belirtilmemiş.'}</p>}
    </section>
  );
}

export default function InternshipPostingDetailPage() {
  const { id } = useParams();
  const [state, setState] = useState({ isLoading: true, posting: null, error: null, notFound: false });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    document.title = 'İlan ayrıntıları | StajFlow';
    const abortController = new AbortController();
    setState({ isLoading: true, posting: null, error: null, notFound: false });

    if (!/^\d+$/.test(id) || Number(id) < 1 || Number(id) > 2147483647) {
      setState({ isLoading: false, posting: null, error: null, notFound: true });
      return () => abortController.abort();
    }

    getInternshipPosting(id, abortController.signal)
      .then((posting) => {
        if (abortController.signal.aborted) return;
        document.title = `${posting.title} | StajFlow`;
        setState({ isLoading: false, posting, error: null, notFound: false });
      })
      .catch((failure) => {
        if (abortController.signal.aborted) return;
        setState({ isLoading: false, posting: null, error: failure.message, notFound: failure.status === 404 });
      });

    return () => abortController.abort();
  }, [id, attempt]);

  const { isLoading, posting, error, notFound } = state;
  return (
    <>
      <Link className="back-link" to="/staj-ilanlari"><Icon name="back" />İlanlara dön</Link>
      {isLoading ? <><h1 className="sr-only">İlan ayrıntıları</h1><LoadingState detail /></> : notFound ? (
        <><h1 className="page-title">İlan bulunamadı</h1><StatePanel title="Bu ilan şu anda görüntülenemiyor" backLink>İlan kaldırılmış veya yayından alınmış olabilir. Diğer staj ilanlarını inceleyebilirsin.</StatePanel></>
      ) : error ? (
        <><h1 className="page-title">İlan ayrıntıları</h1><StatePanel title="İlan ayrıntıları yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel></>
      ) : posting && (
        <>
          <header className="detail-header">
            <div className="detail-company"><span className="company-avatar" aria-hidden="true">{companyInitials(posting.companyName)}</span><span>{posting.companyName}</span></div>
            <h1>{posting.title}</h1>
            {posting.shortDescription && <p className="intro-copy">{posting.shortDescription}</p>}
            <div className="detail-tags">
              <span className="badge badge-published"><span className="status-dot" />Yayında</span>
              <span className="badge">{posting.isRemote ? 'Uzaktan' : 'İş yerinde'}</span>
              <span className="posted-date">Yayımlanma: {formatDate(posting.createdAt)}</span>
            </div>
          </header>
          <div className="detail-grid">
            <article className="detail-content">
              <section>
                <p className="eyebrow">STAJ HAKKINDA</p>
                <h2>İş tanımı</h2>
                <p className="description">{posting.description}</p>
              </section>
              <Requirements title="Zorunlu nitelikler" type="Required" requirements={posting.requirements} />
              <Requirements title="Tercih edilen nitelikler" type="Preferred" requirements={posting.requirements} />
              <ApplicationForm key={posting.id} posting={posting} />
            </article>
            <aside className="summary-card" aria-labelledby="summary-heading">
              <h2 id="summary-heading">Kısaca</h2>
              <dl>
                <div><dt><Icon name="pin" />Konum</dt><dd>{locationLabel(posting)}</dd></div>
                <div><dt><Icon name="globe" />Çalışma biçimi</dt><dd>{posting.isRemote ? 'Uzaktan' : 'İş yerinde'}</dd></div>
                <div><dt><Icon name="clock" />Son başvuru tarihi</dt><dd>{formatDate(posting.applicationDeadline)}</dd></div>
                <div><dt><Icon name="calendar" />Başlangıç tarihi</dt><dd>{formatDate(posting.startDate)}</dd></div>
                <div><dt><Icon name="calendar" />Bitiş tarihi</dt><dd>{formatDate(posting.endDate)}</dd></div>
              </dl>
              <Link className="summary-link" to="/staj-ilanlari">Diğer ilanları incele <Icon name="arrow" /></Link>
            </aside>
          </div>
        </>
      )}
    </>
  );
}

