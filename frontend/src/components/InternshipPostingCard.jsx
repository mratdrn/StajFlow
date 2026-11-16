import { Link } from "react-router";
import Icon from "./Icon.jsx";
import { formatDate, companyInitials, locationLabel } from "./formatters.js";

export default function InternshipPostingCard({ posting }) {
  return (
    <article className="posting-card">
      <div className="card-top">
        <span className="company-avatar" aria-hidden="true">{companyInitials(posting.companyName)}</span>
        <span className={`badge ${posting.isRemote ? 'badge-remote' : ''}`}>
          {posting.isRemote ? 'Uzaktan' : 'İş yerinde'}
        </span>
      </div>
      <p className="company-name">{posting.companyName}</p>
      <h3><Link to={`/staj-ilanlari/${posting.id}`}>{posting.title}</Link></h3>
      {posting.shortDescription && <p className="posting-short-description">{posting.shortDescription}</p>}
      <div className="card-meta">
        <span><Icon name="pin" />{locationLabel(posting)}</span>
        <span><Icon name="calendar" />{formatDate(posting.startDate, 'Başlangıç tarihi esnek')}</span>
      </div>
      <div className="card-footer">
        <span className="deadline"><Icon name="clock" />{posting.applicationDeadline ? `Son başvuru: ${formatDate(posting.applicationDeadline)}` : 'Son başvuru tarihi belirtilmemiş'}</span>
        <Link className="detail-link" to={`/staj-ilanlari/${posting.id}`} aria-label={`İlan ayrıntıları: ${posting.title}`}>Ayrıntıları gör <Icon name="arrow" /></Link>
      </div>
    </article>
  );
}

