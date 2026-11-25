import CompanyAvatar from "../components/CompanyAvatar.jsx";
import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { getMyApplicationPosting } from "../services/applicationService.js";
import { getInternshipPosting } from "../services/internshipPostingService.js";
import { formatDate, locationLabel, workArrangementLabel } from "../components/formatters.js";
import Icon from "../components/Icon.jsx";
import LoadingState from "../components/LoadingState.jsx";
import StatePanel from "../components/StatePanel.jsx";
import ApplicationForm from "../components/ApplicationForm.jsx";
import PostingCompatibility from "../components/PostingCompatibility.jsx";
import { useAuth } from "../components/AuthProvider.jsx";

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

export default function InternshipPostingDetailPage({ applicationPosting = false }) {
  const { id } = useParams();
  const { user } = useAuth();
  const [state, setState] = useState({ isLoading: true, posting: null, error: null, notFound: false });
  const [attempt, setAttempt] = useState(0);
  const [resumeChoice, setResumeChoice] = useState(null);

  const choiceScope = `${id}:${user?.id ?? ''}`;
  const resumeId = resumeChoice?.scope === choiceScope ? resumeChoice.id : '';
  useEffect(() => { setResumeChoice(null); }, [choiceScope, applicationPosting]);

  useEffect(() => {
    document.title = 'İlan ayrıntıları | StajFlow';
    const abortController = new AbortController();
    setState({ isLoading: true, posting: null, error: null, notFound: false });

    if (!/^\d+$/.test(id) || Number(id) < 1 || Number(id) > 2147483647) {
      setState({ isLoading: false, posting: null, error: null, notFound: true });
      return () => abortController.abort();
    }

    const loadPosting = applicationPosting ? getMyApplicationPosting : getInternshipPosting;
    loadPosting(id, abortController.signal)
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
  }, [id, attempt, applicationPosting]);

  const { isLoading, posting, error, notFound } = state;
  const isExpired = posting?.applicationDeadline && new Date(posting.applicationDeadline) <= new Date();
  const isOpen = posting?.status === 'Published' && !isExpired;
  return (
    <>
      <Link className="back-link" to={applicationPosting ? `/ogrenci/basvurular/${id}` : '/staj-ilanlari'}><Icon name="back" />{applicationPosting ? 'Başvuruma dön' : 'İlanlara dön'}</Link>
      {isLoading ? <><h1 className="sr-only">İlan ayrıntıları</h1><LoadingState detail /></> : notFound ? (
        <><h1 className="page-title">İlan bulunamadı</h1><StatePanel title="Bu ilan şu anda görüntülenemiyor" backLink={!applicationPosting}>{applicationPosting ? 'Başvuru bulunamadı veya bu ilanı görüntüleme yetkiniz yok.' : 'İlan kaldırılmış veya yayından alınmış olabilir. Diğer staj ilanlarını inceleyebilirsin.'}</StatePanel></>
      ) : error ? (
        <><h1 className="page-title">İlan ayrıntıları</h1><StatePanel title="İlan ayrıntıları yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel></>
      ) : posting && (
        <>
          <header className="detail-header">
            <div className="detail-company"><CompanyAvatar name={posting.companyName} logoUrl={posting.companyLogoUrl} /><span>{posting.companyName}</span></div>
            <h1>{posting.title}</h1>
            {posting.shortDescription && <p className="intro-copy">{posting.shortDescription}</p>}
            <div className="detail-tags">
              <span className={`badge${isOpen ? ' badge-published' : ''}`}>{isOpen && <span className="status-dot" />}{isOpen ? 'Yayında' : isExpired ? 'Süresi doldu' : 'Başvuruya kapalı'}</span>
              <span className="badge">{workArrangementLabel(posting.workArrangement)}</span>
              <span className="posted-date">Yayımlanma: {formatDate(posting.createdAt)}</span>
            </div>
          </header>
          {applicationPosting && <p className="success-notice" role="status">{isExpired
            ? 'Bu ilanın başvuru süresi dolmuştur. Mevcut başvurunuz korunur; ilanın ayrıntılarını inceleyebilirsiniz.'
            : !isOpen ? 'Bu ilan yeni başvurulara kapalıdır. Mevcut başvurunuz üzerinden ayrıntıları inceleyebilirsiniz.'
            : 'Başvurduğunuz ilanın güncel ayrıntılarını görüntülüyorsunuz.'}</p>}
          {!applicationPosting && posting.id === Number(id) && isOpen && user?.role === 'Student' && <PostingCompatibility
            key={choiceScope} postingId={posting.id} resumeId={resumeId} />}
          <div className="detail-grid">
            <article className="detail-content">
              <section>
                <p className="eyebrow">STAJ HAKKINDA</p>
                <h2>İş tanımı</h2>
                <p className="description">{posting.description}</p>
              </section>
              <Requirements title="Zorunlu nitelikler" type="Required" requirements={posting.requirements} />
              <Requirements title="Tercih edilen nitelikler" type="Preferred" requirements={posting.requirements} />
              {applicationPosting ? <Link className="button" to={`/ogrenci/basvurular/${id}`}>Başvurumu görüntüle</Link> : <ApplicationForm
                key={choiceScope} posting={posting} resumeId={resumeId} onResumeChange={(selected) => setResumeChoice({ scope: choiceScope, id: selected })} />}
            </article>
            <aside className="summary-card" aria-labelledby="summary-heading">
              <h2 id="summary-heading">Kısaca</h2>
              <dl>
                <div><dt><Icon name="pin" />Konum</dt><dd>{locationLabel(posting)}</dd></div>
                <div><dt><Icon name="globe" />Çalışma biçimi</dt><dd>{workArrangementLabel(posting.workArrangement)}</dd></div>
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

