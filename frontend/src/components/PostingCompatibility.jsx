import { useCallback } from "react";
import { Link } from "react-router";
import usePollingQuery from "../hooks/usePollingQuery.js";
import { getPostingCompatibility } from "../services/internshipPostingService.js";
import CompatibilityResult from "./CompatibilityResult.jsx";

function isPending(result) {
  return result?.unavailableReason === 'AnalysisPending';
}

export default function PostingCompatibility({ postingId, resumeId }) {
  const load = useCallback((signal) => getPostingCompatibility(postingId, resumeId, signal), [postingId, resumeId]);
  const { value: result, isLoading, failed, reload } = usePollingQuery(load, isPending);

  if (isLoading || failed) return <section className="compatibility-panel" aria-label="İlanla uyumluluğunuz">
    <h2>İlanla uyumluluğunuz</h2>
    {isLoading ? <p role="status">Uyumluluk sonucu yükleniyor…</p> : <>
      <p role="alert">Uyumluluk sonucu alınamadı. Başvuruya devam edebilirsiniz.</p>
      <button type="button" className="button secondary-button" onClick={reload}>Sonucu yenile</button>
    </>}
  </section>;

  return <CompatibilityResult result={result} compact>
    {(!result.resumeId || result.unavailableReason === 'AnalysisFailed' || result.unavailableReason === 'InsufficientData') &&
      <div className="compatibility-actions">
        <Link className="text-link" to="/ogrenci/ozgecmisler">Özgeçmişlerime git</Link>
        <button type="button" className="button secondary-button" onClick={reload}>Sonucu yenile</button>
      </div>}
  </CompatibilityResult>;
}
