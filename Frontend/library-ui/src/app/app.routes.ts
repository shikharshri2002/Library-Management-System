import { Routes } from '@angular/router';
import { adminGuard, authGuard } from './core/auth.guard';
import { AuthPage } from './pages/auth-page';
import { DashboardPage } from './pages/dashboard-page';

export const routes: Routes = [
  { path: 'login', component: AuthPage, data: { mode: 'login' } },
  { path: 'register', component: AuthPage, data: { mode: 'register' } },
  { path: 'dashboard', component: DashboardPage, canActivate: [authGuard] },
  { path: 'books', component: DashboardPage, canActivate: [authGuard] },
  { path: 'members', component: DashboardPage, canActivate: [adminGuard] },
  { path: 'loans', component: DashboardPage, canActivate: [authGuard] },
  { path: 'overdue', component: DashboardPage, canActivate: [adminGuard] },
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: '**', redirectTo: 'dashboard' }
];
