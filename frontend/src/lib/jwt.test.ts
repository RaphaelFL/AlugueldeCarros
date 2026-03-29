import { getTokenRoles, getTokenUserId, isTokenExpired } from '@/lib/jwt';

function createToken(payload: Record<string, unknown>) {
  const encode = (value: Record<string, unknown>) => Buffer.from(JSON.stringify(value)).toString('base64url');
  return `${encode({ alg: 'none', typ: 'JWT' })}.${encode(payload)}.`;
}

describe('jwt helpers', () => {
  it('resolve roles a partir do claim namespaced', () => {
    const token = createToken({
      'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': ['Customer', 'Admin'],
    });

    expect(getTokenRoles(token)).toEqual(['Customer', 'Admin']);
  });

  it('resolve userId numérico a partir do token', () => {
    const token = createToken({
      'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': '42',
    });

    expect(getTokenUserId(token)).toBe(42);
  });

  it('considera o buffer ao verificar expiração', () => {
    const futureButInsideBuffer = createToken({
      exp: Math.floor(Date.now() / 1000) + 30,
    });

    const futureOutsideBuffer = createToken({
      exp: Math.floor(Date.now() / 1000) + 120,
    });

    expect(isTokenExpired(futureButInsideBuffer, 60)).toBe(true);
    expect(isTokenExpired(futureOutsideBuffer, 60)).toBe(false);
  });
});