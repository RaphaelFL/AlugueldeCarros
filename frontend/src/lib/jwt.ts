import { jwtDecode } from 'jwt-decode';
import type { Role } from '@/types/domain';

interface JwtClaims {
  exp?: number;
  email?: string;
  name?: string;
  role?: string | string[];
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[];
}

export function decodeToken(token: string) {
  return jwtDecode<JwtClaims>(token);
}

export function getTokenRoles(token: string): Role[] {
  const claims = decodeToken(token);
  const rawRoles = claims['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? claims.role ?? [];
  const roles = Array.isArray(rawRoles) ? rawRoles : [rawRoles];
  return roles.filter(Boolean) as Role[];
}

export function getTokenUserId(token: string) {
  const claims = decodeToken(token);
  const rawUserId = claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'];
  return rawUserId ? Number(rawUserId) : null;
}

export function isTokenExpired(token: string, bufferSeconds = 0) {
  const claims = decodeToken(token);
  if (!claims.exp) return true;
  return claims.exp <= Math.floor(Date.now() / 1000) + bufferSeconds;
}
