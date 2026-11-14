import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';
import { LibraryApiService } from '../core/library-api.service';
import { Book, BookInput, BorrowRecord, Member, MemberInput, OverdueBook } from '../core/models';

const emptyBook = (): BookInput => ({ title: '', author: '', isbn: '', category: '', totalCopies: 1 });
const emptyMember = (): MemberInput => ({ name: '', email: '', phone: '' });

@Component({
  selector: 'app-dashboard-page',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss'
})
export class DashboardPage implements OnInit {
  private readonly api = inject(LibraryApiService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthService);
  readonly user = this.auth.session;
  readonly section = this.router.url.split('?')[0].replace('/', '') || 'dashboard';
  books: Book[] = [];
  members: Member[] = [];
  records: BorrowRecord[] = [];
  overdue: OverdueBook[] = [];
  search = '';
  error = '';
  success = '';
  loading = false;
  showBookForm = false;
  showMemberForm = false;
  editingBookId?: number;
  editingMemberId?: number;
  bookForm = emptyBook();
  memberForm = emptyMember();
  selectedBookId = 0;
  selectedMemberId = 0;
  dueDate = '';

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = '';
    if (this.section === 'dashboard' || this.section === 'books') {
      this.api.getBooks(this.section === 'books' ? this.search : '').subscribe({
        next: (items) => { this.books = items; this.loading = false; },
        error: (error: HttpErrorResponse) => this.fail(error)
      });
    }
    if (this.section === 'members') {
      this.api.getMembers(this.search).subscribe({
        next: (items) => { this.members = items; this.loading = false; },
        error: (error: HttpErrorResponse) => this.fail(error)
      });
    }
    if (this.section === 'loans') {
      this.api.getBorrowRecords().subscribe({
        next: (items) => { this.records = items; this.loading = false; },
        error: (error: HttpErrorResponse) => this.fail(error)
      });
      this.api.getBooks().subscribe({ next: (items) => { this.books = items; }, error: (error) => this.fail(error) });
      if (this.auth.isAdmin()) this.api.getMembers().subscribe({ next: (items) => { this.members = items; }, error: (error) => this.fail(error) });
    }
    if (this.section === 'overdue') {
      this.api.getOverdueBooks().subscribe({
        next: (items) => { this.overdue = items; this.loading = false; },
        error: (error: HttpErrorResponse) => this.fail(error)
      });
    }
  }

  searchChanged(): void {
    this.load();
  }

  startBookEdit(book?: Book): void {
    this.editingBookId = book?.id;
    this.bookForm = book
      ? { title: book.title, author: book.author, isbn: book.isbn ?? '', category: book.category ?? '', totalCopies: book.totalCopies }
      : emptyBook();
    this.showBookForm = true;
    this.error = '';
    this.success = '';
  }

  saveBook(): void {
    this.api.saveBook(this.bookForm, this.editingBookId).subscribe({
      next: () => { this.showBookForm = false; this.success = this.editingBookId ? 'Book updated.' : 'Book added to the catalog.'; this.load(); },
      error: (error: HttpErrorResponse) => this.fail(error)
    });
  }

  removeBook(book: Book): void {
    if (!confirm(`Delete “${book.title}”?`)) return;
    this.api.deleteBook(book.id).subscribe({
      next: () => { this.success = 'Book deleted.'; this.load(); },
      error: (error: HttpErrorResponse) => this.fail(error)
    });
  }

  startMemberEdit(member?: Member): void {
    this.editingMemberId = member?.id;
    this.memberForm = member ? { name: member.name, email: member.email, phone: member.phone ?? '' } : emptyMember();
    this.showMemberForm = true;
    this.error = '';
    this.success = '';
  }

  saveMember(): void {
    this.api.saveMember(this.memberForm, this.editingMemberId).subscribe({
      next: () => { this.showMemberForm = false; this.success = this.editingMemberId ? 'Member updated.' : 'Member added.'; this.load(); },
      error: (error: HttpErrorResponse) => this.fail(error)
    });
  }

  removeMember(member: Member): void {
    if (!confirm(`Delete ${member.name}?`)) return;
    this.api.deleteMember(member.id).subscribe({
      next: () => { this.success = 'Member deleted.'; this.load(); },
      error: (error: HttpErrorResponse) => this.fail(error)
    });
  }

  issueBook(): void {
    if (!this.selectedBookId || !this.dueDate) return;
    const dueAt = new Date(`${this.dueDate}T23:59:59`).toISOString();
    this.api.issueBook(this.selectedBookId, dueAt, this.auth.isAdmin() ? this.selectedMemberId : undefined).subscribe({
      next: () => { this.success = 'Book issued successfully.'; this.selectedBookId = 0; this.selectedMemberId = 0; this.load(); },
      error: (error: HttpErrorResponse) => this.fail(error)
    });
  }

  returnBook(record: BorrowRecord): void {
    this.api.returnBook(record.id).subscribe({
      next: () => { this.success = 'Book marked as returned.'; this.load(); },
      error: (error: HttpErrorResponse) => this.fail(error)
    });
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }

  get pageTitle(): string {
    return ({ dashboard: 'Your library, at a glance', books: 'The book collection', members: 'Library members', loans: 'Borrowing desk', overdue: 'Overdue books' } as Record<string, string>)[this.section] ?? 'Your library';
  }

  get currentDateLabel(): string {
    return new Intl.DateTimeFormat('en-US', {
      weekday: 'long', month: 'long', day: '2-digit'
    }).format(new Date()).toUpperCase();
  }

  get minDueDate(): string {
    return new Date().toISOString().slice(0, 10);
  }

  get activeLoans(): number {
    return this.records.filter((record) => !record.returnedAt).length;
  }

  private fail(error: HttpErrorResponse): void {
    this.error = error.error?.message ?? 'The request could not be completed. Check the API connection and try again.';
    this.loading = false;
  }
}
