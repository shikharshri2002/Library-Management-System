import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../core/auth.service';

@Component({
  selector: 'app-auth-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './auth-page.html',
  styleUrl: './auth-page.scss'
})
export class AuthPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly isRegister = inject(ActivatedRoute).snapshot.data['mode'] === 'register';
  name = '';
  email = '';
  password = '';
  phone = '';
  error = '';
  busy = false;

  submit(): void {
    this.busy = true;
    this.error = '';
    const request = this.isRegister
      ? this.auth.register(this.name, this.email, this.password, this.phone)
      : this.auth.login(this.email, this.password);
    request.pipe(finalize(() => { this.busy = false; })).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: (error: HttpErrorResponse) => {
        this.error = error.error?.message ?? 'Unable to connect to the library service. Please try again.';
      }
    });
  }
}
