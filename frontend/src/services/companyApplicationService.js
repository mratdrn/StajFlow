import { apiRequest } from "./apiClient.js";

const path = '/api/companies/me/applications';

export function getIncomingApplications(postingId, signal) {
  const query = postingId ? `?${new URLSearchParams({ internshipPostingId: postingId })}` : '';
  return apiRequest(`${path}${query}`, { signal });
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
