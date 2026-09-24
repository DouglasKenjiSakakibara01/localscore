export interface AuthUser {
  id: string;
  name: string;
  email: string;
}

export interface AuthenticationResponse {
  accessToken: string;
  accessTokenExpiresAt: string;
  sessionExpiresAt: string;
  user: AuthUser;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ApiProblemDetails {
  title?: string;
  detail?: string;
  code?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
