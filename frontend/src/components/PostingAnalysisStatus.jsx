import { useCallback, useEffect, useRef, useState } from "react";
import usePollingQuery from "../hooks/usePollingQuery.js";
import { getPostingAnalysis, requestPostingAnalysis } from "../services/companyPostingService.js";

const labels = { Pending: 'Bekliyor', Processing: 'Analiz ediliyor', Completed: 'Tamamlandı', Failed: 'Başarısız' };
const isActive = (analysis) => analysis?.status === 'Pending' || analysis?.status === 'Processing';

export default function PostingAnalysisStatus({ posting }) {
  const load = useCallback(async (signal) => {
    try { return await getPostingAnalysis(posting.id, signal); }
    catch (error) { if (error.status === 404) return null; throw error; }
  }, [posting.id]);
  const { value: analysis, isLoading, failed, reload } = usePollingQuery(load, isActive);
  const request = useRef(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  useEffect(() => () => request.current?.abort(), []);
  const open = posting.status === 'Published' && (!posting.applicationDeadline || new Date(posting.applicationDeadline) > new Date());
  const canStart = open && !isLoading && !failed && !isActive(analysis) && (!analysis || analysis.requiresReanalysis || analysis.status === 'Failed');

  async function start() {
    if (request.current || !canStart) return;
    const abort = new AbortController();
    request.current = abort;
    setBusy(true);
    setError('');
    try {
      await requestPostingAnalysis(posting.id, abort.signal);
      if (!abort.signal.aborted) reload();
    } catch {
      if (!abort.signal.aborted) setError('Analiz başlatılamadı. Durumu yenileyip tekrar deneyin.');
    } finally {
      if (!abort.signal.aborted) { request.current = null; setBusy(false); }
    }
  }

  return <section className="compatibility-panel">
    <h2>İlan analizi</h2>
    <p role="status">{isLoading ? 'Durum yükleniyor…' : failed ? 'Analiz durumu alınamadı.' : analysis?.requiresReanalysis ? 'Yeniden analiz gerekli' : labels[analysis?.status] || 'Henüz analiz edilmedi'}</p>
    {analysis?.requiresReanalysis && <p>Analiz şeması güncellendi. Yeni uyumluluk hesapları için yeniden analiz başlatın. Geçmiş başvuruların sonuçları değişmez.</p>}
    {analysis?.status === 'Failed' && <p>{analysis.message || 'Analiz tamamlanamadı. İlanınız ve başvuru alma durumu korunur.'}</p>}
    {!open && <p>Analiz yalnız yayındaki ve başvuruya açık ilan için başlatılabilir.</p>}
    {error && <p role="alert">{error}</p>}
    {canStart && <button type="button" className="button secondary-button" disabled={busy} onClick={start}>{busy ? 'İstek gönderiliyor…' : analysis ? 'Yeniden analiz et' : 'Analizi başlat'}</button>}
    {failed && <button type="button" className="button secondary-button" onClick={reload}>Durumu yenile</button>}
  </section>;
}
