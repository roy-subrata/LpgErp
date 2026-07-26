export interface AuthResult {
  isSuccess: boolean;
  token?: string;
  refreshToken?: string;
  expiresAt?: string;
  user?: UserDto;
  error?: string;
}

export interface UserDto {
  id: string;
  username: string;
  email: string;
  fullName?: string;
  phone?: string;
  isActive: boolean;
  lastLoginAt?: string;
  roles: string[];
  permissions: string[];
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  fullName?: string;
  phone?: string;
  roleName?: string;
}

export interface RoleDto {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  permissions: string[];
}

export interface PermissionDto {
  id: string;
  name: string;
  description?: string;
  group: string;
}
