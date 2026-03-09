/**
 * Extracts a user-facing error message from an HTTP or other error.
 * Prefers API response body message, then generic message/statusText.
 */
export function getErrorMessage(err: unknown, fallback: string): string {
  if (err && typeof err === 'object' && 'error' in err) {
    const body = (err as { error?: { message?: string }; message?: string }).error;
    const msg = body && typeof body === 'object' && 'message' in body
      ? (body as { message?: string }).message
      : (err as { message?: string }).message;
    if (typeof msg === 'string' && msg.length > 0) return msg;
  }
  if (err && typeof err === 'object' && 'message' in err && typeof (err as { message: unknown }).message === 'string')
    return (err as { message: string }).message;
  return fallback;
}
