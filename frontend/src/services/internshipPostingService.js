import { apiRequest } from "./apiClient.js";

export function getInternshipPostings(signal, filters = '') {
  return apiRequest(`/api/internship-postings${filters ? `?${filters}` : ''}`, { signal });
}

export function getInternshipPosting(id, signal) {
  return apiRequest(`/api/internship-postings/${encodeURIComponent(id)}`, { signal });
}

