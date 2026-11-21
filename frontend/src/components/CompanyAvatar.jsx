import { useState } from "react";
import { companyInitials } from "./formatters.js";
import { apiAssetUrl } from "../services/apiClient.js";

export default function CompanyAvatar({ name, logoUrl, className = '' }) {
  const [failedUrl, setFailedUrl] = useState(null);
  const source = apiAssetUrl(logoUrl);
  return <span className={`company-avatar ${className}`} aria-hidden="true">
    {source && source !== failedUrl ? <img src={source} alt="" loading="lazy" decoding="async"
      onError={() => setFailedUrl(source)} /> : companyInitials(name)}
  </span>;
}
