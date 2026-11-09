import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { apiErrorMessage } from '../../core/api/api-error';
import { Championship, ChampionshipTeam, Team } from './championship.models';
import { ChampionshipService } from './championship.service';

@Component({
  selector: 'app-championship-teams',
  imports: [RouterLink],
  templateUrl: './championship-teams.html',
})
export class ChampionshipTeams implements OnInit {
  private readonly service = inject(ChampionshipService);
  private readonly route = inject(ActivatedRoute);

  readonly id = this.route.snapshot.paramMap.get('id')!;
  readonly championship = signal<Championship | null>(null);
  readonly participants = signal<ChampionshipTeam[]>([]);
  readonly catalog = signal<Team[]>([]);
  readonly selectedTeamId = signal('');
  readonly loading = signal(true);
  readonly working = signal(false);
  readonly error = signal<string | null>(null);
  readonly activeParticipants = computed(() =>
    this.participants().filter((item) => item.isActive),
  );
  readonly removedParticipants = computed(() =>
    this.participants().filter((item) => !item.isActive),
  );
  readonly availableTeams = computed(() => {
    const active = new Set(this.activeParticipants().map((item) => item.team.id));
    return this.catalog().filter((team) => !active.has(team.id));
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading.set(true);

    forkJoin({
      championship: this.service.getChampionship(this.id),
      participants: this.service.listParticipants(this.id),
      catalog: this.service.listTeams(),
    }).subscribe({
      next: ({ championship, participants, catalog }) => {
        this.championship.set(championship);
        this.participants.set(participants);
        this.catalog.set(catalog);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  select(event: Event): void {
    this.selectedTeamId.set((event.target as HTMLSelectElement).value);
  }

  activate(teamId = this.selectedTeamId()): void {
    if (!teamId || this.working()) {
      return;
    }

    this.working.set(true);
    this.error.set(null);

    this.service.activateTeam(this.id, teamId).subscribe({
      next: () => {
        this.selectedTeamId.set('');
        this.working.set(false);
        this.load();
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.working.set(false);
      },
    });
  }

  remove(teamId: string): void {
    if (!window.confirm('Remover esta equipe do campeonato?')) {
      return;
    }

    this.working.set(true);

    this.service.removeTeam(this.id, teamId).subscribe({
      next: () => {
        this.working.set(false);
        this.load();
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.working.set(false);
      },
    });
  }
}
