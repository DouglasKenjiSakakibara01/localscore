import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/auth/auth.guards';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    loadComponent: () =>
      import('./features/home/home').then((component) => component.Home),
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/login/login').then((component) => component.Login),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () =>
      import('./features/auth/register/register').then(
        (component) => component.Register,
      ),
  },
  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/app-shell/app-shell').then((component) => component.AppShell),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then((component) => component.Dashboard),
      },
      {
        path: 'championships',
        loadComponent: () =>
          import('./features/championships/championship-list').then((component) => component.ChampionshipList),
      },
      {
        path: 'championships/new',
        loadComponent: () =>
          import('./features/championships/championship-form').then((component) => component.ChampionshipForm),
      },
      {
        path: 'championships/:id/edit',
        loadComponent: () =>
          import('./features/championships/championship-form').then((component) => component.ChampionshipForm),
      },
      {
        path: 'championships/:id/teams',
        loadComponent: () =>
          import('./features/championships/championship-teams').then((component) => component.ChampionshipTeams),
      },
      {
        path: 'championships/:id',
        loadComponent: () =>
          import('./features/championships/championship-detail').then((component) => component.ChampionshipDetail),
      },
      {
        path: 'teams',
        loadComponent: () =>
          import('./features/teams/team-list').then((component) => component.TeamList),
      },
      {
        path: 'teams/new',
        loadComponent: () =>
          import('./features/teams/team-form').then((component) => component.TeamForm),
      },
      {
        path: 'teams/:id/edit',
        loadComponent: () =>
          import('./features/teams/team-form').then((component) => component.TeamForm),
      },
    ],
  },
  {
    path: 'championships/:slug',
    loadComponent: () =>
      import('./features/championships/public-championship').then((component) => component.PublicChampionshipPage),
  },
  { path: '**', redirectTo: '' },
];
