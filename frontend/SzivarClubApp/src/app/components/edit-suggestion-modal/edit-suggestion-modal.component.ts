import { Component, Input, OnInit } from '@angular/core';
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
  IonSelect,
  IonSelectOption,
  IonTextarea,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  ModalController,
  ToastController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  closeOutline,
  storefrontOutline,
  timeOutline,
  locationOutline,
  createOutline,
  alertCircleOutline,
  checkmarkCircleOutline
} from 'ionicons/icons';
import { ShopDetails } from '../../services/shop.service';
import { SuggestionService } from '../../services/suggestion.service';
import { CreateSuggestionRequest } from '../../models/suggestion.model';

interface OpeningHourInput {
  dayOfWeek: string;
  dayId: number;
  openHour: string;
  closeHour: string;
  isClosed: boolean;
}

type SuggestionType = 'edit_name' | 'edit_address' | 'edit_hours';

@Component({
  selector: 'app-edit-suggestion-modal',
  templateUrl: './edit-suggestion-modal.component.html',
  styleUrls: ['./edit-suggestion-modal.component.scss'],
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
    IonSelect,
    IonSelectOption,
    IonTextarea,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent
  ]
})
export class EditSuggestionModalComponent implements OnInit {
  @Input() shopDetails!: ShopDetails;

  // Választott típus
  selectedType: SuggestionType = 'edit_name';

  // Név módosítás
  newName: string = '';

  // Cím módosítás
  newAddress: string = '';
  newCity: string = '';

  // Nyitvatartás módosítás
  openingHours: OpeningHourInput[] = [];

  // Állapotkezelés
  isSubmitting = false;
  errorMessage = '';

  // Típus leírások
  typeDescriptions = {
    edit_name: 'A bolt nevének javítása vagy frissítése',
    edit_address: 'A bolt címének és városának módosítása',
    edit_hours: 'A nyitvatartási idő frissítése'
  };

  constructor(
    private modalController: ModalController,
    private toastController: ToastController,
    private suggestionService: SuggestionService
  ) {
    addIcons({
      closeOutline,
      storefrontOutline,
      timeOutline,
      locationOutline,
      createOutline,
      alertCircleOutline,
      checkmarkCircleOutline
    });
  }

  ngOnInit() {
    // Alapértelmezett értékek kitöltése
    this.newName = this.shopDetails.name;
    this.newAddress = this.shopDetails.address;
    this.newCity = this.shopDetails.city;

    // Nyitvatartás inicializálása
    this.initializeOpeningHours();
  }

  initializeOpeningHours() {
    const daysOfWeek = [
      { id: 1, name: 'Hétfő' },
      { id: 2, name: 'Kedd' },
      { id: 3, name: 'Szerda' },
      { id: 4, name: 'Csütörtök' },
      { id: 5, name: 'Péntek' },
      { id: 6, name: 'Szombat' },
      { id: 7, name: 'Vasárnap' }
    ];

    this.openingHours = daysOfWeek.map(day => {
      const existingHour = this.shopDetails.openingHours.find(
        oh => oh.dayOfWeek === day.name
      );

      if (existingHour) {
        return {
          dayOfWeek: day.name,
          dayId: day.id,
          openHour: existingHour.openHour || '',
          closeHour: existingHour.closeHour || '',
          isClosed: !existingHour.openHour || existingHour.openHour === 'Zárva'
        };
      }

      return {
        dayOfWeek: day.name,
        dayId: day.id,
        openHour: '09:00',
        closeHour: '18:00',
        isClosed: false
      };
    });
  }

  onTypeChange(event: any) {
    this.selectedType = event.detail.value;
  }

  isFormValid(): boolean {
    switch (this.selectedType) {
      case 'edit_name':
        return !!(this.newName && this.newName.trim() && this.newName !== this.shopDetails.name);

      case 'edit_address':
        return !!(
          this.newAddress &&
          this.newAddress.trim() &&
          this.newCity &&
          this.newCity.trim() &&
          (this.newAddress !== this.shopDetails.address || this.newCity !== this.shopDetails.city)
        );

      case 'edit_hours':
        // Ellenőrizzük, hogy legalább egy nap ki van töltve
        const hasValidHours = this.openingHours.some(oh =>
          !oh.isClosed && oh.openHour && oh.closeHour
        );
        return hasValidHours;

      default:
        return false;
    }
  }

  getTypeTitle(): string {
    switch (this.selectedType) {
      case 'edit_name': return 'Név módosítása';
      case 'edit_address': return 'Cím módosítása';
      case 'edit_hours': return 'Nyitvatartás módosítása';
      default: return 'Módosítás';
    }
  }

  async dismiss() {
    await this.modalController.dismiss();
  }

  async submitSuggestion() {
    if (!this.isFormValid() || this.isSubmitting) {
      await this.showToast('Kérlek töltsd ki az összes kötelező mezőt!', 'warning');
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    let suggestionData: CreateSuggestionRequest;

    switch (this.selectedType) {
      case 'edit_name':
        suggestionData = {
          type: 'edit_name',
          shopId: this.shopDetails.id,
          proposedValue: this.newName.trim()
        };
        break;

      case 'edit_address':
        suggestionData = {
          type: 'edit_address',
          shopId: this.shopDetails.id,
          proposedValue: this.newAddress.trim(),
          additionalData: {
            city: this.newCity.trim()
          }
        };
        break;

      case 'edit_hours':
        suggestionData = {
          type: 'edit_hours',
          shopId: this.shopDetails.id,
          proposedValue: 'Nyitvatartás módosítása',
          additionalData: {
            openingHours: this.openingHours.map(oh => ({
              dayOfWeek: oh.dayOfWeek,
              openHour: oh.isClosed ? 'Zárva' : oh.openHour,
              closeHour: oh.isClosed ? '' : oh.closeHour
            }))
          }
        };
        break;

      default:
        this.isSubmitting = false;
        return;
    }

    this.suggestionService.createSuggestion(suggestionData).subscribe({
      next: async (suggestion) => {
        console.log('Módosítási javaslat létrehozva:', suggestion);
        await this.showToast('Javaslat sikeresen beküldve! ✓', 'success');
        await this.modalController.dismiss(suggestion, 'submit');
      },
      error: async (error) => {
        console.error('Hiba a javaslat létrehozásakor:', error);
        this.errorMessage = error.error?.message || 'Nem sikerült elküldeni a javaslatot.';
        await this.showToast(this.errorMessage, 'danger');
        this.isSubmitting = false;
      }
    });
  }

  async showToast(message: string, color: string) {
    const toast = await this.toastController.create({
      message,
      duration: 2000,
      color,
      position: 'bottom'
    });
    await toast.present();
  }
}

