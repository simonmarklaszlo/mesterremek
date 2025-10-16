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

  constructor(private router: Router) {
    addIcons({ bonfireOutline, logInOutline });
  }

  ngOnInit() {
  }

  onLogin() {
    console.log('Login attempt:', this.email);

    // TODO: Itt később majd a tényleges autentikációs logika jön
    // Egyelőre csak simuláljuk a bejelentkezést
    if (this.email && this.password) {
      // Siker esetén navigálunk a home oldalra
      this.router.navigate(['/tabs/home']);
    }
  }

  goToRegister() {
    this.router.navigate(['/register']);
  }

}
