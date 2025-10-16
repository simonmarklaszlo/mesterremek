import { Routes } from '@angular/router';
import { TabsPage } from './tabs.page';

export const routes: Routes = [
  {
    path: '',
    component: TabsPage,
    children: [
      {
        path : 'home',
        loadComponent: () =>
          import('../pages/home/home.page').then(m => m.HomePage),
      },
      {
        path : 'map',
        loadComponent: () =>
          import('../pages/map/map.page').then(m => m.MapPage),
      },
      {
        path : 'settings',
        loadComponent: () =>
          import('../pages/settings/settings.page').then(m => m.SettingsPage),
      },
      {
        path: '',
        redirectTo: 'home',
        pathMatch: 'full',
      },
    ],
  },
];
