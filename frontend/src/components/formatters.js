const dateFormatter = new Intl.DateTimeFormat('tr-TR', {
  day: 'numeric', month: 'short', year: 'numeric',
});

export function formatDate(value, fallbackText = 'Belirtilmemiş') {
  if (!value) return fallbackText;
  const date = /^\d{4}-\d{2}-\d{2}$/.test(value)
    ? new Date(`${value}T00:00:00`)
    : new Date(value);
  return Number.isNaN(date.getTime()) ? fallbackText : dateFormatter.format(date);
}

export function locationLabel(posting) {
  return posting.location || (posting.isRemote ? 'Uzaktan' : 'Konum belirtilmemiş');
}

export function companyInitials(name) {
  return name.replace(/\([^)]*\)/g, '').trim().split(/\s+/).slice(0, 2)
    .map((word) => word[0]).join('').toLocaleUpperCase('tr-TR');
}

export function roleName(role) {
  return role === 'Student' ? 'Öğrenci' : role === 'Company' ? 'Şirket' : 'Kullanıcı';
}

export function postingStatusLabel(status) {
  return { Draft: 'Taslak', Published: 'Yayında', Closed: 'Kapalı' }[status] || 'Bilinmeyen durum';
}

export function applicationStatusLabel(status) {
  return { Submitted: 'Gönderildi', UnderReview: 'İnceleniyor', Shortlisted: 'Kısa listeye alındı',
    Accepted: 'Kabul edildi', Rejected: 'Reddedildi', Withdrawn: 'Geri çekildi' }[status] || 'Bilinmeyen durum';
}

