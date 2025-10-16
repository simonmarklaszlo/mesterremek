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
  IonLabel
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { bonfireOutline, personAddOutline } from 'ionicons/icons';

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
    CommonModule,
    FormsModule
  ]
})
export class RegisterPage implements OnInit {
  fullName: string = '';
  email: string = '';
  password: string = '';
  confirmPassword: string = '';
  acceptTerms: boolean = false;

  constructor(private router: Router) {
    addIcons({ bonfireOutline, personAddOutline });
  }

  ngOnInit() {
  }

  onRegister() {
    if (this.password !== this.confirmPassword) {
      console.log('Passwords do not match');
      return;
    }

    if (!this.acceptTerms) {
      console.log('Terms not accepted');
      return;
    }

    console.log('Registration attempt:', {
      fullName: this.fullName,
      email: this.email
    });

    // TODO: Itt később majd a tényleges regisztrációs logika jön
    // Egyelőre csak simuláljuk a regisztrációt
    if (this.fullName && this.email && this.password) {
      // Siker esetén navigálunk a login oldalra
      this.router.navigate(['/login']);
    }
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}
