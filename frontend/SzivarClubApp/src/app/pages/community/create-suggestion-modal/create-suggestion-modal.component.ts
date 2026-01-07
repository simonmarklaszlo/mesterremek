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
  ModalController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { closeOutline, storefrontOutline, timeOutline } from 'ionicons/icons';

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
    IonCheckbox
  ]
})
export class CreateSuggestionModalComponent implements OnInit {
  // Új bolt adatai
  newShop = {
    name: '',
    address: '',
    city: ''
  };

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

  constructor(private modalController: ModalController) {
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
    if (!this.isFormValid()) return;

    const suggestionData = {
      type: 'new_shop',
      proposedValue: this.newShop.name,
      shopAddress: this.newShop.address,
      city: this.newShop.city,
      openingHours: this.openingHours.map(oh => ({
        dayOfWeek: oh.dayOfWeek,
        openHour: oh.isClosed ? 'Zárva' : oh.openHour,
        closeHour: oh.isClosed ? '' : oh.closeHour
      }))
    };

    console.log('Javaslat beküldése:', suggestionData);

    // TODO: API hívás
    // this.shopService.createSuggestion(suggestionData).subscribe(...)

    this.modalController.dismiss(suggestionData, 'submit');
  }
}

