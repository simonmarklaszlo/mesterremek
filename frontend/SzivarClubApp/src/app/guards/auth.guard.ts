import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isLoggedIn()) {
    console.log('Auth Guard: Felhasználó be van jelentkezve');
    return true;
  }

  console.log('Auth Guard: Nincs bejelentkezve, átirányítás login-ra');
  router.navigate(['/login']);
  return false;


};

