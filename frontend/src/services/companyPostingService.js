import { apiRequest } from "./apiClient.js";

const basePath = '/api/companies/me/internship-postings';
export const getMyPostings = (signal, archived) => apiRequest(`${basePath}${archived === undefined ? '' : `?archived=${archived}`}`, { signal });
export const getCompanyPosting = (id, signal) => apiRequest(`${basePath}/${encodeURIComponent(id)}`, { signal });
export const createCompanyPosting = (body) => apiRequest(basePath, { method: 'POST', body: body });
export const updateCompanyPosting = (id, body) => apiRequest(`${basePath}/${encodeURIComponent(id)}`, { method: 'PUT', body: body });
