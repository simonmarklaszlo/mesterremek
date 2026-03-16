import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonIcon,
  IonText
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { homeOutline, arrowBackOutline, alertCircleOutline } from 'ionicons/icons';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.page.html',
  styleUrls: ['./not-found.page.scss'],
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonIcon,
    IonText,
    CommonModule
  ]
})
export class NotFoundPage implements OnInit {

  constructor(private router: Router) {
    addIcons({ homeOutline, arrowBackOutline, alertCircleOutline });
  }

  ngOnInit() {
    console.log('404 - Oldal nem található');
  }

  goHome() {
    this.router.navigate(['/login']);
  }

  goBack() {
    window.history.back();
  }
}

