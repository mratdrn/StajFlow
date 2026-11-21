import { useEffect, useRef, useState } from "react";
import { Link } from "react-router";
import { getResumes, uploadResume, deleteResume } from "../services/resumeService.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";

export default function ResumesPage() {
  const [data, setData] = useState(null);
  const [error, setError] = useState('');
  const [uploadError, setUploadError] = useState('');
  const [success, setSuccess] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [deletingId, setDeletingId] = useState(null);
  const [removeError, setRemoveError] = useState('');
  const removalAbort = useRef(null);
  const fileField = useRef(null);
  const uploadAbort = useRef(null);

  useEffect(() => {
    document.title = 'Özgeçmişlerim | StajFlow';
    const abort = new AbortController();
    setError('');
    setData(null);
    getResumes(abort.signal).then((result) => {
      if (!abort.signal.aborted) setData(result);
    }).catch((failure) => {
      if (!abort.signal.aborted) setError(failure.message);
    });
    return () => abort.abort();
  }, [attempt]);

  useEffect(() => () => { uploadAbort.current?.abort(); removalAbort.current?.abort(); }, []);

  async function upload(event) {
    event.preventDefault();
    setUploadError('');
    setSuccess('');
    const file = fileField.current.files[0];
    if (!file) { setUploadError('PDF dosyası seçin.'); return; }
    if (!file.name.toLowerCase().endsWith('.pdf') || file.type !== 'application/pdf') {
      setUploadError('Yalnızca PDF dosyaları yüklenebilir.'); return;
    }
    if (!file.size || file.size > data.maxFileSizeBytes) {
      setUploadError(`Boş olmayan, en fazla ${data.maxFileSizeBytes / 1024 / 1024} MB boyutunda bir PDF seçin.`); return;
    }
    const abort = new AbortController();
    uploadAbort.current = abort;
    setIsLoading(true);
    try {
      const result = await uploadResume(file, abort.signal);
      if (abort.signal.aborted) return;
      setData((previous) => ({ ...previous, resumes: [result, ...previous.resumes] }));
      fileField.current.value = '';
      setSuccess('Özgeçmişiniz yüklendi.');
    } catch (failure) {
      if (!abort.signal.aborted) setUploadError(failure.message);
    } finally { if (!abort.signal.aborted) setIsLoading(false); }
  }

  async function remove(resume) {
    if (deletingId !== null) return;
    const abort = new AbortController();
    removalAbort.current = abort;
    setDeletingId(resume.id);
    setRemoveError('');
    setSuccess('');
    try {
      await deleteResume(resume.id, abort.signal);
      if (abort.signal.aborted) return;
      setData((previous) => ({ ...previous, resumes: previous.resumes.filter((item) => item.id !== resume.id) }));
      setSuccess('Özgeçmişiniz kaldırıldı.');
    } catch (failure) {
      if (!abort.signal.aborted) setRemoveError(failure.message);
    } finally { if (!abort.signal.aborted) setDeletingId(null); }
  }

  return <>
    <p className="eyebrow">ÖĞRENCİ HESABI</p>
    <h1 className="page-title">Özgeçmişlerim</h1>
    <p className="muted">PDF özgeçmişlerinizi yükleyin, görüntüleyin ve kaldırın. Başvuruda kullandığınız özgeçmişler kaldırılamaz.</p>
    {success && <p className="success-notice" role="status">{success}</p>}
    {error ? <StatePanel title="Özgeçmişleriniz yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel> : !data ? <LoadingState /> : <>
      <form className="resume-upload" onSubmit={upload}>
        <h2>PDF özgeçmiş yükle</h2>
        <div className="form-field"><label htmlFor="resume-file">PDF dosyası</label>
          <input ref={fileField} id="resume-file" type="file" accept=".pdf,application/pdf" disabled={isLoading || deletingId !== null} aria-describedby="file-size-limit" />
          <p className="field-hint" id="file-size-limit">En fazla {data.maxFileSizeBytes / 1024 / 1024} MB. Her yükleme ayrı bir özgeçmiş olarak saklanır.</p></div>
        {uploadError && <p className="form-alert" role="alert">{uploadError}</p>}
        <button className="button primary-action" disabled={isLoading || deletingId !== null} type="submit">{isLoading ? 'Yükleniyor…' : 'Özgeçmiş yükle'}</button>
      </form>
      <section aria-labelledby="resume-list"><h2 id="resume-list">Yüklediğiniz özgeçmişler</h2>
        {removeError && <p className="form-alert" role="alert">{removeError}</p>}
        {data.resumes.length === 0 ? <StatePanel title="Henüz özgeçmişiniz yok">İlk PDF özgeçmişinizi yukarıdaki alandan yükleyebilirsiniz.</StatePanel> :
          <div className="resume-list">{data.resumes.map((resume) => <article className="resume-card" key={resume.id}>
            <div><h3>{resume.originalFileName}</h3><p className="muted">{(resume.fileSizeBytes / 1024).toLocaleString('tr-TR', { maximumFractionDigits: 1 })} KB · {new Date(resume.uploadedAt).toLocaleString('tr-TR')}</p></div>
            <div className="resume-card-actions">
              <Link className={`button${deletingId === resume.id ? ' disabled-link' : ''}`}
                aria-disabled={deletingId === resume.id} aria-label={`Görüntüle: ${resume.originalFileName}`}
                onClick={(event) => { if (deletingId === resume.id) event.preventDefault(); }}
                to={`/ogrenci/ozgecmisler/${resume.id}`}>Görüntüle</Link>
              <button className="button danger-button" type="button" disabled={isLoading || deletingId !== null}
                aria-label={`Kaldır: ${resume.originalFileName}`} onClick={() => remove(resume)}>
                {deletingId === resume.id ? 'Kaldırılıyor…' : 'Kaldır'}</button>
            </div>
          </article>)}</div>}
      </section>
    </>}
  </>;
}
