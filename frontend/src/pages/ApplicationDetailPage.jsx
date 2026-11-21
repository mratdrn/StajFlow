import { useEffect, useState } from "react";
import { Link, useLocation, useNavigate, useParams } from "react-router";
import { getMyApplication, withdrawApplication } from "../services/applicationService.js";
import { applicationStatusLabel, postingStatusLabel, locationLabel, workArrangementLabel } from "../components/formatters.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function ApplicationDetailPage() {
  const { id } = useParams();
  const location = useLocation();
  const navigate = useNavigate();
  const [application, setApplication] = useState(null);
  const [error, setError] = useState(null);
  const [attempt, setAttempt] = useState(0);
  const [isConfirming, setIsConfirming] = useState(false);
  const [isWithdrawing, setIsWithdrawing] = useState(false);
  const [withdrawalError, setWithdrawalError] = useState('');
  useEffect(() => {
    document.title = 'Başvuru ayrıntıları | StajFlow';
    const abort = new AbortController();
    setIsConfirming(false);
    setWithdrawalError('');
    setApplication(null);
    setError(null);
    if (!/^[1-9]\d*$/.test(id) || Number(id) > 2147483647) { setError({ status: 404 }); return () => abort.abort(); }
    let isFetching = false;
    async function loadApplication() {
      if (isFetching) return;
      isFetching = true;
      setError(null);
      try {
        const data = await getMyApplication(id, abort.signal);
        if (!abort.signal.aborted) setApplication(data);
      } catch (failure) {
        if (!abort.signal.aborted) {
          if (failure.status === 404) setApplication(null);
          setError(failure);
        }
      } finally { isFetching = false; }
    }
    const refreshOnReturn = () => {
      if (document.visibilityState === 'visible') loadApplication();
    };
    loadApplication();
    window.addEventListener('focus', refreshOnReturn);
    document.addEventListener('visibilitychange', refreshOnReturn);
    return () => {
      abort.abort();
      window.removeEventListener('focus', refreshOnReturn);
      document.removeEventListener('visibilitychange', refreshOnReturn);
    };
  }, [id, attempt]);

  async function withdraw() {
    if (isWithdrawing) return;
    setIsWithdrawing(true);
    setWithdrawalError('');
    try {
      await withdrawApplication(id);
      navigate('/ogrenci/basvurular', { replace: true, state: { success: 'Başvurunuz geri çekildi. İlan başvuruya açıksa aynı veya farklı bir CV ile yeniden başvurabilirsiniz.' } });
    } catch (failure) {
      setWithdrawalError(failure.message);
    } finally { setIsWithdrawing(false); }
  }

  return <div className="profile-page">
    <Link className="back-link" to="/ogrenci/basvurular">← Başvurularıma dön</Link>
    <h1 className="page-title">Başvuru ayrıntıları</h1>
    {error && application && <p className="form-alert" role="alert">Başvuru durumu yenilenemedi. {error.message}</p>}
    {error && !application ? error.status === 404 ? <StatePanel title="Başvuru bulunamadı">Bu başvuru mevcut değil veya hesabınıza ait değil.</StatePanel> :
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
            <div><dt>Konum ve çalışma biçimi</dt><dd>{locationLabel(application)} · {workArrangementLabel(application.workArrangement)}</dd></div>
            <div><dt>Başvuruda kullanılan özgeçmiş</dt><dd><Link className="text-link" to={`/ogrenci/ozgecmisler/${application.resumeId}`}>{application.resumeFileName}</Link></dd></div>
            <div><dt>Ön yazı</dt><dd>{application.coverLetter || 'Eklenmedi'}</dd></div>
            <div><dt>İlanın güncel durumu</dt><dd>{application.postingStatus !== 'Closed' && application.applicationDeadline && new Date(application.applicationDeadline) <= new Date() ? 'Süresi doldu' : postingStatusLabel(application.postingStatus)}</dd></div>
          </dl>
          <div className="profile-form-actions"><button className="button secondary-button" type="button" disabled={isWithdrawing} onClick={() => setAttempt((value) => value + 1)}>Durumu yenile</button>
            <Link className="button" to={`/ogrenci/basvurular/${application.id}/ilan`}>İlanı görüntüle</Link></div>
          {['Submitted', 'UnderReview'].includes(application.status) && <section className="application-section" aria-labelledby="withdraw-heading">
            <h3 id="withdraw-heading">Başvuruyu geri çek</h3>
            <p>Başvurunuz listeden ve şirketin gelen başvurularından kaldırılır. İlan başvuruya açıksa aynı veya farklı bir CV ile yeniden başvurabilirsiniz.</p>
            {withdrawalError && <p className="form-alert" role="alert">{withdrawalError}</p>}
            {isConfirming ? <>
              <p>Başvurunuzu geri çekmek istediğinizden emin misiniz?</p>
              <div className="profile-form-actions">
                <button className="button danger-button" type="button" disabled={isWithdrawing} onClick={withdraw}>{isWithdrawing ? 'Geri çekiliyor…' : 'Evet, geri çek'}</button>
                <button className="button secondary-button" type="button" disabled={isWithdrawing} onClick={() => setIsConfirming(false)}>Vazgeç</button>
              </div>
            </> : <button className="button danger-button" type="button" onClick={() => setIsConfirming(true)}>Başvuruyu geri çek</button>}
          </section>}
        </article>
      </>}
  </div>;
}
