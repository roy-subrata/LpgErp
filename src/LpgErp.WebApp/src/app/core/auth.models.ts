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

export interface CreateUserRequest {
  username: string;
  email: string;
  password: string;
  fullName?: string;
  phone?: string;
  roleIds?: string[];
}

export interface UpdateUserRequest {
  email?: string;
  fullName?: string;
  phone?: string;
  isActive?: boolean;
  newPassword?: string;
}

/** A role definition, as managed on the Roles & Permissions screen — richer than RoleDto above,
 *  which only serves the role picker on the Users screen. */
export interface RoleSummary {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
  permissions: string[];
  permissionIds: string[];
  userCount: number;
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
  isActive: boolean;
  permissionIds: string[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string;
  isActive: boolean;
  permissionIds: string[];
}

export interface PermissionDto {
  id: string;
  name: string;
  description?: string;
  group: string;
}
