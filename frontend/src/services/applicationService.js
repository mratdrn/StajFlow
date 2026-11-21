import { apiRequest } from "./apiClient.js";

const applicationPath = '/api/students/me/applications';

export function getMyApplications(signal, page, status = '') {
  const query = new URLSearchParams();
  if (page !== undefined) query.set('page', page);
  if (status) query.set('status', status);
  return apiRequest(`${applicationPath}${query.size ? `?${query}` : ''}`, { signal });
}

export function getMyApplication(id, signal) {
  return apiRequest(`${applicationPath}/${encodeURIComponent(id)}`, { signal });
}

export function submitApplication(request, signal) {
  return apiRequest(applicationPath, { method: 'POST', body: request, signal });
}

export const withdrawApplication = (id, signal) => apiRequest(`/api/students/me/applications/${id}/withdraw`, { method: 'POST', signal });

export function getMyApplicationPosting(id, signal) {
  return apiRequest(`${applicationPath}/${encodeURIComponent(id)}/posting`, { signal });
}
