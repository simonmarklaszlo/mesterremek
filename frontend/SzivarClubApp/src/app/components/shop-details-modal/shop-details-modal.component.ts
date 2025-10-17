import { Component, Input, OnInit } from '@angular/core';
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

interface OpeningHour {
  id: number;
  dayOfWeek: string;
  openHour: string;
  closeHour: string;
}

interface CigarBrand {
  id: number;
  brandId: number;
  brandName: string;
  addedBy: number;
  createdAt: string;
}

interface Review {
  id: number;
  userId: number;
  userName: string;
  rating: number;
  comment: string;
  createdAt: string;
}

interface ShopDetails {
  id: number;
  name: string;
  address: string;
  city: string;
  latitude: number;
  longitude: number;
  createdAt: string;
  updatedAt: string;
  openingHours: OpeningHour[];
  cigarBrands: CigarBrand[];
  reviews: Review[];
  averageRating: number;
  totalReviews: number;
  isOpenNow: boolean;
  nextClosingTime: string | null;
}

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
export class ShopDetailsModalComponent implements OnInit {
  @Input() shopId!: number;
  @Input() isOpen: boolean = false;

  shopDetails: ShopDetails | null = null;
  isLoading: boolean = false;
  errorMessage: string = '';

  // Math objektum elérhetővé tétele a template számára
  Math = Math;

  constructor() {
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
    if (this.shopId) {
      this.loadShopDetails();
    }
  }

  loadShopDetails() {
    this.isLoading = true;
    this.errorMessage = '';

    // Szimulált API hívás (később ShopService-szel)
    // TODO: this.shopService.getShopDetails(this.shopId).subscribe(...)
    setTimeout(() => {
      // Példa adatok
      this.shopDetails = {
        id: this.shopId,
        name: 'Nemzeti Dohánybolt',
        address: 'Andrássy út 42',
        city: 'Budapest',
        latitude: 47.5028,
        longitude: 19.0620,
        createdAt: '2024-01-15T10:30:00Z',
        updatedAt: '2024-10-10T14:20:00Z',
        openingHours: [
          { id: 1, dayOfWeek: 'Hétfő', openHour: '08:00', closeHour: '20:00' },
          { id: 2, dayOfWeek: 'Kedd', openHour: '08:00', closeHour: '20:00' },
          { id: 3, dayOfWeek: 'Szerda', openHour: '08:00', closeHour: '20:00' },
          { id: 4, dayOfWeek: 'Csütörtök', openHour: '08:00', closeHour: '20:00' },
          { id: 5, dayOfWeek: 'Péntek', openHour: '08:00', closeHour: '21:00' },
          { id: 6, dayOfWeek: 'Szombat', openHour: '09:00', closeHour: '18:00' },
          { id: 7, dayOfWeek: 'Vasárnap', openHour: '10:00', closeHour: '16:00' }
        ],
        cigarBrands: [
          { id: 1, brandId: 5, brandName: 'Cohiba', addedBy: 12, createdAt: '2024-02-10T09:15:00Z' },
          { id: 2, brandId: 8, brandName: 'Montecristo', addedBy: 12, createdAt: '2024-02-10T09:16:00Z' },
          { id: 3, brandId: 15, brandName: 'Romeo y Julieta', addedBy: 23, createdAt: '2024-03-05T14:30:00Z' }
        ],
        reviews: [
          {
            id: 45,
            userId: 12,
            userName: 'Kiss János',
            rating: 5,
            comment: 'Kiváló kínálat, kedves kiszolgálás!',
            createdAt: '2024-09-20T15:30:00Z'
          },
          {
            id: 46,
            userId: 23,
            userName: 'Nagy Péter',
            rating: 4,
            comment: 'Jó árak, széles választék.',
            createdAt: '2024-09-25T11:20:00Z'
          }
        ],
        averageRating: 4.5,
        totalReviews: 23,
        isOpenNow: true,
        nextClosingTime: '20:00'
      };
      this.isLoading = false;
    }, 800);
  }

  closeModal() {
    this.isOpen = false;
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
