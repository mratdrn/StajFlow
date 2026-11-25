const percentage = new Intl.NumberFormat('tr-TR', { style: 'percent', maximumFractionDigits: 2 });

export const requiredCriteriaLabels = {
  AllMatched: 'Tüm zorunlu kriterler doğrulandı',
  MissingEvidence: 'Zorunlu kriterlerde kanıt eksik',
  NoRequiredCriteria: 'Zorunlu kriter yok',
  Unavailable: 'Zorunlu kriter durumu belirlenemedi',
};

export function compatibilityScore(result) {
  return result?.status === 'Calculated' && Number.isFinite(result.score) ? percentage.format(result.score / 100) : null;
}
