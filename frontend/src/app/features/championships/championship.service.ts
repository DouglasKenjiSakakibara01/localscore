import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import {
  Championship,
  ChampionshipInput,
  ChampionshipTeam,
  PublicChampionship,
  Team,
  TeamInput,
} from './championship.models';

@Injectable({ providedIn: 'root' })
export class ChampionshipService {
  private readonly http = inject(HttpClient);

  listChampionships() {
    return this.http.get<Championship[]>('/api/v1/championships');
  }

  getChampionship(id: string) {
    return this.http.get<Championship>(`/api/v1/championships/${id}`);
  }

  createChampionship(input: ChampionshipInput) {
    return this.http.post<Championship>('/api/v1/championships', input);
  }

  updateChampionship(id: string, input: ChampionshipInput & { slug: string }) {
    return this.http.put<Championship>(`/api/v1/championships/${id}`, input);
  }

  deleteChampionship(id: string) {
    return this.http.delete<void>(`/api/v1/championships/${id}`);
  }

  changeStatus(id: string, action: 'publish' | 'return-to-draft' | 'finish' | 'reopen') {
    return this.http.post<Championship>(`/api/v1/championships/${id}/${action}`, {});
  }

  listTeams() {
    return this.http.get<Team[]>('/api/v1/teams');
  }

  getTeam(id: string) {
    return this.http.get<Team>(`/api/v1/teams/${id}`);
  }

  createTeam(input: TeamInput) {
    return this.http.post<Team>('/api/v1/teams', input);
  }

  updateTeam(id: string, input: TeamInput) {
    return this.http.put<Team>(`/api/v1/teams/${id}`, input);
  }

  deleteTeam(id: string) {
    return this.http.delete<void>(`/api/v1/teams/${id}`);
  }

  listParticipants(championshipId: string) {
    return this.http.get<ChampionshipTeam[]>(
      `/api/v1/championships/${championshipId}/teams`,
    );
  }

  activateTeam(championshipId: string, teamId: string) {
    return this.http.put<ChampionshipTeam>(
      `/api/v1/championships/${championshipId}/teams/${teamId}`,
      {},
    );
  }

  removeTeam(championshipId: string, teamId: string) {
    return this.http.delete<void>(
      `/api/v1/championships/${championshipId}/teams/${teamId}`,
    );
  }

  getPublicChampionship(slug: string) {
    return this.http.get<PublicChampionship>(`/api/v1/public/championships/${slug}`);
  }
}
