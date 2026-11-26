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
      return <p>İlanda puanlanabilecek bir koşul bulunamadı.</p>;
    default:
      return <p>{snapshot
        ? result ? 'Başvuru sırasında gerekli analiz bilgileri bulunmuyordu.' : 'Bu başvuru için kayıtlı bir değerlendirme bulunmuyor.'
        : result?.resumeId ? 'Güncel şemaya uygun CV ve ilan analizi gerekiyor. İlgili analizleri yeniden başlatın.' : 'Uyumluluğu görmek için bir PDF özgeçmiş yükleyin.'}</p>;
  }
}

function CompactSummary({ result, score, criteria }) {
  if (score === null) return <p className="compatibility-unavailable">Uyumluluk hesaplanamadı</p>;

  const required = criteria.filter((item) => item.type === 'Required' && item.matchStatus === 'NoEvidence');
  const preferred = criteria.filter((item) => item.type === 'Preferred' && item.matchStatus === 'NoEvidence');
  const missing = [...new Set([...required, ...preferred].map((item) => item.description?.trim()).filter(Boolean))];
  const both = required.length > 0 && preferred.length > 0;
  const warning = both ? 'Kriterlerin tamamı CV’de doğrulanamadı.'
    : required.length ? 'Zorunlu kriterlerin tamamı CV’de doğrulanamadı.'
    : preferred.length ? 'Tercih edilen kriterlerin tamamı CV’de doğrulanamadı.' : null;
  const introduction = both ? 'İlan beklentilerinden' : required.length ? 'İlan gereksinimlerinden' : 'İlan tercihlerinden';
  const names = new Intl.ListFormat('tr-TR', { style: 'long', type: 'conjunction' }).format(missing);
  const explanation = missing.length
    ? `${introduction} ${names} için CV’nizde yeterli kanıt bulunamadı. Başvurunuzda bol şans!`
    : criteria.length
    ? 'İlanın zorunlu ve tercih edilen kriterlerinde doğrulanamayan bir koşul bulunmuyor. Başvurunuzda bol şans!'
    : result?.socialIndicators?.length
    ? 'Bu ilanda zorunlu veya tercih edilen kriter bulunmuyor. Uyumluluk puanı sosyal beceri beklentilerine göre hesaplandı. Başvurunuzda bol şans!'
    : 'İlanın değerlendirilen kriterleri için doğrulanamayan bir koşul bulunmuyor. Başvurunuzda bol şans!';

  return <>
    <div className="compatibility-score-summary">
      <p className="compatibility-score">{score}</p>
      {warning && <p className="compatibility-warning">{warning}</p>}
    </div>
    <p>{explanation}</p>
  </>;
}

export default function CompatibilityResult({ result, snapshot = false, compact = false, children }) {
  const heading = useId();
  const score = compatibilityScore(result);
  const criteria = snapshot ? result?.data?.criteria || [] : [...(result?.required || []), ...(result?.preferred || [])];
  const warnings = snapshot ? result?.data?.mandatoryWarnings : result?.mandatoryWarnings;
  const social = snapshot ? result?.data?.socialIndicators : result?.socialIndicators;

  return <section className="compatibility-panel" aria-labelledby={heading}>
    <h2 id={heading}>{snapshot ? 'Başvuru anındaki uyumluluk' : 'İlanla uyumluluğunuz'}</h2>
    {compact ? <CompactSummary result={result} score={score} criteria={criteria} /> : <>
      {score !== null ? <p className="compatibility-score">{score}</p>
        : <p className="compatibility-unavailable">Uyumluluk hesaplanamadı</p>}
      {warnings?.map((warning, index) => <p className="compatibility-warning" key={index}>{warning.message}</p>)}
    </>}
    {score === null && <UnavailableResult result={result} snapshot={snapshot} />}
    {children}
    {!compact && criteria.length > 0 && <div className="compatibility-groups">
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
    {!compact && social?.length > 0 && <section className="compatibility-social">
      <h3>Sosyal beceriler</h3>
      <p className="muted">{result?.scoringRuleVersion === '3'
        ? 'Sosyal beceriler 5 temel ağırlıkla puana katılır. Bu şema yalnız listelenen beceriyi gösterir; somut faaliyet kanıtı içermez.'
        : 'Bu değerlendirmede sosyal göstergeler uyumluluk puanına katılmaz.'}</p>
      <ul>{social.map((item, index) => <li key={`${item.name}-${index}`}>
        <strong>{item.name}</strong> <span className="badge">{socialLabels[item.evidenceStatus] || socialLabels.NoEvidence}</span>
        {item.evidenceStatus !== 'NoEvidence' && <Evidence items={item.evidence} />}
      </li>)}</ul>
    </section>}
  </section>;
}
