import { Link } from "react-router";
import Icon from "./Icon.jsx";

export default function StatePanel({ title, children, onRetry, isError = false, backLink = false }) {
  return (
    <section className="state-panel" role={isError ? 'alert' : 'status'}>
      <span className="state-icon"><Icon name="briefcase" /></span>
      <h2>{title}</h2>
      <p>{children}</p>
      {onRetry && <button className="button" type="button" onClick={onRetry}>Tekrar dene <Icon name="arrow" /></button>}
      {backLink && <Link className="button" to="/staj-ilanlari">Staj ilanları <Icon name="arrow" /></Link>}
    </section>
  );
}

