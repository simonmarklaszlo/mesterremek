import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';

interface LoginResponse {
  token: string;
  user: {
    id: number;
    email: string;
    name: string;
    role?: string;
  };
}

interface RegisterData {
  email: string;
  password: string;
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`; // Backend URL az environment-ből
  private tokenKey = 'auth_token';
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadStoredToken();
  }

  // Bejelentkezés
  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, { email, password })
      .pipe(
        tap(response => {
          localStorage.setItem(this.tokenKey, response.token);
          this.currentUserSubject.next(response.user);
          console.log('Token elmentve:', response.token);
        })
      );
  }

  // Regisztráció
  register(data: RegisterData): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, data);
  }

  // Kijelentkezés
  logout() {
    localStorage.removeItem(this.tokenKey);
    this.currentUserSubject.next(null);
  }

  // Token lekérése
  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  // Be van-e jelentkezve?
  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    // Ellenőrizzük, hogy lejárt-e a token
    try {
      const decoded = this.getDecodedToken();
      if (!decoded || !decoded.exp) return false;

      const expirationDate = new Date(decoded.exp * 1000);
      return expirationDate > new Date();
    } catch (e) {
      return false;
    }
  }

  // Token dekódolása
  getDecodedToken(): any {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch (e) {
      console.error('Token dekódolási hiba:', e);
      return null;
    }
  }

  // Felhasználó role lekérése
  getUserRole(): string | null {
    const decoded = this.getDecodedToken();
    return decoded?.role || null;
  }

  // Admin jogosultság ellenőrzése
  isAdmin(): boolean {
    return this.getUserRole() === 'admin';
  }

  // Tárolt token betöltése (oldal frissítéskor)
  private loadStoredToken() {
    const token = this.getToken();
    if (token && this.isLoggedIn()) {
      const decoded = this.getDecodedToken();
      if (decoded) {
        this.currentUserSubject.next({
          id: decoded.userId,
          email: decoded.email,
          name: decoded.name,
          role: decoded.role
        });
      }
    }
  }
}
