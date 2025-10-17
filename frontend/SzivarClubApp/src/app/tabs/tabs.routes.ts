import { Routes } from '@angular/router';
import { TabsPage } from './tabs.page';

export const routes: Routes = [
  {
    path: '',
    component: TabsPage,
    children: [
      {
        path : 'list',
        loadComponent: () =>
          import('../pages/list/list.page').then(m => m.ListPage),
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
        redirectTo: 'list',
        pathMatch: 'full',
      },
    ],
  },
];
