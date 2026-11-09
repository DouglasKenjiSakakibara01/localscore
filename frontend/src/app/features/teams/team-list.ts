import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/api/api-error';
import { Team } from '../championships/championship.models';
import { ChampionshipService } from '../championships/championship.service';

@Component({
  selector: 'app-team-list',
  imports: [RouterLink],
  templateUrl: './team-list.html',
})
export class TeamList implements OnInit {
  private readonly service = inject(ChampionshipService);

  readonly teams = signal<Team[]>([]);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.service.listTeams().subscribe({
      next: (teams) => {
        this.teams.set(teams);
        this.loading.set(false);
      },
      error: (error) => {
        this.error.set(apiErrorMessage(error));
        this.loading.set(false);
      },
    });
  }

  remove(team: Team): void {
    if (!window.confirm(`Excluir definitivamente ${team.name}?`)) {
      return;
    }

    this.service.deleteTeam(team.id).subscribe({
      next: () => this.load(),
      error: (error) => this.error.set(apiErrorMessage(error)),
    });
  }
}
