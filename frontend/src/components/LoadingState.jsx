export default function LoadingState({ detail = false }) {
  return (
    <div role="status" aria-live="polite" aria-busy="true">
      <p className="loading-label">{detail ? 'İlan ayrıntıları yükleniyor…' : 'Staj ilanları yükleniyor…'}</p>
      <div className={detail ? undefined : 'posting-grid'} aria-hidden="true">
        {Array.from({ length: detail ? 1 : 2 }, (_, index) => (
          <div className="skeleton-card" key={index}>
            <div className="skeleton skeleton-badge" />
            <div className="skeleton skeleton-title" />
            <div className="skeleton skeleton-line" />
            <div className="skeleton skeleton-line short" />
          </div>
        ))}
      </div>
    </div>
  );
}

