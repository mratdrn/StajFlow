export default function Pagination({ page, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;
  const pages = (totalPages <= 7 ? Array.from({ length: totalPages }, (_, index) => index + 1)
    : [...new Set([1, page - 1, page, page + 1, totalPages])])
    .filter((number) => number >= 1 && number <= totalPages).sort((a, b) => a - b);
  return <nav className="pagination" aria-label="Sayfalar">
    <button className="button secondary-button" type="button" disabled={page === 1}
      onClick={() => onPageChange(page - 1)}>Önceki</button>
    <div className="pagination-numbers">{pages.map((number, index) => <span className="pagination-item" key={number}>
      {index > 0 && number > pages[index - 1] + 1 && <span className="pagination-gap" aria-hidden="true">…</span>}
      <button className={`button${number === page ? '' : ' secondary-button'}`} type="button"
        aria-label={`${number}. sayfa`} aria-current={number === page ? 'page' : undefined}
        onClick={() => { if (number !== page) onPageChange(number); }}>{number}</button>
    </span>)}</div>
    <button className="button secondary-button" type="button" disabled={page === totalPages}
      onClick={() => onPageChange(page + 1)}>Sonraki</button>
  </nav>;
}
