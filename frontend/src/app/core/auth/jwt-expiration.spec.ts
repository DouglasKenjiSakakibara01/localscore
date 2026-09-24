import { getTokenExpiration, isTokenUsable } from './jwt-expiration';

function createToken(payload: object): string {
  const encoded = btoa(JSON.stringify(payload))
    .replace(/=/g, '')
    .replace(/\+/g, '-')
    .replace(/\//g, '_');

  return `header.${encoded}.signature`;
}

describe('JWT expiration', () => {
  it('reads the exp claim', () => {
    const expiration = getTokenExpiration(createToken({ exp: 2_000_000_000 }));
    expect(expiration?.getTime()).toBe(2_000_000_000_000);
  });

  it('rejects malformed tokens and missing exp claims', () => {
    expect(getTokenExpiration('invalid')).toBeNull();
    expect(getTokenExpiration(createToken({ sub: 'user' }))).toBeNull();
  });

  it('only considers a non-expired token usable', () => {
    const now = new Date('2026-01-01T00:00:00Z');
    const future = createToken({ exp: Math.floor(now.getTime() / 1000) + 60 });
    const past = createToken({ exp: Math.floor(now.getTime() / 1000) - 60 });

    expect(isTokenUsable(future, now)).toBeTrue();
    expect(isTokenUsable(past, now)).toBeFalse();
  });
});
