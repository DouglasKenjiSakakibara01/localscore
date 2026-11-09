import { HttpErrorResponse } from '@angular/common/http';
import { ApiProblemDetails } from '../auth/auth.models';

export function apiErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const problem = error.error as ApiProblemDetails | undefined;
    return problem?.detail ?? 'Não foi possível concluir a operação.';
  }

  return 'Não foi possível concluir a operação.';
}
