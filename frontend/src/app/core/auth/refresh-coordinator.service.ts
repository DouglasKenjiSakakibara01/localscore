import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class RefreshCoordinator {
  async runExclusive<T>(
    hasFreshResult: () => boolean,
    operation: () => Promise<T>,
  ): Promise<T | null> {
    if ('locks' in navigator) {
      return navigator.locks.request('localscore-auth-refresh', async () => {
        if (hasFreshResult()) {
          return null;
        }

        return operation();
      });
    }

    if (hasFreshResult()) {
      return null;
    }

    return operation();
  }
}
