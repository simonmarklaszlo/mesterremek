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
  IonCardContent
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  closeOutline,
  locationOutline,
  timeOutline,
  starOutline,
  star,
  checkmarkCircle,
  closeCircle
} from 'ionicons/icons';
import { ShopService, ShopDetails } from '../../services/shop.service';

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
  @Output() didDismiss = new EventEmitter<void>();

  shopDetails: ShopDetails | null = null;
  isLoading: boolean = false;
  errorMessage: string = '';

  // Math objektum elérhetővé tétele a template számára
  Math = Math;

  constructor(private shopService: ShopService) {
    addIcons({
      closeOutline,
      locationOutline,
      timeOutline,
      starOutline,
      star,
      checkmarkCircle,
      closeCircle
    });
  }

  ngOnInit() {
    if (this.shopId && this.isOpen) {
      this.loadShopDetails();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
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
    this.isOpen = false;
    this.didDismiss.emit();
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
}
