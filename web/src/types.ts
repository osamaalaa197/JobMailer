export interface UserInfo {
  userId: number;
  email: string;
  fullName: string;
}

export interface Profile {
  id: number;
  title: string;
  fullName: string;
  phone: string;
  email: string;
  cvFileName: string | null;
  hasCv: boolean;
}

export interface ProfileInput {
  title: string;
  fullName: string;
  phone: string;
  email: string;
  cvBase64: string | null;
  cvFileName: string | null;
}

export interface Template {
  id: number;
  key: string;
  subject: string;
  body: string;
}

export interface RenderedEmail {
  subject: string;
  body: string;
}

export interface ApplicationRecord {
  id: number;
  company: string | null;
  jobTitle: string | null;
  profileTitle: string;
  toEmail: string;
  subject: string;
  sentAt: string;
}
