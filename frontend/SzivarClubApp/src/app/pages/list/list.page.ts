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

interface Shop {
  id: number;
  name: string;
  address: string;
  city: string;
  distance: number;
  hasCigars: boolean;
}

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
    FormsModule
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

  constructor() {
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

    // Szimulált API hívás (később valódi API lesz)
    // TODO: Helyettesítsd ezt az AuthService-hez hasonló ShopService-szel
    setTimeout(() => {
      // Példa adatok (később API-ból jön)
      const allShops: Shop[] = [
        {
          id: 1,
          name: 'Nemzeti Dohánybolt',
          address: 'Andrássy út 42',
          city: 'Budapest',
          distance: 0.8,
          hasCigars: true
        },
        {
          id: 2,
          name: 'Tabán Dohány',
          address: 'Tabán utca 15',
          city: 'Budapest',
          distance: 1.2,
          hasCigars: true
        },
        {
          id: 3,
          name: 'Dohánybolt Westend',
          address: 'Váci út 1-3',
          city: 'Budapest',
          distance: 2.5,
          hasCigars: false
        },
        {
          id: 4,
          name: 'Szivar Sziget',
          address: 'Margit körút 88',
          city: 'Budapest',
          distance: 3.1,
          hasCigars: true
        },
        {
          id: 5,
          name: 'Premium Tobacco',
          address: 'Kossuth Lajos utca 10',
          city: 'Budapest',
          distance: 4.5,
          hasCigars: true
        },
        {
          id: 6,
          name: 'Dohány Pont',
          address: 'Rákóczi út 25',
          city: 'Budapest',
          distance: 5.2,
          hasCigars: false
        },
        {
          id: 7,
          name: 'City Tobacco',
          address: 'Deák Ferenc tér 3',
          city: 'Budapest',
          distance: 6.8,
          hasCigars: true
        },
        {
          id: 8,
          name: 'Oktogon Dohány',
          address: 'Oktogon tér 1',
          city: 'Budapest',
          distance: 8.3,
          hasCigars: false
        }
      ];

      // Szűrések alkalmazása (kliens oldali - később szerver oldali lesz)
      this.filteredShops = allShops.filter(shop => {
        const searchMatch = !this.searchText ||
          shop.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
          shop.address.toLowerCase().includes(this.searchText.toLowerCase()) ||
          shop.city.toLowerCase().includes(this.searchText.toLowerCase());

        const cigarMatch = !this.filterCigars || shop.hasCigars;
        const distanceMatch = shop.distance <= this.maxDistance;

        return searchMatch && cigarMatch && distanceMatch;
      });

      this.filteredShops.sort((a, b) => a.distance - b.distance);

      this.isLoading = false;
      console.log(`Found ${this.filteredShops.length} shops`);
    }, 1000); // 1 másodperces delay a szimulációhoz
  }

  // Bolt részleteinek megjelenítése
  openShopDetails(shop: Shop) {
    console.log('Bolt részletei:', shop);
    // TODO: Navigálás a bolt részletes oldalára
  }
}

