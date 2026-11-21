import { apiRequest } from "./apiClient.js";

const path = '/api/companies/me/applications';

export function getIncomingApplications(postingId, signal, page, status = '') {
  const query = new URLSearchParams();
  if (postingId) query.set('internshipPostingId', postingId);
  if (page !== undefined) query.set('page', page);
  if (status) query.set('status', status);
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
