import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Book, BookInput, BorrowRecord, Member, MemberInput, OverdueBook } from './models';

const apiUrl = 'http://localhost:5160/api';

@Injectable({ providedIn: 'root' })
export class LibraryApiService {
  constructor(private readonly http: HttpClient) {}

  getBooks(search = ''): Observable<Book[]> {
    return this.http.get<Book[]>(`${apiUrl}/books`, { params: this.searchParams(search) });
  }

  saveBook(book: BookInput, id?: number): Observable<Book> {
    return id
      ? this.http.put<Book>(`${apiUrl}/books/${id}`, book)
      : this.http.post<Book>(`${apiUrl}/books`, book);
  }

  deleteBook(id: number): Observable<void> {
    return this.http.delete<void>(`${apiUrl}/books/${id}`);
  }

  getMembers(search = ''): Observable<Member[]> {
    return this.http.get<Member[]>(`${apiUrl}/members`, { params: this.searchParams(search) });
  }

  saveMember(member: MemberInput, id?: number): Observable<Member> {
    return id
      ? this.http.put<Member>(`${apiUrl}/members/${id}`, member)
      : this.http.post<Member>(`${apiUrl}/members`, member);
  }

  deleteMember(id: number): Observable<void> {
    return this.http.delete<void>(`${apiUrl}/members/${id}`);
  }

  getBorrowRecords(): Observable<BorrowRecord[]> {
    return this.http.get<BorrowRecord[]>(`${apiUrl}/borrowing`);
  }

  issueBook(bookId: number, dueAt: string, memberId?: number): Observable<BorrowRecord> {
    return this.http.post<BorrowRecord>(`${apiUrl}/borrowing`, { bookId, dueAt, memberId: memberId ?? null });
  }

  returnBook(id: number): Observable<void> {
    return this.http.post<void>(`${apiUrl}/borrowing/${id}/return`, {});
  }

  getOverdueBooks(): Observable<OverdueBook[]> {
    return this.http.get<OverdueBook[]>(`${apiUrl}/borrowing/overdue`);
  }

  private searchParams(search: string): HttpParams {
    return search ? new HttpParams().set('search', search) : new HttpParams();
  }
}
