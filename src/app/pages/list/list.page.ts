import { Component, OnInit, OnDestroy } from '@angular/core';
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
import { SearchService } from '../../services/search.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

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
export class ListPage implements OnInit, OnDestroy {
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

  private destroy$ = new Subject<void>();
  private isUpdatingFromSync = false;

  constructor(private shopService: ShopService, private searchService: SearchService) {
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

    // search servive státuszára update
    this.searchService.searchState$
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        this.isUpdatingFromSync = true;
        this.searchText = state.searchText;
        this.filterCigars = state.filterCigars;
        this.maxDistance = state.maxDistance;
        this.isUpdatingFromSync = false;
      });

    // Trigger ha a map oldalon történt keresés
    this.searchService.searchTriggered$
      .pipe(takeUntil(this.destroy$))
      .subscribe((trigger) => {
        // Only respond if the search was triggered from the map page
        if (trigger.source === 'map') {
          this.searchText = trigger.searchText;
          this.filterCigars = trigger.filterCigars;
          this.maxDistance = trigger.maxDistance;
          // Execute search without triggering back to prevent loops
          this.executeSearch(false);
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
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
    // Only trigger if this is a manual search (not from a trigger)
    const triggerOtherPage = true;
    await this.executeSearch(triggerOtherPage);
  }

  // Actual search execution
  private async executeSearch(triggerOtherPage: boolean = false) {
    if (!this.userLocation) {
      console.error('User location not available yet');
      // Várunk egy kicsit és újra próbáljuk
      setTimeout(() => this.executeSearch(triggerOtherPage), 500);
      return;
    }

    this.isLoading = true;
    this.hasSearched = true;

    const safeSearch = this.sanitizeSearchText(this.searchText);

    // API hívás paraméterek
    const searchParams = {
      latitude: this.userLocation.latitude,
      longitude: this.userLocation.longitude,
      maxDistance: this.maxDistance,
      hasCigars: this.filterCigars ? true : undefined,
      search: safeSearch || undefined,
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

        // Ha kész a keresés, másik oldal triggerelése
        if (triggerOtherPage) {
          this.searchService.triggerSearch('list', {
            searchText: this.searchText,
            filterCigars: this.filterCigars,
            maxDistance: this.maxDistance
          });
        }
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

   private sanitizeSearchText(value: string): string {
    if (!value) return '';
    const trimmed = value.trim().normalize('NFKC');
    if (!trimmed) return '';

    // Allow letters, numbers, spaces and common punctuation used in names/addresses.
    const safe = trimmed.replace(/[^\p{L}\p{N}\s'’\-.,/]/gu, '');
    const collapsed = safe.replace(/\s+/g, ' ').trim();
    const maxLen = 100;
    return collapsed.slice(0, maxLen);
  }
}
