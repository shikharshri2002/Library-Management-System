import { HttpClient } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthResponse } from './models';

const apiUrl = 'http://localhost:5160/api';
const sessionKey = 'library-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly sessionState = signal<AuthResponse | null>(this.readSession());
  readonly session = this.sessionState.asReadonly();
  readonly isAdmin = computed(() => this.sessionState()?.role === 'Admin');

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${apiUrl}/auth/login`, { email, password })
      .pipe(tap((session) => this.saveSession(session)));
  }

  register(name: string, email: string, password: string, phone: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${apiUrl}/auth/register`, {
      name, email, password, phone: phone.trim() || null
    })
      .pipe(tap((session) => this.saveSession(session)));
  }

  logout(): void {
    localStorage.removeItem(sessionKey);
    this.sessionState.set(null);
  }

  private saveSession(session: AuthResponse): void {
    localStorage.setItem(sessionKey, JSON.stringify(session));
    this.sessionState.set(session);
  }

  private readSession(): AuthResponse | null {
    try {
      const stored = localStorage.getItem(sessionKey);
      if (!stored) return null;
      const session = JSON.parse(stored) as AuthResponse;
      if (!session.token || !session.expiresAt || new Date(session.expiresAt) <= new Date()) {
        localStorage.removeItem(sessionKey);
        return null;
      }
      return session;
    } catch {
      localStorage.removeItem(sessionKey);
      return null;
    }
  }
}
