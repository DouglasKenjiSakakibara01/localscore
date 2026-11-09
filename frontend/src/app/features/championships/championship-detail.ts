import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/api/api-error';
import { Championship } from './championship.models';
import { ChampionshipService } from './championship.service';

@Component({
  selector: 'app-championship-detail',
  imports: [DatePipe, RouterLink],
  templateUrl: './championship-detail.html',
})
export class ChampionshipDetail implements OnInit {
  private readonly service = inject(ChampionshipService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly item = signal<Championship | null>(null);
  readonly loading = signal(true);
  readonly working = signal(false);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.getChampionship(this.id).subscribe({
      next: (item) => {
        this.item.set(item);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  transition(
    action: 'publish' | 'return-to-draft' | 'finish' | 'reopen',
    message?: string,
  ): void {
    if (message && !window.confirm(message)) {
      return;
    }

    this.working.set(true);
    this.error.set(null);

    this.service.changeStatus(this.id, action).subscribe({
      next: (item) => {
        this.item.set(item);
        this.working.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.working.set(false);
      },
    });
  }

  remove(): void {
    if (!window.confirm('Excluir definitivamente este campeonato?')) {
      return;
    }

    this.working.set(true);

    this.service.deleteChampionship(this.id).subscribe({
      next: () => void this.router.navigate(['/app/championships']),
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.working.set(false);
      },
    });
  }
}
