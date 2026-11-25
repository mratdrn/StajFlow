import { apiRequest } from "./apiClient.js";

const path = '/api/companies/me/applications';

export function getIncomingApplications(postingId, signal, page, status = '', filters = {}) {
  const query = new URLSearchParams();
  if (postingId) query.set('internshipPostingId', postingId);
  if (page !== undefined) query.set('page', page);
  if (status) query.set('status', status);
  for (const name of ['sort', 'scoreStatus', 'minScore', 'maxScore', 'requiredCriteriaStatus']) {
    if (filters[name] !== undefined && filters[name] !== '') query.set(name, filters[name]);
  }
  return apiRequest(`${path}${query.size ? `?${query}` : ''}`, { signal });
}

export function getIncomingApplication(id, signal) {
  return apiRequest(`${path}/${encodeURIComponent(id)}`, { signal });
}

export function updateApplicationStatus(id, status, rowVersion, signal) {
  return apiRequest(`${path}/${encodeURIComponent(id)}/status`, { method: 'PUT', body: { status: status, rowVersion: rowVersion }, signal });
}

export function getApplicationResume(id, signal) {
  return apiRequest(`${path}/${encodeURIComponent(id)}/resume`, { signal, fileResponse: true });
}
