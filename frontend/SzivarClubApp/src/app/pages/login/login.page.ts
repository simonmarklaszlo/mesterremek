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
  IonText
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { bonfireOutline, logInOutline } from 'ionicons/icons';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
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
    CommonModule,
    FormsModule
  ]
})
export class LoginPage implements OnInit {
  email: string = '';
  password: string = '';
  errorMessage: string = '';
  isLoading: boolean = false;

  constructor(
    private router: Router,
    private authService: AuthService
  ) {
    addIcons({ bonfireOutline, logInOutline });
  }

  ngOnInit() {
    // Ha már be van jelentkezve, átirányítjuk a home-ra
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/tabs/home']);
    }
  }

  onLogin() {
    this.errorMessage = '';

    // Validáció
    if (!this.email || !this.password) {
      this.errorMessage = 'Kérlek töltsd ki az összes mezőt!';
      return;
    }

    this.isLoading = true;

    // Bejelentkezés
    this.authService.login(this.email, this.password).subscribe({
      next: (response) => {
        console.log('Sikeres bejelentkezés!', response);
        this.isLoading = false;
        this.router.navigate(['/tabs/home']);
      },
      error: (error) => {
        console.error('Bejelentkezési hiba:', error);
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Hibás email vagy jelszó!';
      }
    });
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }
}
