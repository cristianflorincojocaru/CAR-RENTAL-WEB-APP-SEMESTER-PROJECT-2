// ============================================================
// API ERROR MODEL
// ============================================================

/** Structura standard de eroare returnată de ASP.NET Core API */
export interface ApiError {
  status: number;
  title: string;
  detail?: string;
  errors?: Record<string, string[]>;  // validation errors
  traceId?: string;
}
