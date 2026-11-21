import { apiRequest } from "./apiClient.js";

const resumePath = '/api/students/me/resumes';

export function getResumes(signal) {
  return apiRequest(resumePath, { signal });
}

export function uploadResume(file, signal) {
  const form = new FormData();
  form.append('file', file);
  return apiRequest(resumePath, { method: 'POST', body: form, signal });
}

export function getResumeFile(id, signal) {
  return apiRequest(`${resumePath}/${encodeURIComponent(id)}/file`, { signal, fileResponse: true });
}

export function deleteResume(id, signal) {
  return apiRequest(`${resumePath}/${encodeURIComponent(id)}`, { method: 'DELETE', signal });
}
