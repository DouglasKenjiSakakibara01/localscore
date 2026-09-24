import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { ApiProblemDetails } from '../../../core/auth/auth.models';
import { AuthService } from '../../../core/auth/auth.service';

export const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl,
): ValidationErrors | null => {
  const password = control.get('password')?.value as string | undefined;
  const confirmation = control.get('passwordConfirmation')?.value as
    | string
    | undefined;

  return password === confirmation ? null : { passwordsMismatch: true };
};

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: '../auth-form.scss',
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly serverErrors = signal<Record<string, string[]>>({});

  readonly form = this.formBuilder.nonNullable.group(
    {
      name: [
        '',
        [
          Validators.required,
          Validators.minLength(2),
          Validators.maxLength(100),
          Validators.pattern(/^[\p{L}\p{M}][\p{L}\p{M} '\-]*$/u),
        ],
      ],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.maxLength(128),
          Validators.pattern(/^(?=.*[a-z])(?=.*\d).+$/),
        ],
      ],
      passwordConfirmation: ['', Validators.required],
    },
    { validators: passwordsMatchValidator },
  );

  submit(): void {
    this.errorMessage.set(null);
    this.serverErrors.set({});

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    const value = this.form.getRawValue();

    this.authService
      .register({
        name: value.name.trim(),
        email: value.email.trim(),
        password: value.password,
      })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: () => void this.router.navigateByUrl('/app'),
        error: (error: unknown) => this.handleError(error),
      });
  }

  private handleError(error: unknown): void {
    const problem =
      error instanceof HttpErrorResponse
        ? (error.error as ApiProblemDetails | null)
        : null;

    this.serverErrors.set(problem?.errors ?? {});
    this.errorMessage.set(
      problem?.detail ?? 'Não foi possível criar a conta. Tente novamente.',
    );
  }
}
