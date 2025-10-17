import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonList,
  IonItem,
  IonInput,
  IonButton,
  IonIcon,
  IonText,
  IonCheckbox,
  IonLabel,
  IonSpinner
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { bonfireOutline, personAddOutline } from 'ionicons/icons';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss'],
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonList,
    IonItem,
    IonInput,
    IonButton,
    IonIcon,
    IonText,
    IonCheckbox,
    IonLabel,
    IonSpinner,
    CommonModule,
    FormsModule
  ]
})
export class RegisterPage implements OnInit {
  username: string = '';
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  acceptTerms: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    addIcons({ bonfireOutline, personAddOutline });
  }

  ngOnInit() {
    // Ha már be van jelentkezve, átirányítjuk a list oldalra
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/tabs/list']);
    }
  }

  onRegister() {
    // Töröljük a korábbi üzeneteket
    this.errorMessage = '';
    this.successMessage = '';

    // Validációk
    if (!this.username.trim()) {
      this.errorMessage = 'Kérlek add meg a felhasználóneved!';
      return;
    }

    if (!this.email.trim()) {
      this.errorMessage = 'Kérlek add meg az email címed!';
      return;
    }

    if (!this.isValidEmail(this.email)) {
      this.errorMessage = 'Érvénytelen email cím!';
      return;
    }

    if (this.password.length < 6) {
      this.errorMessage = 'A jelszónak legalább 6 karakter hosszúnak kell lennie!';
      return;
    }

    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'A jelszavak nem egyeznek!';
      return;
    }

    if (!this.acceptTerms) {
      this.errorMessage = 'El kell fogadnod az Általános Szerződési Feltételeket!';
      return;
    }

    // Regisztráció indítása
    this.isLoading = true;

    const registerData = {
      name: this.username.trim(),
      email: this.email.trim().toLowerCase(),
      password: this.password
    };

    this.authService.register(registerData).subscribe({
      next: () => {
        console.log('Sikeres regisztráció');
        this.successMessage = 'Sikeres regisztráció! Automatikus bejelentkezés...';

        // Automatikus bejelentkezés a regisztráció után
        setTimeout(() => {
          this.autoLogin();
        }, 1500);
      },
      error: (error) => {
        console.error('Regisztrációs hiba:', error);
        this.isLoading = false;

        // Részletes hibakezelés
        if (error.status === 409) {
          this.errorMessage = 'Ez az email cím már használatban van!';
        } else if (error.status === 400) {
          this.errorMessage = error.error?.message || 'Érvénytelen adatok!';
        } else if (error.status === 0) {
          this.errorMessage = 'Nem sikerült kapcsolódni a szerverhez. Ellenőrizd a kapcsolatot!';
        } else {
          this.errorMessage = 'Hiba történt a regisztráció során. Próbáld újra!';
        }
      }
    });
  }

  // Automatikus bejelentkezés regisztráció után
  private autoLogin() {
    this.authService.login(this.email, this.password).subscribe({
      next: () => {
        console.log('Automatikus bejelentkezés sikeres');
        this.isLoading = false;
        this.router.navigate(['/tabs/list']);
      },
      error: (error) => {
        console.error('Automatikus bejelentkezési hiba:', error);
        this.isLoading = false;
        this.successMessage = 'Sikeres regisztráció! Jelentkezz be a folytatáshoz.';

        // Ha nem sikerül automatikusan, átirányítjuk a login-ra
        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 2000);
      }
    });
  }

  // Email validálás
  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }

  // Jelszó erősség ellenőrzése
  getPasswordStrength(): string {
    if (!this.password) return '';

    if (this.password.length < 6) return 'weak';
    if (this.password.length < 10) return 'medium';
    return 'strong';
  }
}
