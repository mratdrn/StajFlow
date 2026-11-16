import { useEffect, useState } from "react";
import { useSearchParams } from "react-router";
import { getInternshipPostings } from "../services/internshipPostingService.js";
import InternshipPostingCard from "../components/InternshipPostingCard.jsx";
import LoadingState from "../components/LoadingState.jsx";
import StatePanel from "../components/StatePanel.jsx";
import internshipJourneyImage from "../../logo/photo.png";

export default function InternshipPostingsPage() {
  const [postings, setPostings] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [attempt, setAttempt] = useState(0);
  const [parameters, setParameters] = useSearchParams();
  const filters = parameters.toString();

  useEffect(() => {
    document.title = 'Staj ilanları | StajFlow';
    const abortController = new AbortController();
    setIsLoading(true);
    setError(null);

    getInternshipPostings(abortController.signal, filters)
      .then((data) => {
        if (!abortController.signal.aborted) setPostings(data);
      })
      .catch((failure) => {
        if (!abortController.signal.aborted) setError(failure.message);
      })
      .finally(() => {
        if (!abortController.signal.aborted) setIsLoading(false);
      });

    return () => abortController.abort();
  }, [attempt, filters]);

  function searchPostings(event) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    const newItem = new URLSearchParams();
    for (const field of ['q', 'location', 'isRemote']) {
      const value = form.get(field)?.trim();
      if (value) newItem.set(field, value);
    }
    setParameters(newItem);
  }

  return (
    <>
      <section className="listing-intro">
        <div>
          <p className="eyebrow">GELECEĞİNE BİR ADIM</p>
          <h1>Güzel başlangıçlar.<br /><span>Büyük olanaklar.</span></h1>
          <p className="intro-copy">Merakını deneyime dönüştüreceğin stajı bul.<br className="desktop-break" /> İlanları incele, geleceğine yön ver.</p>
        </div>
        <div className="listing-illustration">
          <img src={internshipJourneyImage} alt="Staj yolculuğunda basamakları çıkarak ilerleyen öğrenciler." width="1774" height="887" />
        </div>
      </section>

      <section className="opportunities" aria-labelledby="opportunities-heading">
        <form className="posting-search-form" onSubmit={searchPostings} key={filters} aria-label="İlan arama ve filtreleme">
          <div className="form-field"><label htmlFor="posting-search">Anahtar kelime</label>
            <input id="posting-search" name="q" maxLength={200} defaultValue={parameters.get('q') || ''} placeholder="İlan, şirket veya beceri" /></div>
          <div className="form-field"><label htmlFor="location-search">Konum</label>
            <input id="location-search" name="location" maxLength={250} defaultValue={parameters.get('location') || ''} placeholder="Şehir veya konum" /></div>
          <div className="form-field"><label htmlFor="work-arrangement">Çalışma biçimi</label>
            <select id="work-arrangement" name="isRemote" defaultValue={parameters.get('isRemote') || ''}>
              <option value="">Tümü</option><option value="true">Uzaktan</option><option value="false">İş yerinde</option>
            </select></div>
          <div className="search-actions"><button className="button primary-action" type="submit">İlan ara</button>
            <button className="button secondary-button" type="button" onClick={() => setParameters({})}>Temizle</button></div>
        </form>
        <div className="section-heading">
          <h2 id="opportunities-heading">Staj ilanları</h2>
          {!isLoading && !error && <span className="result-count">{postings.length} ilan</span>}
        </div>
        {isLoading ? <LoadingState /> : error ? (
          <StatePanel title="Staj ilanları yüklenemedi" isError onRetry={() => setAttempt((value) => value + 1)}>{error}</StatePanel>
        ) : postings.length === 0 ? (
          filters ? <StatePanel title="Aramanıza uygun ilan bulunamadı">Anahtar kelimeyi veya filtreleri değiştirerek tekrar deneyebilirsiniz.</StatePanel> :
          <StatePanel title="Yeni staj ilanları yolda">Şu anda yayımlanmış staj ilanı bulunmuyor. Yeni fırsatlar için daha sonra tekrar göz atabilirsin.</StatePanel>
        ) : (
          <div className="posting-grid">{postings.map((posting) => <InternshipPostingCard key={posting.id} posting={posting} />)}</div>
        )}
      </section>
      <p className="listing-footnote">Yakından incele. Her staj, yeni bir öğrenme fırsatı.</p>
    </>
  );
}

