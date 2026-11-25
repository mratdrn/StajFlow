import { useId } from "react";
import { compatibilityScore } from "./compatibilityLabels.js";

const criterionGroups = [
  ['Required', true, 'Eşleşen zorunlu kriterler'],
  ['Required', false, 'CV’de kanıt bulunmayan zorunlu kriterler'],
  ['Preferred', true, 'Eşleşen tercih edilen kriterler'],
  ['Preferred', false, 'CV’de kanıt bulunmayan tercih edilen kriterler'],
];

const socialLabels = {
  EvidenceFound: 'Somut kanıt',
  DeclarationOnly: 'Yalnızca beyan',
  NoEvidence: 'Kanıt bulunamadı',
};

function Evidence({ items }) {
  return items?.map((item, index) => <div className="compatibility-evidence" key={index}>
    {item.text && <p>{item.text}</p>}
    {item.sourceQuote && <blockquote>“{item.sourceQuote}”</blockquote>}
  </div>);
}

function UnavailableResult({ result, snapshot }) {
  switch (result?.unavailableReason) {
    case 'AnalysisPending':
      return snapshot ? <p>Başvuru sırasında gerekli analiz henüz tamamlanmamıştı.</p>
        : <p role="status"><strong>Analiz devam ediyor.</strong> Analiz tamamlandığında uyumluluk sonucu güncellenir.</p>;
    case 'AnalysisFailed':
      return <p>{snapshot ? 'Başvuru sırasında gerekli analiz tamamlanamamıştı.' : 'CV veya ilan analizi başarısız oldu.'}</p>;
    case 'NoMeasurableCriteria':
      return <p>İlanda puanlanabilecek teknik beceri, proje, deneyim veya eğitim kriteri bulunamadı.</p>;
    default:
      return <p>{snapshot
        ? result ? 'Başvuru sırasında gerekli analiz bilgileri bulunmuyordu.' : 'Bu başvuru için kayıtlı bir değerlendirme bulunmuyor.'
        : result?.resumeId ? 'Güncel CV veya ilan analizi bulunamadı.' : 'Uyumluluğu görmek için bir PDF özgeçmiş yükleyin.'}</p>;
  }
}

export default function CompatibilityResult({ result, snapshot = false, children }) {
  const heading = useId();
  const score = compatibilityScore(result);
  const criteria = snapshot ? result?.data?.criteria || [] : [...(result?.required || []), ...(result?.preferred || [])];
  const warnings = snapshot ? result?.data?.mandatoryWarnings : result?.mandatoryWarnings;
  const social = snapshot ? result?.data?.socialIndicators : result?.socialIndicators;

  return <section className="compatibility-panel" aria-labelledby={heading}>
    <h2 id={heading}>{snapshot ? 'Başvuru anındaki uyumluluk' : 'İlanla uyumluluğunuz'}</h2>
    {score !== null ? <p className="compatibility-score">{score}</p>
      : <p className="compatibility-unavailable">Uyumluluk hesaplanamadı</p>}
    {warnings?.map((warning, index) => <p className="compatibility-warning" key={index}>{warning.message}</p>)}
    {score === null && <UnavailableResult result={result} snapshot={snapshot} />}
    {children}
    {criteria.length > 0 && <div className="compatibility-groups">
      {criterionGroups.map(([type, matched, title]) => {
        const items = criteria.filter((item) => item.type === type && (item.matchStatus === 'Matched') === matched);
        return <section className="compatibility-group" key={title}>
          <h3>{title}</h3>
          {!items.length ? <p className="muted">Bu grupta kriter yok.</p> : <ul>{items.map((item) => <li key={item.criterionId}>
            <strong>{item.description}</strong>
            <span className="muted">{matched ? 'Eşleşti' : 'CV’de doğrulanamadı'}</span>
            {(item.postingEvidence?.sourceQuote || item.evidence?.length > 0) && <details>
              <summary>Kaynak ve kanıt</summary>
              {item.postingEvidence?.sourceQuote && <blockquote>İlan: “{item.postingEvidence.sourceQuote}”</blockquote>}
              <Evidence items={item.evidence} />
            </details>}
          </li>)}</ul>}
        </section>;
      })}
    </div>}
    {social?.length > 0 && <section className="compatibility-social">
      <h3>Sosyal beceriler</h3>
      <p className="muted">Sosyal göstergeler uyumluluk puanına katılmaz.</p>
      <ul>{social.map((item, index) => <li key={`${item.name}-${index}`}>
        <strong>{item.name}</strong> <span className="badge">{socialLabels[item.evidenceStatus] || socialLabels.NoEvidence}</span>
        {item.evidenceStatus !== 'NoEvidence' && <Evidence items={item.evidence} />}
      </li>)}</ul>
    </section>}
  </section>;
}
