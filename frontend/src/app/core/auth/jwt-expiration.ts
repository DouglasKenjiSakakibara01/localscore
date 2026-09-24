interface JwtPayload {
  exp?: unknown;
}

export function getTokenExpiration(token: string): Date | null {
  const parts = token.split('.');

  if (parts.length !== 3) {
    return null;
  }

  try {
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padding = '='.repeat((4 - (base64.length % 4)) % 4);
    const payload = JSON.parse(atob(base64 + padding)) as JwtPayload;

    if (typeof payload.exp !== 'number' || !Number.isFinite(payload.exp)) {
      return null;
    }

    return new Date(payload.exp * 1000);
  } catch {
    return null;
  }
}

export function isTokenUsable(token: string, now = new Date()): boolean {
  const expiration = getTokenExpiration(token);
  return expiration !== null && expiration.getTime() > now.getTime();
}
