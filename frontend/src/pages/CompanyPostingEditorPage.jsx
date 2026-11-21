import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router";
import { getCompanyPosting } from "../services/companyPostingService.js";
import CompanyPostingForm from "../components/CompanyPostingForm.jsx";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function CompanyPostingEditorPage() {
  const { id } = useParams();
  const [posting, setPosting] = useState(null);
  const [isLoading, setIsLoading] = useState(Boolean(id));
  const [error, setError] = useState(null);
  const [attempt, setAttempt] = useState(0);
  const navigate = useNavigate();

  useEffect(() => {
    document.title = `${id ? 'İlanı düzenle' : 'Yeni staj ilanı'} | StajFlow`;
    if (!id) { setPosting(null); setError(null); setIsLoading(false); return; }
    const abort = new AbortController();
    setIsLoading(true);
    setError(null);
    if (!/^\d+$/.test(id) || Number(id) < 1 || Number(id) > 2147483647) {
      setError({ status: 404, message: 'İlan bulunamadı.' });
      setIsLoading(false);
      return () => abort.abort();
    }
    getCompanyPosting(id, abort.signal).then((data) => {
      if (!abort.signal.aborted) setPosting(data);
    }).catch((failure) => { if (!abort.signal.aborted) setError(failure); })
      .finally(() => { if (!abort.signal.aborted) setIsLoading(false); });
    return () => abort.abort();
  }, [id, attempt]);

  return <div className="company-editor-page">
    <Link className="back-link" to="/sirket/ilanlar">← İlanlarıma dön</Link>
    <div className="posting-management-heading"><div><p className="eyebrow">ŞİRKET HESABI</p>
      <h1 className="page-title">{id ? 'İlanı düzenle' : 'Yeni staj ilanı'}</h1>
      <p className="muted">İlan bilgilerini, aradığınız nitelikleri ve yayın tercihlerinizi düzenleyin.</p></div></div>
    {isLoading ? <LoadingState detail /> : error ?
      <StatePanel title={error.status === 404 ? 'İlan bulunamadı' : 'İlan yüklenemedi'}
        isError onRetry={error.status === 404 ? undefined : () => setAttempt((value) => value + 1)}>
        {error.status === 404 ? 'Bu ilan mevcut değil veya şirketinize ait değil.' : error.message}
      </StatePanel> :
      <CompanyPostingForm key={posting ? `${posting.id}-${posting.rowVersion}` : 'yeni'} posting={id ? posting : null}
        reload={() => setAttempt((value) => value + 1)}
        onSaved={(status) => navigate(status === 'Draft' ? '/sirket/taslak-ilanlar' : '/sirket/ilanlar', { state: { success: id ? 'İlan ve nitelikler güncellendi.' : 'İlan oluşturuldu.' } })} />}
  </div>;
}
