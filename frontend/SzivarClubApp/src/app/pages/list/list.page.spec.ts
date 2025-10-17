import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ListPage } from './list.page';

describe('ListPage', () => {
  let component: ListPage;
  let fixture: ComponentFixture<ListPage>;

  beforeEach(() => {
    fixture = TestBed.createComponent(ListPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
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
  IonRange
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { listOutline, locationOutline, checkmarkCircle, searchOutline } from 'ionicons/icons';

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
    CommonModule,
    FormsModule
  ]
})
export class ListPage implements OnInit {
  searchText: string = '';
  filterCigars: boolean = false;
  maxDistance: number = 10;

  allShops: Shop[] = [
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

  filteredShops: Shop[] = [];

  constructor() {
    addIcons({ listOutline, locationOutline, checkmarkCircle, searchOutline });
  }

  ngOnInit() {
    this.applyFilters();
  }

  applyFilters() {
    this.filteredShops = this.allShops.filter(shop => {
      const searchMatch = !this.searchText ||
        shop.name.toLowerCase().includes(this.searchText.toLowerCase()) ||
        shop.address.toLowerCase().includes(this.searchText.toLowerCase()) ||
        shop.city.toLowerCase().includes(this.searchText.toLowerCase());

      const cigarMatch = !this.filterCigars || shop.hasCigars;
      const distanceMatch = shop.distance <= this.maxDistance;

      return searchMatch && cigarMatch && distanceMatch;
    });

    this.filteredShops.sort((a, b) => a.distance - b.distance);
  }

  onSearchChange(event: any) {
    this.searchText = event.detail.value || '';
    this.applyFilters();
  }

  onDistanceChange(event: any) {
    this.maxDistance = event.detail.value;
    this.applyFilters();
  }

  onCigarFilterChange() {
    this.applyFilters();
  }

  openShopDetails(shop: Shop) {
    console.log('Bolt részletei:', shop);
  }
}

