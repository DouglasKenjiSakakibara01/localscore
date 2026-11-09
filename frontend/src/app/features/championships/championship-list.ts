import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/api/api-error';
import { Championship } from './championship.models';
import { ChampionshipService } from './championship.service';

@Component({
  selector: 'app-championship-list',
  imports: [DatePipe, RouterLink],
  templateUrl: './championship-list.html',
})
export class ChampionshipList implements OnInit {
  private readonly service = inject(ChampionshipService);

  readonly championships = signal<Championship[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.error.set(null);

    this.service.listChampionships().subscribe({
      next: (items) => {
        this.championships.set(items);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }
}
