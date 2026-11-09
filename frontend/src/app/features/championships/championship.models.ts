export interface ChampionshipStatus {
  code: 1 | 2 | 3;
  description: string;
}

export interface Championship {
  id: string;
  name: string;
  slug: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  status: ChampionshipStatus;
  canChangeSlug: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ChampionshipInput {
  name: string;
  slug?: string | null;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
}

export interface Team {
  id: string;
  name: string;
  shortName: string | null;
  acronym: string | null;
  city: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface TeamInput {
  name: string;
  shortName: string | null;
  acronym: string | null;
  city: string | null;
  primaryColor: string | null;
  secondaryColor: string | null;
}

export interface ChampionshipTeam {
  id: string;
  team: Team;
  registeredAt: string;
  removedAt: string | null;
  isActive: boolean;
}

export interface PublicChampionship {
  name: string;
  slug: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  status: ChampionshipStatus;
  teams: Omit<Team, 'createdAt' | 'updatedAt'>[];
}
