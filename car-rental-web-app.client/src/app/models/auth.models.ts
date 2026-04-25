// ============================================================
// AUTH MODELS
// ============================================================

export type UserRole = 'Administrator' | 'Manager' | 'Operator' | 'Client';

export interface UserInfo {
  id: number;
  fullName: string;
  username: string;
  email: string;
  phone?: string;
  role: UserRole;
  branchId?: number;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;       // secunde până la expirare access token
  user: UserInfo;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface SignupRequest {
  fullName: string;
  username: string;
  email: string;
  phone: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}
