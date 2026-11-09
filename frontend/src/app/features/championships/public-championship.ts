import { Component, inject, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { PublicChampionship } from './championship.models';
import { ChampionshipService } from './championship.service';

@Component({
  selector: 'app-public-championship',
  imports: [DatePipe, RouterLink],
  templateUrl: './public-championship.html',
})
export class PublicChampionshipPage implements OnInit {
  private readonly service = inject(ChampionshipService);
  private readonly route = inject(ActivatedRoute);

  readonly championship = signal<PublicChampionship | null>(null);
  readonly loading = signal(true);
  readonly notFound = signal(false);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug')!;

    this.service.getPublicChampionship(slug).subscribe({
      next: (item) => {
        this.championship.set(item);
        this.loading.set(false);
      },
      error: () => {
        this.notFound.set(true);
        this.loading.set(false);
      },
    });
  }
}
