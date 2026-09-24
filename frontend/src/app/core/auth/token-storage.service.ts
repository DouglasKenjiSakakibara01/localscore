import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenStorage {
  static readonly key = 'LocalScore.AccessToken';

  private readonly tokenState = signal<string | null>(
    localStorage.getItem(TokenStorage.key),
  );

  readonly token = this.tokenState.asReadonly();

  constructor() {
    window.addEventListener('storage', (event) => {
      if (event.key === TokenStorage.key) {
        this.tokenState.set(event.newValue);
      }
    });
  }

  set(token: string): void {
    localStorage.setItem(TokenStorage.key, token);
    this.tokenState.set(token);
  }

  clear(): void {
    localStorage.removeItem(TokenStorage.key);
    this.tokenState.set(null);
  }
}
