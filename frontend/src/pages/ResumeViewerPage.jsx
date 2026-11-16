import { useEffect, useState } from "react";
import { Link, useParams } from "react-router";
import { getResumeFile } from "../services/resumeService.js";
import { getApplicationResume } from "../services/companyApplicationService.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function ResumeViewerPage({ companyApplication = false }) {
  const { id } = useParams();
  const [address, setObjectUrl] = useState('');
  const [error, setError] = useState(null);
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    document.title = 'Özgeçmiş görüntüleme | StajFlow';
    const abort = new AbortController();
    let objectUrl;
    setObjectUrl('');
    setError(null);
    if (!/^[1-9]\d*$/.test(id)) { setError({ status: 404 }); return () => abort.abort(); }
    const getFile = companyApplication ? getApplicationResume : getResumeFile;
    getFile(id, abort.signal).then((file) => {
      if (!abort.signal.aborted) {
        objectUrl = URL.createObjectURL(file);
        setObjectUrl(objectUrl);
      }
    }).catch((failure) => { if (!abort.signal.aborted) setError(failure); });
    return () => { abort.abort(); if (objectUrl) URL.revokeObjectURL(objectUrl); };
  }, [id, attempt, companyApplication]);

  return <>
    <Link className="back-link" to={companyApplication ? `/sirket/basvurular/${id}` : '/ogrenci/ozgecmisler'}>{companyApplication ? '← Başvuruya dön' : '← Özgeçmişlerime dön'}</Link>
    <h1 className="page-title">Özgeçmiş görüntüleme</h1>
    {error ? error.status === 404 ? <StatePanel title="Özgeçmiş bulunamadı">Bu dosya mevcut değil veya görüntüleme yetkiniz yok.</StatePanel> :
      <StatePanel title="Özgeçmiş açılamadı" isError onRetry={() => setAttempt((value) => value + 1)}>{error.message}</StatePanel> : !address ? <LoadingState /> : <>
        <p className="muted">Tarayıcınız PDF önizlemesini desteklemiyorsa <a className="text-link" href={address} download="ozgecmis.pdf">PDF dosyasını indirebilirsiniz.</a></p>
        <iframe className="resume-preview" title="PDF özgeçmiş önizlemesi" src={address} />
      </>}
  </>;
}
