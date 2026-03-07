import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'results',
    pathMatch: 'full'
  },
  {
    path: 'results',
    loadComponent: () => import('./features/results/results.page').then(m => m.ResultsPage)
  }
];
