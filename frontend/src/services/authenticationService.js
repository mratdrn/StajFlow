import { apiRequest } from "./apiClient.js";

export const getCurrentUser = (signal) => apiRequest('/api/auth/me', { signal });
export const login = (body) => apiRequest('/api/auth/login', { method: 'POST', body });
export const logout = () => apiRequest('/api/auth/logout', { method: 'POST' });
export const registerStudent = (body) => apiRequest('/api/auth/register/student', { method: 'POST', body });
export const registerCompany = (body) => apiRequest('/api/auth/register/company', { method: 'POST', body });
export const getMyProfile = (role, signal) => apiRequest(
  role === 'Student' ? '/api/students/me' : '/api/companies/me', { signal },
);

export const updateMyProfile = (role, body) => apiRequest(
  role === 'Student' ? '/api/students/me' : '/api/companies/me', { method: 'PUT', body: body },
);

