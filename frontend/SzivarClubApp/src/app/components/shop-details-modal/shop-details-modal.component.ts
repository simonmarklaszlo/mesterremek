import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonButtons,
  IonButton,
  IonIcon,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonChip,
  IonText,
  IonSpinner,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  ToastController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  closeOutline,
  locationOutline,
  timeOutline,
  starOutline,
  star,
  checkmarkCircle,
  closeCircle,
  mapOutline,
  createOutline
} from 'ionicons/icons';
import { ShopService, ShopDetails } from '../../services/shop.service';
import { ModalController } from '@ionic/angular/standalone';
import { EditSuggestionModalComponent } from '../edit-suggestion-modal/edit-suggestion-modal.component';

@Component({
  selector: 'app-shop-details-modal',
  templateUrl: './shop-details-modal.component.html',
  styleUrls: ['./shop-details-modal.component.scss'],
  standalone: true,
  imports: [
    IonModal,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonIcon,
    IonContent,
    IonList,
    IonItem,
    IonLabel,
    IonChip,
    IonText,
    IonSpinner,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    CommonModule
  ]
})
export class ShopDetailsModalComponent implements OnInit, OnChanges {
  @Input() shopId!: number;
  @Input() isOpen: boolean = false;
  @Output() close = new EventEmitter<void>();

  shopDetails: ShopDetails | null = null;
  isLoading: boolean = false;
  errorMessage: string = '';
  isReceivedCigarSubmitting = false;

  // Math objektum elérhetővé tétele a template számára
  Math = Math;

  constructor(
    private shopService: ShopService,
    private modalCtrl: ModalController,
    private toastCtrl: ToastController
  ) {
    addIcons({
      closeOutline,
      locationOutline,
      timeOutline,
      starOutline,
      star,
      checkmarkCircle,
      closeCircle,
      mapOutline,
      createOutline
    });
  }

  ngOnInit() {
    if (this.shopId && this.isOpen) {
      this.loadShopDetails();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    // If modal closes, clear error message and reset state
    if (changes['isOpen'] && !this.isOpen && changes['isOpen'].previousValue) {
      this.errorMessage = '';
      this.shopDetails = null;
      this.isLoading = false;
    }
    // Ha a modal megnyílik vagy a shopId megváltozik, töltsd újra az adatokat
    if ((changes['isOpen'] && this.isOpen && this.shopId) ||
        (changes['shopId'] && this.shopId && this.isOpen)) {
      this.loadShopDetails();
    }
  }

  loadShopDetails() {
    this.isLoading = true;
    this.errorMessage = '';

    // Valódi API hívás
    this.shopService.getShopDetails(this.shopId).subscribe({
      next: (response) => {
        console.log('Shop details response:', response);
        if (response && response.success && response.data) {
          this.shopDetails = response.data;
        } else {
          this.errorMessage = 'Nem sikerült betölteni a bolt adatait.';
          console.error('Invalid shop details response:', response);
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Shop details error:', error);
        this.errorMessage = 'Hiba történt az adatok betöltése során.';
        this.isLoading = false;
      }
    });
  }

  closeModal() {
    // Only emit event - parent handles state changes
    this.close.emit();
  }

  // Csillagok tömbje az értékeléshez
  getStars(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

  // Dátum formázása
  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('hu-HU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  // Módosítási javaslat modal megnyitása
  async openEditModal() {
    if (!this.shopDetails) return;

    const modal = await this.modalCtrl.create({
      component: EditSuggestionModalComponent,
      componentProps: {
        shopDetails: this.shopDetails
      }
    });

    modal.onDidDismiss().then((result) => {
      if (result.role === 'submit' && result.data) {
        console.log('Módosítási javaslat beküldve:', result.data);
        // Opcionálisan: Toast üzenet vagy értesítés
      }
    });

    return await modal.present();
  }

  openInGoogleMaps(lat: number | undefined, lng: number | undefined, label?: string, city?: string, address?: string) {
    if (!lat || !lng) return;
    let query = '';
    if (city && address) {
      query = encodeURIComponent(`${city} ${address}`);
    } else if (address) {
      query = encodeURIComponent(address);
    } else if (label) {
      query = encodeURIComponent(label);
    } else {
      query = encodeURIComponent(`${lat},${lng}`);
    }
    const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
    const ua = navigator.userAgent || '';
    const isAndroid = /android/i.test(ua);
    const isIOS = /iPhone|iPad|iPod/i.test(ua);
    if (isAndroid) {
      const intentUrl = `intent://maps.google.com/maps?daddr=${query}#Intent;package=com.google.android.apps.maps;scheme=https;end`;
      try {
        window.location.href = intentUrl;
        setTimeout(() => { window.location.href = webUrl; }, 1200);
      } catch { window.open(webUrl, '_blank'); }
      return;
    }
    if (isIOS) {
      const appleScheme = `maps://?q=${query}`;
      try { window.location.href = appleScheme; } catch { window.open(webUrl, '_blank'); }
      return;
    }
    window.open(webUrl, '_blank');
  }

  async onReceivedCigarClick() {
    if (!this.shopId || this.isReceivedCigarSubmitting) return;

    this.isReceivedCigarSubmitting = true;
    this.shopService.receivedCigar(this.shopId).subscribe({
      next: async () => {
        const toast = await this.toastCtrl.create({
          message: 'Rögzítve: kaptam szivart',
          duration: 2000,
          color: 'success',
          position: 'top'
        });
        await toast.present();
        this.isReceivedCigarSubmitting = false;
      },
      error: async (err) => {
        console.error('receivedCigar error:', err);
        const toast = await this.toastCtrl.create({
          message: 'Nem sikerült rögzíteni',
          duration: 2500,
          color: 'danger',
          position: 'top'
        });
        await toast.present();
        this.isReceivedCigarSubmitting = false;
      }
    });
  }
}
