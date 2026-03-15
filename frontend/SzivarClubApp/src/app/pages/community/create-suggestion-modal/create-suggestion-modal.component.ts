import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonButton,
  IonButtons,
  IonItem,
  IonLabel,
  IonInput,
  IonList,
  IonIcon,
  IonText,
  IonCheckbox,
  IonSpinner,
  ModalController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { closeOutline, storefrontOutline, timeOutline } from 'ionicons/icons';
import { SuggestionService } from '../../../services/suggestion.service';
import { CreateSuggestionRequest } from '../../../models/suggestion.model';

interface OpeningHourInput {
  dayOfWeek: string;
  openHour: string;
  closeHour: string;
  isClosed: boolean;
}

@Component({
  selector: 'app-create-suggestion-modal',
  templateUrl: './create-suggestion-modal.component.html',
  styleUrls: ['./create-suggestion-modal.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonContent,
    IonButton,
    IonButtons,
    IonItem,
    IonLabel,
    IonInput,
    IonList,
    IonIcon,
    IonText,
    IonCheckbox,
    IonSpinner
  ]
})
export class CreateSuggestionModalComponent implements OnInit {
  // Új bolt adatai
  newShop = {
    name: '',
    address: '',
    city: ''
  };

  isSubmitting = false;
  errorMessage = '';

  // Nyitvatartás
  openingHours: OpeningHourInput[] = [
    { dayOfWeek: 'Hétfő', openHour: '09:00', closeHour: '18:00', isClosed: false },
    { dayOfWeek: 'Kedd', openHour: '09:00', closeHour: '18:00', isClosed: false },
    { dayOfWeek: 'Szerda', openHour: '09:00', closeHour: '18:00', isClosed: false },
    { dayOfWeek: 'Csütörtök', openHour: '09:00', closeHour: '18:00', isClosed: false },
    { dayOfWeek: 'Péntek', openHour: '09:00', closeHour: '18:00', isClosed: false },
    { dayOfWeek: 'Szombat', openHour: '10:00', closeHour: '14:00', isClosed: false },
    { dayOfWeek: 'Vasárnap', openHour: '', closeHour: '', isClosed: true }
  ];

  constructor(
    private modalController: ModalController,
    private suggestionService: SuggestionService
  ) {
    addIcons({ closeOutline, storefrontOutline, timeOutline });
  }

  ngOnInit() {}

  isFormValid(): boolean {
    return !!(this.newShop.name && this.newShop.address && this.newShop.city);
  }

  dismiss() {
    this.modalController.dismiss();
  }

  submitSuggestion() {
    if (!this.isFormValid() || this.isSubmitting) return;

    this.isSubmitting = true;
    this.errorMessage = '';

    const suggestionData: CreateSuggestionRequest = {
      type: 'new_shop',
      proposedValue: this.newShop.name,
      additionalData: {
        address: this.newShop.address,
        city: this.newShop.city,
        openingHours: this.openingHours.map(oh => ({
          dayOfWeek: oh.dayOfWeek,
          openHour: oh.isClosed ? 'Zárva' : oh.openHour,
          closeHour: oh.isClosed ? '' : oh.closeHour
        }))
      }
    };

    this.suggestionService.createSuggestion(suggestionData).subscribe({
      next: (suggestion) => {
        console.log('Javaslat létrehozva:', suggestion);
        this.modalController.dismiss(suggestion, 'submit');
      },
      error: (error) => {
        console.error('Hiba a javaslat létrehozásakor:', error);
        this.errorMessage = error.error?.message || 'Nem sikerült elküldeni a javaslatot.';
        this.isSubmitting = false;
      }
    });
  }
}

