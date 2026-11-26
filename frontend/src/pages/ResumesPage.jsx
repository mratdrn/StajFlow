import { useEffect, useRef, useState } from "react";
import { Link } from "react-router";
import { getResumes, uploadResume, deleteResume, setFavoriteResume } from "../services/resumeService.js";
import StatePanel from "../components/StatePanel.jsx";
import LoadingState from "../components/LoadingState.jsx";
import ResumeAnalysisStatus from "../components/ResumeAnalysisStatus.jsx";
import Icon from "../components/Icon.jsx";

export default function ResumesPage() {
  const [data, setData] = useState(null);
  const [error, setError] = useState('');
  const [uploadError, setUploadError] = useState('');
  const [success, setSuccess] = useState('');
  const [attempt, setAttempt] = useState(0);
  const [isLoading, setIsLoading] = useState(false);
  const [deletingId, setDeletingId] = useState(null);
  const [removeError, setRemoveError] = useState('');
  const [favoritingId, setFavoritingId] = useState(null);
  const [favoriteError, setFavoriteError] = useState('');
  const favoriteAbort = useRef(null);
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

  useEffect(() => () => { uploadAbort.current?.abort(); removalAbort.current?.abort(); favoriteAbort.current?.abort(); }, []);

  const isBusy = isLoading || deletingId !== null || favoritingId !== null;

  async function favorite(resume) {
    if (isBusy || favoriteAbort.current || resume.isFavorite) return;
    const abort = new AbortController();
    favoriteAbort.current = abort;
    setFavoritingId(resume.id);
    setFavoriteError('');
    setSuccess('');
    try {
      const result = await setFavoriteResume(resume.id, abort.signal);
      if (abort.signal.aborted) return;
      setData(result);
      setSuccess('Favori özgeçmişiniz güncellendi.');
    } catch {
      if (!abort.signal.aborted) setFavoriteError('Favori özgeçmiş değiştirilemedi. Listeyi yenileyip tekrar deneyin.');
    } finally {
      if (!abort.signal.aborted) { setFavoritingId(null); favoriteAbort.current = null; }
    }
  }

  async function upload(event) {
    event.preventDefault();
    if (isBusy) return;
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
    if (isBusy) return;
    const abort = new AbortController();
    removalAbort.current = abort;
    setDeletingId(resume.id);
    setRemoveError('');
    setSuccess('');
    try {
      await deleteResume(resume.id, abort.signal);
      if (abort.signal.aborted) return;
      setData((previous) => ({ ...previous, resumes: previous.resumes.filter((item) => item.id !== resume.id) }));
      setAttempt((value) => value + 1);
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
          <input ref={fileField} id="resume-file" type="file" accept=".pdf,application/pdf" disabled={isBusy} aria-describedby="file-size-limit" />
          <p className="field-hint" id="file-size-limit">En fazla {data.maxFileSizeBytes / 1024 / 1024} MB. Her yükleme ayrı bir özgeçmiş olarak saklanır.</p></div>
        {uploadError && <p className="form-alert" role="alert">{uploadError}</p>}
        <button className="button primary-action" disabled={isBusy} type="submit">{isLoading ? 'Yükleniyor…' : 'Özgeçmiş yükle'}</button>
      </form>
      <section aria-labelledby="resume-list"><h2 id="resume-list">Yüklediğiniz özgeçmişler</h2>
        {removeError && <p className="form-alert" role="alert">{removeError}</p>}
        {favoriteError && <p className="form-alert" role="alert">{favoriteError}</p>}
        {data.resumes.length === 0 ? <StatePanel title="Henüz özgeçmişiniz yok">İlk PDF özgeçmişinizi yukarıdaki alandan yükleyebilirsiniz.</StatePanel> :
          <div className="resume-list">{data.resumes.map((resume) => <article className="resume-card" key={resume.id}>
            <div><h3>{resume.originalFileName}</h3><p className="muted">{(resume.fileSizeBytes / 1024).toLocaleString('tr-TR', { maximumFractionDigits: 1 })} KB · {new Date(resume.uploadedAt).toLocaleString('tr-TR')}</p>
              <ResumeAnalysisStatus resumeId={resume.id} disabled={isBusy} /></div>
            <div className="resume-card-actions">
              <button className={`resume-favorite-button${resume.isFavorite ? ' is-favorite' : ''}`} type="button" disabled={isBusy}
                aria-pressed={resume.isFavorite} aria-busy={favoritingId === resume.id}
                aria-label={`${resume.isFavorite ? 'Favori özgeçmiş' : 'Favori yap'}: ${resume.originalFileName}`}
                title={favoritingId === resume.id ? 'Favori güncelleniyor…' : resume.isFavorite ? 'Favori özgeçmiş' : 'Favori yap'}
                onClick={() => favorite(resume)}><Icon name="heart" /></button>
              <Link className={`button${deletingId === resume.id ? ' disabled-link' : ''}`}
                aria-disabled={deletingId === resume.id} aria-label={`Görüntüle: ${resume.originalFileName}`}
                onClick={(event) => { if (deletingId === resume.id) event.preventDefault(); }}
                to={`/ogrenci/ozgecmisler/${resume.id}`}>Görüntüle</Link>
              <button className="button danger-button" type="button" disabled={isBusy}
                aria-label={`Kaldır: ${resume.originalFileName}`} onClick={() => remove(resume)}>
                {deletingId === resume.id ? 'Kaldırılıyor…' : 'Kaldır'}</button>
            </div>
          </article>)}</div>}
      </section>
    </>}
  </>;
}
