import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonSearchbar,
  IonList,
  IonItem,
  IonLabel,
  IonCheckbox,
  IonIcon,
  IonText,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardSubtitle,
  IonChip,
  IonRange,
  IonButton,
  IonSpinner
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  listOutline,
  locationOutline,
  checkmarkCircle,
  searchOutline,
  filterOutline,
  chevronDownOutline,
  chevronUpOutline,
  syncOutline
} from 'ionicons/icons';
import { ShopDetailsModalComponent } from '../../components/shop-details-modal/shop-details-modal.component';
import { ShopService, Shop } from '../../services/shop.service';

@Component({
  selector: 'app-list',
  templateUrl: './list.page.html',
  styleUrls: ['./list.page.scss'],
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonSearchbar,
    IonList,
    IonItem,
    IonLabel,
    IonCheckbox,
    IonIcon,
    IonText,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardSubtitle,
    IonChip,
    IonRange,
    IonButton,
    IonSpinner,
    CommonModule,
    FormsModule,
    ShopDetailsModalComponent
  ]
})
export class ListPage implements OnInit {
  searchText: string = '';
  filterCigars: boolean = false;
  maxDistance: number = 10;
  showFilters: boolean = false;
  isLoading: boolean = false;
  hasSearched: boolean = false;

  userLocation: { latitude: number; longitude: number } | null = null;

  filteredShops: Shop[] = [];

  // Modal állapot
  isModalOpen: boolean = false;
  selectedShopId: number | null = null;

  constructor(private shopService: ShopService) {
    addIcons({
      listOutline,
      locationOutline,
      checkmarkCircle,
      searchOutline,
      filterOutline,
      chevronDownOutline,
      chevronUpOutline,
      syncOutline
    });
  }

  ngOnInit() {
    // GPS pozíció lekérése
    this.getUserLocation();
  }

  // GPS pozíció megszerzése
  getUserLocation() {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          this.userLocation = {
            latitude: position.coords.latitude,
            longitude: position.coords.longitude
          };
          console.log('User location:', this.userLocation);
        },
        (error) => {
          console.error('Geolocation error:', error);
          // Fallback: Budapest központ
          this.userLocation = {
            latitude: 47.4979,
            longitude: 19.0402
          };
          console.log('Using fallback location (Budapest)');
        }
      );
    } else {
      // Fallback ha nincs geolocation
      this.userLocation = {
        latitude: 47.4979,
        longitude: 19.0402
      };
      console.log('Geolocation not supported, using fallback location');
    }
  }

  // Szűrők megjelenítése/elrejtése
  toggleFilters() {
    this.showFilters = !this.showFilters;
  }

  // Keresés indítása (gombra kattintva)
  async searchShops() {
    if (!this.userLocation) {
      console.error('User location not available yet');
      // Várunk egy kicsit és újra próbáljuk
      setTimeout(() => this.searchShops(), 500);
      return;
    }

    this.isLoading = true;
    this.hasSearched = true;

    // API hívás paraméterek
    const searchParams = {
      latitude: this.userLocation.latitude,
      longitude: this.userLocation.longitude,
      maxDistance: this.maxDistance,
      hasCigars: this.filterCigars ? true : undefined,
      search: this.searchText || undefined,
      limit: 20,
      offset: 0
    };

    // Valódi API hívás
    this.shopService.searchShops(searchParams).subscribe({
      next: (response) => {
        console.log('API Response:', response); // Debug log

        if (response && response.success && Array.isArray(response.data)) {
          this.filteredShops = response.data;
          console.log(`Found ${response.total} shops`);
        } else {
          console.error('Search failed or invalid response format:', response);
          this.filteredShops = [];
        }
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Search error:', error);
        this.filteredShops = [];
        this.isLoading = false;
        // TODO: Hibaüzenet megjelenítése a felhasználónak
      }
    });
  }

  // Bolt részleteinek megjelenítése modal-ban
  openShopDetails(shop: Shop) {
    console.log('Bolt részletei:', shop);
    this.selectedShopId = shop.id;
    this.isModalOpen = true;
  }

  // Modal bezárása
  closeModal() {
    this.isModalOpen = false;
    this.selectedShopId = null;
  }
}
