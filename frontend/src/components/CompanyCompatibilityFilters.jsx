import { useState } from "react";
import { requiredCriteriaLabels } from "./compatibilityLabels.js";

const sortOptions = {
  ScoreDesc: 'Puan: yüksekten düşüğe',
  ScoreAsc: 'Puan: düşükten yükseğe',
};
const scoreOptions = { Calculated: 'Hesaplananlar', NotCalculated: 'Hesaplanamayanlar' };
export const compatibilityFilterKeys = ['sort', 'scoreStatus', 'requiredCriteriaStatus', 'minScore', 'maxScore'];

export function readCompatibilityFilters(parameters) {
  function readOption(name, options) {
    const value = parameters.get(name);
    return value && Object.hasOwn(options, value) ? value : '';
  }
  function readScore(name) {
    const value = parameters.get(name);
    return value && /^\d{1,3}(\.\d{1,2})?$/.test(value) && Number(value) <= 100 ? value : '';
  }
  const minScore = readScore('minScore');
  const maxScore = readScore('maxScore');
  const isRangeValid = !(minScore && maxScore && Number(minScore) > Number(maxScore));
  return {
    sort: readOption('sort', sortOptions),
    scoreStatus: readOption('scoreStatus', scoreOptions),
    requiredCriteriaStatus: readOption('requiredCriteriaStatus', requiredCriteriaLabels),
    minScore: isRangeValid ? minScore : '',
    maxScore: isRangeValid ? maxScore : '',
  };
}

function SelectField({ name, label, options, placeholder, value }) {
  const id = `company-application-${name}`;
  return <div className="form-field">
    <label htmlFor={id}>{label}</label>
    <select id={id} name={name} defaultValue={value}>
      <option value="">{placeholder}</option>
      {Object.entries(options).map(([key, text]) => <option key={key} value={key}>{text}</option>)}
    </select>
  </div>;
}

export default function CompanyCompatibilityFilters({ active, onApply, onClear }) {
  const [error, setError] = useState('');

  function apply(event) {
    event.preventDefault();
    setError('');
    const fields = new FormData(event.currentTarget);
    const next = Object.fromEntries(compatibilityFilterKeys.map((name) => [name, fields.get(name)]));
    if (next.minScore && next.maxScore && Number(next.minScore) > Number(next.maxScore)) {
      setError('En düşük puan, en yüksek puandan büyük olamaz.');
      return;
    }
    for (const name of ['minScore', 'maxScore']) {
      if (next[name] !== '') next[name] = String(Number(next[name]));
    }
    onApply(next);
  }

  return <form className="compatibility-filter-form" aria-label="Uyumluluk filtreleri" onSubmit={apply}>
    <div className="compatibility-filter-fields">
      <SelectField name="sort" label="Sıralama" options={sortOptions} placeholder="En yeni başvuru" value={active.sort} />
      <SelectField name="scoreStatus" label="Uyumluluk puanı" options={scoreOptions} placeholder="Tüm puan durumları" value={active.scoreStatus} />
      <SelectField name="requiredCriteriaStatus" label="Zorunlu kriter durumu" options={requiredCriteriaLabels}
        placeholder="Tüm kriter durumları" value={active.requiredCriteriaStatus} />
      <div className="form-field">
        <label htmlFor="min-compatibility-score">En düşük puan</label>
        <input id="min-compatibility-score" name="minScore" type="number" min="0" max="100" step="0.01" defaultValue={active.minScore} />
      </div>
      <div className="form-field">
        <label htmlFor="max-compatibility-score">En yüksek puan</label>
        <input id="max-compatibility-score" name="maxScore" type="number" min="0" max="100" step="0.01" defaultValue={active.maxScore} />
      </div>
    </div>
    {error && <p className="form-alert" role="alert">{error}</p>}
    <div className="compatibility-actions">
      <button className="button secondary-button" type="submit">Uyumluluk filtrelerini uygula</button>
      {Object.values(active).some(Boolean) && <button className="button secondary-button" type="button" onClick={onClear}>Uyumluluk filtrelerini temizle</button>}
    </div>
  </form>;
}
