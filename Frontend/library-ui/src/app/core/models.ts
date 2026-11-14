export interface AuthResponse {
  token: string;
  expiresAt: string;
  role: 'Admin' | 'User';
  name: string;
}

export interface Book {
  id: number;
  title: string;
  author: string;
  isbn: string | null;
  category: string | null;
  totalCopies: number;
  availableCopies: number;
}

export interface BookInput {
  title: string;
  author: string;
  isbn: string;
  category: string;
  totalCopies: number;
}

export interface Member {
  id: number;
  name: string;
  email: string;
  phone: string | null;
  joinedAt: string;
}

export interface MemberInput {
  name: string;
  email: string;
  phone: string;
}

export interface BorrowRecord {
  id: number;
  bookId: number;
  bookTitle: string;
  memberId: number;
  memberName: string;
  borrowedAt: string;
  dueAt: string;
  returnedAt: string | null;
}

export interface OverdueBook {
  borrowRecordId: number;
  bookId: number;
  title: string;
  memberId: number;
  memberName: string;
  memberEmail: string;
  borrowedAt: string;
  dueAt: string;
  daysOverdue: number;
}
