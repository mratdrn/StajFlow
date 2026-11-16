import { apiRequest } from "./apiClient.js";

const applicationPath = '/api/students/me/applications';

export function getMyApplications(signal) {
  return apiRequest(applicationPath, { signal });
}

export function getMyApplication(id, signal) {
  return apiRequest(`${applicationPath}/${encodeURIComponent(id)}`, { signal });
}

export function submitApplication(request, signal) {
  return apiRequest(applicationPath, { method: 'POST', body: request, signal });
}
