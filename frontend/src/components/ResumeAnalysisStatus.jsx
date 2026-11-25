import { useCallback, useEffect, useRef, useState } from "react";
import usePollingQuery from "../hooks/usePollingQuery.js";
import { getResumeAnalysis, requestResumeAnalysis } from "../services/resumeService.js";

const labels = {
  Pending: 'Bekliyor',
  Processing: 'Analiz ediliyor',
  Completed: 'Tamamlandı',
  Failed: 'Başarısız',
};

const documentErrors = {
  InvalidPdf: 'PDF geçersiz veya bozuk. Okunabilir bir PDF yükleyin.',
  EncryptedPdf: 'PDF parola korumalı. Şifresiz bir PDF yükleyin.',
  NoText: 'PDF’de seçilebilir metin bulunamadı. Metin içeren bir PDF yükleyin.',
  UnreadableText: 'PDF metni okunamadı. Okunabilir bir PDF yükleyin.',
  FileTooLarge: 'PDF boyutu analiz sınırını aşıyor. Daha küçük bir PDF yükleyin.',
  FileNotFound: 'PDF dosyası bulunamadı. Özgeçmişinizi tekrar yükleyin.',
  InputTooLong: 'Özgeçmiş metni analiz için çok uzun. Daha kısa bir PDF yükleyin.',
  ContentBlocked: 'Bu içerik analiz edilemedi. Özgeçmişinizi kontrol edin.',
};

function isActive(analysis) {
  return analysis?.status === 'Pending' || analysis?.status === 'Processing';
}

export default function ResumeAnalysisStatus({ resumeId, disabled }) {
  const load = useCallback(async (signal) => {
    try {
      return await getResumeAnalysis(resumeId, signal);
    } catch (error) {
      if (error.status === 404) return null;
      throw error;
    }
  }, [resumeId]);
  const { value: analysis, isLoading, failed, reload } = usePollingQuery(load, isActive);
  const request = useRef(null);
  const [requestError, setRequestError] = useState('');
  const [isRequesting, setIsRequesting] = useState(false);

  useEffect(() => () => request.current?.abort(), []);

  async function startAnalysis() {
    if (request.current || disabled) return;
    const abort = new AbortController();
    request.current = abort;
    setIsRequesting(true);
    setRequestError('');
    try {
      await requestResumeAnalysis(resumeId, abort.signal);
      if (!abort.signal.aborted) reload();
    } catch (error) {
      if (!abort.signal.aborted) {
        if (error.status === 409) reload();
        else setRequestError('Analiz başlatılamadı. Daha sonra tekrar deneyin.');
      }
    } finally {
      if (!abort.signal.aborted) {
        request.current = null;
        setIsRequesting(false);
      }
    }
  }

  const label = isLoading ? 'Durum yükleniyor…' : failed ? 'Durum alınamadı'
    : analysis ? labels[analysis.status] || 'Durum bilinmiyor' : 'Henüz analiz edilmedi';
  const documentError = analysis?.status === 'Failed' ? documentErrors[analysis.errorCode] : null;
  const canStart = !isLoading && !failed && (!analysis || (analysis.status === 'Failed' && !documentError));
  const failureMessage = analysis?.errorCode === 'QuotaExceeded'
    ? 'Analiz kullanım sınırına ulaşıldı. Daha sonra yeniden deneyebilirsiniz.'
    : analysis?.errorCode === 'ProviderUnavailable'
    ? 'Analiz hizmetinden yanıt alınamadı. Daha sonra yeniden deneyebilirsiniz. Özgeçmişiniz başvuruda kullanılabilir.'
    : analysis?.errorCode === 'InvalidResponse'
    ? 'Analiz yanıtı doğrulanamadı. Yeniden analiz edebilirsiniz. Özgeçmişiniz başvuruda kullanılabilir.'
    : 'Analiz tamamlanamadı. Özgeçmişiniz yüklü ve başvuruda kullanılabilir.';

  return <div className="resume-analysis">
    <div className="resume-analysis-label" role="status"><span>CV analizi</span><span className="badge">{label}</span></div>
    {analysis?.status === 'Failed' && <p className="muted">{documentError || failureMessage}</p>}
    {requestError && <p className="muted" role="alert">{requestError}</p>}
    {failed && <button className="button secondary-button" type="button" onClick={() => { setRequestError(''); reload(); }} disabled={disabled}>Durumu yenile</button>}
    {canStart && <button className="button secondary-button" type="button" onClick={startAnalysis} disabled={disabled || isRequesting}>
      {isRequesting ? 'İstek gönderiliyor…' : analysis ? 'Yeniden analiz et' : 'Analizi başlat'}
    </button>}
  </div>;
}
