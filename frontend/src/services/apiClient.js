const apiBaseAddress = import.meta.env.VITE_API_BASE_URL?.trim().replace(/\/+$/, '');

class ApiError extends Error {
  constructor(message, status, errors = {}) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.errors = errors;
  }
}

export async function apiRequest(path, { method = 'GET', body, signal, fileResponse = false } = {}) {
  if (!apiBaseAddress) throw new ApiError('Hizmete şu anda erişilemiyor. Daha sonra tekrar deneyin.', 0);
  const headers = { Accept: 'application/json' };
  const isFormData = body instanceof FormData;
  if (body !== undefined && !isFormData) headers['Content-Type'] = 'application/json';

  if (!['GET', 'HEAD', 'OPTIONS'].includes(method)) {
    const { requestToken } = await apiRequest('/api/auth/csrf', { signal });
    headers['X-CSRF-TOKEN'] = requestToken;
  }

  let response;
  try {
    response = await fetch(`${apiBaseAddress}${path}`, {
      method, headers, credentials: 'include', cache: 'no-store',
      body: body === undefined ? undefined : isFormData ? body : JSON.stringify(body), signal,
    });
  } catch (error) {
    if (error.name === 'AbortError') throw error;
    throw new ApiError('Sunucuya bağlanılamadı. Lütfen tekrar deneyin.', 0);
  }
  if (response.status === 204) return null;
  if (response.ok && fileResponse) {
    if (!response.headers.get('Content-Type')?.startsWith('application/pdf'))
      throw new ApiError('Sunucudan geçerli bir PDF alınamadı.', response.status);
    return response.blob();
  }
  const responseData = await response.json().catch(() => null);
  if (!response.ok) {
    if (response.status === 401) window.dispatchEvent(new Event('auth:unauthorized'));
    throw new ApiError(
      responseData?.message || (response.status === 413 ? 'Dosya yükleme sınırı aşıldı. Daha küçük bir dosya seçin.' :
        response.status === 400 ? 'Lütfen aşağıdaki bilgileri kontrol edip tekrar deneyin.' :
        response.status === 403 ? 'Bu sayfaya erişim yetkiniz yok.' :
        response.status === 404 ? 'İstenen kayıt bulunamadı.' : 'İşlem tamamlanamadı. Lütfen tekrar deneyin.'),
      response.status, responseData?.errors || {},
    );
  }
  if (responseData === null) throw new ApiError('Sunucudan beklenmeyen bir yanıt alındı. Lütfen tekrar deneyin.', response.status);
  return responseData;
}


export function apiAssetUrl(path) {
  return apiBaseAddress && path?.startsWith('/api/') ? `${apiBaseAddress}${path}` : null;
}
