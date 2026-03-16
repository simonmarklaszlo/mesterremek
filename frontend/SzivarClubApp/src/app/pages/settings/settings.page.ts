import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { IonContent, IonHeader, IonTitle, IonToolbar, IonButton, IonIcon, IonToggle, IonItem, IonLabel } from '@ionic/angular/standalone';
import { AuthService } from '../../services/auth.service';
import { ThemeService } from '../../services/theme.service';
import { addIcons } from 'ionicons';
import { logOutOutline } from 'ionicons/icons';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.page.html',
  styleUrls: ['./settings.page.scss'],
  standalone: true,
  imports: [IonContent, IonHeader, IonTitle, IonToolbar, IonButton, IonIcon, IonToggle, IonItem, IonLabel, CommonModule, FormsModule]
})
export class SettingsPage implements OnInit {
  isDarkTheme$ = this.themeService.theme$;

  constructor(
    private authService: AuthService,
    private router: Router,
    private themeService: ThemeService
  ) {
    addIcons({ logOutOutline });
  }

  ngOnInit() {
  }

  onThemeChange(event: any) {
    const isDark = event.detail.checked;
    this.themeService.setTheme(isDark);
  }

  onLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

}
