// ============================================================
// CONTACT MODELS
// ============================================================

export interface ContactMessageRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  subject: string;
  message: string;
}

export interface ContactMessageResponse {
  success: boolean;
  message: string;
}
