// #region Imports and Setup
import { Component, AfterViewInit, OnDestroy, NgZone, ChangeDetectorRef, ApplicationRef, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { IonInput, IonButton, IonIcon, IonHeader, IonToolbar, IonTitle,
         IonLabel, IonChip, IonText, IonSpinner,
         IonCheckbox,
         IonFab, IonFabButton,
         IonList, IonItem,
         IonCard, IonCardHeader, IonCardTitle, IonCardContent,
         IonSearchbar, IonRange } from '@ionic/angular/standalone';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { forkJoin, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as L from 'leaflet';
import { Capacitor } from '@capacitor/core';
import { Geolocation } from '@capacitor/geolocation';
import { addIcons } from 'ionicons';
import { radioButtonOn, locationSharp, storefront, search, locate, arrowBack,
         timeOutline, starOutline, star, checkmarkCircle, closeCircle, locationOutline, mapOutline,
         filterOutline, chevronDownOutline, chevronUpOutline, syncOutline } from 'ionicons/icons';
import { ThemeService } from '../../services/theme.service';
import { ErrorLogService } from '../../services/error-log.service';
import { SearchService } from '../../services/search.service';
import { ShopService, ShopDetails } from '../../services/shop.service';
import { ShopDetailsModalComponent } from '../../components/shop-details-modal/shop-details-modal.component';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { environment } from '../../../environments/environment';
import { ToastController, AlertController, IonToggle } from '@ionic/angular/standalone';

// Fix Leaflet marker paths
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'assets/leaflet/images/marker-icon-2x.png',
  iconUrl: 'assets/leaflet/images/marker-icon.png',
  shadowUrl: 'assets/leaflet/images/marker-shadow.png'
});
// #endregion

// #region Component Decorator
@Component({
  selector: 'app-map',
  standalone: true,
  templateUrl: 'map.page.html',
  styleUrls: ['map.page.scss'],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  imports: [
    CommonModule,
    IonInput, IonButton, IonIcon, IonHeader, IonToolbar, IonTitle,
    IonLabel, IonChip, IonText, IonSpinner,
    IonCheckbox,
    IonFab, IonFabButton,
    IonList, IonItem,
    IonCard, IonCardHeader, IonCardTitle, IonCardContent,
    IonSearchbar, IonRange,
    FormsModule,
    ShopDetailsModalComponent,
    IonToggle
  ]
})
// #endregion

// #region Class Definition and Properties
export class MapPage implements AfterViewInit, OnDestroy, OnInit {
  map?: L.Map;
  Varos_Coords: any[] = [];
  Shop_Data: any[] = [];
  isThemeDark = true;
  userLat = 0;
  userLong = 0;
  searchText: string = '';
  filterCigars: boolean = false;
  maxDistance: number = 10;
  useDistance: boolean = true;
  showFilters: boolean = false;
  isLoading: boolean = false;
  private currentTheme = true;

  // Modal state for mobile
  isModalOpen: boolean = false;
  selectedShopId: number = 0;

  // Window width for responsive design
  windowWidth: number = window.innerWidth;
  windowHeight: number = window.innerHeight;
  isWideLayout: boolean = (window.innerWidth / window.innerHeight) > 1;

  // Sidebar state (remade)
  sidebar: { type: 'list', data: any[] } | { type: 'details', data: any } | null = null;

  // Detailed shop data for sidebar
  sidebarShopDetails: ShopDetails | null = null;
  isSidebarLoading: boolean = false;
  sidebarErrorMessage: string = '';

  // Math object for template
  Math = Math;

  private tileLayer?: L.TileLayer;
  private shopMarkersLayer?: L.LayerGroup;
  private clusterMarkersLayer?: L.LayerGroup;
  private userMarker?: L.Marker;
  private shopMarkerIndex: Map<string, L.Marker> = new Map();
  private pendingOpenShop?: { lat: number; lng: number };
  private destroy$ = new Subject<void>();

  private keyFor(lat: number, lng: number): string {
    return `${lat.toFixed(6)},${lng.toFixed(6)}`;
  }

  private storeIcon = L.divIcon({
    className: 'store-marker',
    html: '<ion-icon name="storefront" style="font-size:16px; color: #bbbbbb;"></ion-icon>',
    iconSize: [25, 25],
    iconAnchor: [16, 16],
    popupAnchor: [0, -16]
  });

  private UserIcon = L.divIcon({
    className: 'user-location-marker',
    html: '<ion-icon name="radio-button-on" style="font-size: 20px; color: var(--ion-color-primary);"></ion-icon>',
    iconSize: [28, 28],
    iconAnchor: [14, 14],
    popupAnchor: [0, -16]
  });

  hungaryBounds: L.LatLngBoundsExpression = [[45.637, 16.113], [48.685, 22.897]];
  // #endregion

  // #region Constructor and Initialization
  constructor(
    private http: HttpClient,
    private theme: ThemeService,
    private errorLog: ErrorLogService,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef,
    private appRef: ApplicationRef,
    private sanitizer: DomSanitizer,
    private searchService: SearchService,
    private shopService: ShopService,
    private toastCtrl: ToastController,
    private alertController: AlertController
  ) {
    addIcons({ radioButtonOn, locationSharp, storefront, search, locate, arrowBack,
               timeOutline, starOutline, star, checkmarkCircle, closeCircle, locationOutline, mapOutline,
               filterOutline, chevronDownOutline, chevronUpOutline, syncOutline });
    // Listen to window resize events
    window.addEventListener('resize', () => {
      this.windowWidth = window.innerWidth;
      this.windowHeight = window.innerHeight;

      const wasWideLayout = this.isWideLayout;
      const isWideLayoutNow = (this.windowWidth / this.windowHeight) > 1;
      this.isWideLayout = isWideLayoutNow;

      if (wasWideLayout !== isWideLayoutNow) {
        // Layout mode changed, need to fix map
        this.handleModeChange();
      } else if (this.map) {
        // Just a resize within the same mode
        this.map.invalidateSize();
      }

      this.cdr.detectChanges();
    });
  }
  // #endregion

  // #region Lifecycle Hooks
  private handleModeChange() {
    // Wait for DOM to update, then fix map
    setTimeout(() => {
      if (this.map) {
        // Force complete recalculation of map size and center
        this.map.invalidateSize(true);
        // Re-center the map to ensure it's properly positioned
        const currentCenter = this.map.getCenter();
        const currentZoom = this.map.getZoom();
        this.map.setView(currentCenter, currentZoom || 13);
        // Force a complete redraw
        this.updateMarkers();
      }
    }, 200);
  }

  ngOnInit() {
    // Set initial theme (already handled by ThemeService)
    this.isThemeDark = this.theme.isDark;
    this.currentTheme = this.theme.isDark;

    // Subscribe to theme changes
    this.theme.theme$
      .pipe(takeUntil(this.destroy$))
      .subscribe((isDark: boolean) => {
        this.isThemeDark = isDark;
        // Update tile layer if map is initialized and theme actually changed
        if (this.map && this.tileLayer && this.currentTheme !== isDark) {
          this.currentTheme = isDark;
          this.updateTileLayer(isDark);
        }
      });

    // Subscribe to search service changes for state sync
    this.searchService.searchState$
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        this.searchText = state.searchText;
        this.filterCigars = state.filterCigars;
        this.maxDistance = state.maxDistance;
        this.useDistance = state.useDistance;
      });

    // Subscribe to search triggers from list page
    this.searchService.searchTriggered$
      .pipe(takeUntil(this.destroy$))
      .subscribe((trigger) => {
        if (trigger.source === 'list') {
          this.searchText = trigger.searchText;
          this.filterCigars = trigger.filterCigars;
          this.maxDistance = trigger.maxDistance;
          this.useDistance = trigger.useDistance;
          // Execute search without triggering back to prevent loops
          this.executeVarosKereses(false);
        }
      });
  }

  ngAfterViewInit() {
    setTimeout(() => this.initMap(), 100);
  }
  // #endregion

  // #region Map Initialization
  private initMap() {
    this.map = L.map('map', {
      center: [47.4979, 19.0402],
      zoom: 13,
      maxZoom: 18,
      minZoom: 8,
      maxBounds: this.hungaryBounds,
      maxBoundsViscosity: 1.0
    });

    this.map.fitBounds(this.hungaryBounds);

    const url = this.isThemeDark
      ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
      : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';
    this.tileLayer = L.tileLayer(url, {
      attribution: '&copy; OpenStreetMap & CARTO',
      subdomains: ['a', 'b', 'c', 'd'],
      crossOrigin: true as any
    }).addTo(this.map);

    this.shopMarkersLayer = L.layerGroup().addTo(this.map);
    this.clusterMarkersLayer = L.layerGroup().addTo(this.map);

    this.map.on('zoomend', () => {
      this.updateMarkers();
      if (this.pendingOpenShop && (this.map?.getZoom() || 0) > 13) {
        this.openShopPopupNow(this.pendingOpenShop.lat, this.pendingOpenShop.lng);
        this.pendingOpenShop = undefined;
      }
    });

    this.User_Marker_Place();
    this.setMapHeight();
    window.addEventListener('resize', this.setMapHeightBound);
    window.addEventListener('orientationchange', this.setMapHeightBound);

    setTimeout(() => {
      this.map?.invalidateSize();
    }, 300);
  }
  // #endregion

  // #region User Interactions and Search
  centerOnUser() {
    if (!this.map) return;
    if (!this.userLat || !this.userLong || !this.userMarker) {
      // If user location not yet available, attempt to place it now
      this.User_Marker_Place();
      return;
    }
    this.map.setView([this.userLat, this.userLong], Math.max(this.map.getZoom() || 13, 13), { animate: true });
  }

  onFilterCigarsChanged() {
    // keep shared state updated so list <-> map stays synced
    this.searchService.updateFilterCigars(this.filterCigars);
  }

  toggleFilters() {
    this.showFilters = !this.showFilters;
  }

  searchShops() {
    this.varos_kereses();
  }

  varos_kereses(event?: Event) {
    // Manual search - trigger other page
    this.executeVarosKereses(true, event);
  }

  private executeVarosKereses(triggerOtherPage: boolean = false, event?: Event) {
    // Reset state
    this.Varos_Coords = [];
    this.Shop_Data = [];
    this.sidebar = null;

    // Best-effort search text (optional)
    let raw = this.searchText ?? '';
    if (!raw) {
      const el = document.getElementById('varos') as HTMLInputElement | null;
      raw = el?.value ?? '';
    }
    if (!raw && event) {
      const tgt = event.target as any;
      raw = tgt?.value ?? '';
    }
    const safeSearch = (raw ?? '').toString().trim();

    // Use map center, fallback to user location or default
    const lat = this.map?.getCenter().lat ?? (this.userLat && this.userLong ? this.userLat : 47.4979);
    const lng = this.map?.getCenter().lng ?? (this.userLat && this.userLong ? this.userLong : 19.0402);

    // Dynamic maxDistance calculation based on map view:
    // When zooming out significantly we should increase maxDistance so results show up,
    // otherwise the 10km-20km slider restriction will hide them.
    let searchDistance = this.useDistance ? this.maxDistance : 1000;
    if (this.map && !triggerOtherPage && this.useDistance) {
      const bounds = this.map.getBounds();
      const pt1 = this.map.project(bounds.getNorthEast(), this.map.getZoom());
      const pt2 = this.map.project(bounds.getSouthWest(), this.map.getZoom());
      // Increase search distance based on map bounds loosely when searching on map manually
      const distOnMapKm = this.map.distance(bounds.getNorthEast(), bounds.getSouthWest()) / 1000;
      if (distOnMapKm / 2 > searchDistance) {
        searchDistance = Math.min(Math.round(distOnMapKm / 2), 500); // max 500km
      }
    }

    // NOTE: map page uses the same search endpoint as list page so filters behave identically
    const params = {
      latitude: lat,
      longitude: lng,
      maxDistance: searchDistance,
      hasCigars: this.filterCigars ? true : undefined,
      search: safeSearch || undefined,
      limit: 200,
      offset: 0
    };

    this.isLoading = true;

    this.shopService.searchShops(params).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response && response.success && Array.isArray(response.data)) {
          this.Shop_Data = response.data as any[];
          this.Varos_Coords = this.Shop_Data
            .map((s) => [Number(s.latitude), Number(s.longitude)])
            .filter((c) => !Number.isNaN(c[0]) && !Number.isNaN(c[1]));

          this.updateMarkers();

          if (this.Shop_Data.length > 0) {
            this.showShopList(this.Shop_Data);
          }

          // Center map to first result if any
          if (this.Shop_Data.length > 0 && this.map) {
            const first = this.Shop_Data[0];
            const fLat = Number(first.latitude);
            const fLng = Number(first.longitude);
            if (!Number.isNaN(fLat) && !Number.isNaN(fLng)) {
              this.map.setView([fLat, fLng], Math.max(this.map.getZoom() || 13, 13));
            }
          }

          if (triggerOtherPage) {
            this.searchService.triggerSearch('map', {
              searchText: this.searchText,
              filterCigars: this.filterCigars,
              maxDistance: this.maxDistance,
              useDistance: this.useDistance
            });
          }
        } else {
          console.error('Map search invalid response:', response);
        }
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Map search error:', err);
      }
    });
  }
  // #endregion

  // #region Marker Management
  private updateMarkers() {
    if (!this.shopMarkersLayer || !this.clusterMarkersLayer || !this.map) return;
    this.shopMarkersLayer.clearLayers();
    this.clusterMarkersLayer.clearLayers();
    const currentZoom = this.map.getZoom() || 0;
    if (currentZoom <= 13) this.placeClusteredMarkers(); else this.placeIndividualMarkers();
  }

  private placeClusteredMarkers() {
    if (!this.clusterMarkersLayer) return;
    const clusters: any[] = [];
    const processed = new Set<number>();
    const coords = this.Shop_Data.map(s => [Number(s.latitude ?? s.lat), Number(s.longitude ?? s.lng)]);
    const currentZoom = this.map?.getZoom() || 0;
    let radiusKm = 3;
    if (currentZoom >= 13) radiusKm = 1;
    else if (currentZoom >= 11) radiusKm = 1.5;
    else if (currentZoom < 9) radiusKm = 6;

    const calculateDistance = (lat1:number, lon1:number, lat2:number, lon2:number) => {
      const R = 6371;
      const dLat = (lat2 - lat1) * Math.PI / 180;
      const dLon = (lon2 - lon1) * Math.PI / 180;
      const a = Math.sin(dLat/2) * Math.sin(dLat/2) + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon/2) * Math.sin(dLon/2);
      return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    };

    coords.forEach((coord, index) => {
      if (processed.has(index) || !coord || Number.isNaN(coord[0])) return;
      const cluster = { shops: [this.Shop_Data[index]], indices: [index] };
      coords.forEach((other, otherIndex) => {
        if (index === otherIndex || processed.has(otherIndex)) return;
        if (calculateDistance(coord[0], coord[1], other[0], other[1]) <= radiusKm) {
          cluster.shops.push(this.Shop_Data[otherIndex]);
          cluster.indices.push(otherIndex);
          processed.add(otherIndex);
        }
      });
      processed.add(index);
      clusters.push(cluster);
    });

    clusters.forEach(cluster => {
      const avgLat = cluster.indices.reduce((sum:number, idx:number) => sum + coords[idx][0], 0) / cluster.indices.length;
      const avgLng = cluster.indices.reduce((sum:number, idx:number) => sum + coords[idx][1], 0) / cluster.indices.length;
      const count = cluster.shops.length;

      let clusterHtml = '';
      if (count === 1) {
        clusterHtml = `<div style="position: relative;">
          <ion-icon name="storefront" style="font-size:32px; color: var(--ion-color-primary, #3880ff);"></ion-icon>
        </div>`;
      } else {
        clusterHtml = `<div style="
          background-color: var(--ion-color-primary, #3880ff);
          color: white;
          border-radius: 50%;
          width: 40px;
          height: 40px;
          display: flex;
          align-items: center;
          justify-content: center;
          font-weight: bold;
          border: 2px solid white;
          box-shadow: 0 2px 5px rgba(0,0,0,0.3);
        ">${count}</div>`;
      }

      const clusterIcon = L.divIcon({
        className: 'custom-cluster-icon',
        html: clusterHtml,
        iconSize: count === 1 ? [32, 32] : [40, 40],
        iconAnchor: count === 1 ? [16, 32] : [20, 20],
        popupAnchor: [0, count === 1 ? -32 : -20]
      });

      const marker = L.marker([avgLat, avgLng], { icon: clusterIcon }).addTo(this.clusterMarkersLayer!);
      // Show shop list in sidebar on cluster click
      marker.on('click', () => {
        this.ngZone.run(() => {
          if (count === 1) {
            // Single shop in cluster - show details
            if (!this.isWideLayout) {
              this.openShopDetails(cluster.shops[0]);
              if (avgLat && avgLng) {
                this.map?.setView([avgLat, avgLng], 15);
              }

            } else {
              this.showShopDetails(cluster.shops[0]);
              if (avgLat && avgLng) {
                this.map?.setView([avgLat, avgLng], 15);
              }
            }
          } else {
            // Multiple shops in cluster
            if (!this.isWideLayout) {
              // Mobile: zoom to fit all shops in cluster
              const bounds = L.latLngBounds(cluster.shops.map((shop: any)   => [
                Number(shop.latitude ?? shop.lat),
                Number(shop.longitude ?? shop.lng)
              ]));
              this.map?.fitBounds(bounds, { padding: [50, 50] });
              if (avgLat && avgLng) {
                this.map?.setView([avgLat, avgLng], 15);
              }
            } else {
              // Desktop: show list in sidebar
              this.showShopList([...cluster.shops]);
              if (avgLat && avgLng) {
                this.map?.setView([avgLat, avgLng], 15);
              }
            }
          }
        });
      });
    });
  }

  private placeIndividualMarkers() {
    if (!this.shopMarkersLayer) return;
    this.shopMarkerIndex.clear();
    this.Shop_Data.forEach((shop, index) => {
      const lat = Number(shop.latitude ?? shop.lat);
      const lng = Number(shop.longitude ?? shop.lng);
      if (Number.isNaN(lat) || Number.isNaN(lng)) return;

      const marker = L.marker([lat, lng], { icon: this.storeIcon }).addTo(this.shopMarkersLayer!);
      this.shopMarkerIndex.set(this.keyFor(lat, lng), marker);
      // Show shop details on click
      marker.on('click', () => {
        this.ngZone.run(() => {
          if (!this.isWideLayout) {
            this.openShopDetails(shop);
          } else {
            this.showShopDetails(shop);
          }
        });
      });
    });
  }
  // #endregion

  // #region Sidebar Management
  // Sidebar logic for showing shop list
  showShopList(shops: any[]) {
    console.log('Sidebar: showing shop list', shops);
    this.sidebar = { type: 'list', data: shops };
    this.cdr.detectChanges();
    this.appRef.tick();
  }

  // Sidebar logic for showing shop details
  showShopDetails(shop: any) {
    console.log('Sidebar: showing shop details', shop);
    this.sidebar = { type: 'details', data: shop };
    this.loadSidebarShopDetails(shop.id);
    this.cdr.detectChanges();
    this.appRef.tick();
  }

  loadSidebarShopDetails(shopId: number) {
    this.isSidebarLoading = true;
    this.sidebarErrorMessage = '';
    this.sidebarShopDetails = null;

    this.shopService.getShopDetails(shopId).subscribe({
      next: (response) => {
        console.log('Sidebar shop details response:', response);
        if (response && response.success && response.data) {
          this.sidebarShopDetails = response.data;
        } else {
          this.sidebarErrorMessage = 'Nem sikerült betölteni a bolt adatait.';
          console.error('Invalid shop details response:', response);
        }
        this.isSidebarLoading = false;
        this.cdr.detectChanges();
      },
      error: (error) => {
        console.error('Sidebar shop details error:', error);
        this.sidebarErrorMessage = 'Hiba történt az adatok betöltése során.';
        this.isSidebarLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  getStars(rating: number): boolean[] {
    return Array(5).fill(false).map((_, i) => i < rating);
  }

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
  // #endregion

  // #region Modal and Shop Details
  BackToSidebarList() {
    if (this.sidebar && this.sidebar.type === 'details') {
      this.showShopList((this.sidebar as any).previousList || this.Shop_Data);
    }
  }

  // Modal methods for mobile
  openShopDetails(shop: any) {
    console.log('Opening shop details modal:', shop);
    this.selectedShopId = shop.id;
    this.isModalOpen = true;
  }

  closeModal() {
    this.isModalOpen = false;
    this.selectedShopId = 0;
    // Force change detection
    this.cdr.detectChanges();
  }

  focusShop(shop: any) {
    // Center map on the shop
    const lat = Number(shop.latitude ?? shop.lat);
    const lng = Number(shop.longitude ?? shop.lng);

    if (this.map && lat && lng) {
      // Zoom to level 15 to show individual markers
      this.map.setView([lat, lng], 15, { animate: true });

      // Wait for animation, then ensure markers are updated and open popup
      setTimeout(() => {
        this.updateMarkers();
        const key = this.keyFor(lat, lng);
        const marker = this.shopMarkerIndex.get(key);
        if (marker) {
          marker.openPopup();
        }
      }, 300);
    }

    // Use modal on mobile, sidebar on desktop
    if (!this.isWideLayout) {
      this.openShopDetails(shop);
    } else {
      this.showShopDetails(shop);
    }
  }

  private openShopFromCluster(lat: number, lng: number) {
    if (!this.map) return;
    const shouldZoom = (this.map.getZoom() || 0) <= 13;
    if (shouldZoom) {
      this.pendingOpenShop = { lat, lng };
      this.map.setView([lat, lng], 15, { animate: true });
    } else {
      this.openShopPopupNow(lat, lng);
    }
  }

  private openShopPopupNow(lat: number, lng: number) {
    const key = this.keyFor(lat, lng);
    const marker = this.shopMarkerIndex.get(key);
    if (marker) {
      marker.openPopup();
      this.map?.panTo([lat, lng]);
    } else {
      // Ensure markers are up to date and try again quickly
      this.updateMarkers();
      const m2 = this.shopMarkerIndex.get(key);
      if (m2) {
        m2.openPopup();
        this.map?.panTo([lat, lng]);
      }
    }
  }
  // #endregion

  // #region Geolocation and User Location
  private async User_Marker_Place() {
    try {
      let lat = 0, lng = 0;
      let locationFailed = false;
      if (Capacitor.getPlatform() !== 'web') {
        try {
          await Geolocation.requestPermissions();
          const pos = await Geolocation.getCurrentPosition({ enableHighAccuracy: true, timeout: 10000 });
          lat = pos.coords.latitude; lng = pos.coords.longitude;
        } catch (e) {
          const msg = e instanceof Error ? e.message : String(e);
          const stack = e instanceof Error ? e.stack : undefined;
          this.errorLog.logError(`Geolocation plugin error: ${msg}`, stack, 'error');
          console.warn('Capacitor Geolocation failed, falling back to navigator', e);
        }
      }
      if ((lat === 0 && lng === 0) || Capacitor.getPlatform() === 'web') {
        await new Promise<void>((resolve) => {
          navigator.geolocation.getCurrentPosition((position) => {
            lat = position.coords.latitude;
            lng = position.coords.longitude;
            resolve();
          }, (err) => {
            const msg = err?.message || 'navigator.geolocation failed';
            if (msg.toLowerCase().includes('denied')) {
              console.warn(`Browser geolocation warning: User denied Geolocation.`);
            } else {
              this.errorLog.logError(`Browser geolocation error: ${msg}`);
            }
            locationFailed = true;
            resolve();
          });
        });
      }
      if (!lat && !lng) {
        if (locationFailed) {
          await this.showLocationAlert();
        }
        return;
      }
      this.userLat = lat; this.userLong = lng;
      if (this.userMarker) this.map?.removeLayer(this.userMarker);
      this.userMarker = L.marker([this.userLat, this.userLong], { icon: this.UserIcon }).addTo(this.map!);
      this.map?.setView([this.userLat, this.userLong], 13);
    } catch (err) {
      const e = err as any;
      const msg = e?.message || String(e);
      const stack = e?.stack;
      this.errorLog.logError(`Unexpected geolocation error: ${msg}`, stack, 'error');
      console.warn('Geolocation error', err);
    }
  }

  async showLocationAlert() {
    const alert = await this.alertController.create({
      header: 'Helymeghatározás',
      message: 'Nem sikerült megállapítani a pontos helyzetedet. Biztosan engedélyezted az appnak a helyadatok használatát?',
      buttons: ['Rendben']
    });
    await alert.present();
  }
  // #endregion

  // #region Google Maps Integration
  openInGoogleMaps(lat: number, lng: number, label?: string, city?: string, address?: string) {
    if (!lat || !lng) return;

    // Mindig a pontos koordinátákat használjuk a kereséshez, hogy a marker a megfelelő helyen legyen.
    const query = encodeURIComponent(`${lat},${lng}`);
    const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;

    const ua = navigator.userAgent || '';
    const isAndroid = /android/i.test(ua);
    const isIOS = /iPhone|iPad|iPod/i.test(ua);

    if (isAndroid) {
      // Androidon a geo: intent a legbiztosabb a Google Maps megnyitására
      const intentUrl = `geo:${lat},${lng}?q=${query}`;
      try {
        window.location.href = intentUrl;
        setTimeout(() => { window.location.href = webUrl; }, 1200);
      } catch { window.open(webUrl, '_blank'); }
      return;
    }
    if (isIOS) {
      // iOS-en a maps:// URL scheme a natív Apple/Google Maps-hoz
      const appleScheme = `maps://?q=${query}`;
      try { window.location.href = appleScheme; } catch { window.open(webUrl, '_blank'); }
      return;
    }

    // Weben simán megnyitjuk egy új lapon
    window.open(webUrl, '_blank');
  }
  // #endregion

  // #region Theme Management
  ToggleTheme() {
    // Persist desired theme; subscription above updates the tiles and body class
    this.theme.setTheme(!this.isThemeDark);
  }
  // #endregion

  // #region UI and Layout
  private setMapHeight = () => {
    const el = document.getElementById('map');
    if (!el) return;
    try { this.map?.invalidateSize(); } catch {}
  }

  private updateTileLayer(isDark: boolean) {
    if (!this.map || !this.tileLayer) return;
    try {
      // Remove old tile layer
      this.map.removeLayer(this.tileLayer);

      // Create new tile layer
      const url = isDark
        ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
        : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';

      this.tileLayer = L.tileLayer(url, {
        attribution: '&copy; OpenStreetMap & CARTO',
        subdomains: ['a', 'b', 'c', 'd'],
        crossOrigin: true as any
      });

      // Insert at the bottom so markers are on top
      this.tileLayer.addTo(this.map);
      if (this.shopMarkersLayer) {
        (this.shopMarkersLayer as any).bringToFront();
      }
      if (this.clusterMarkersLayer) {
        (this.clusterMarkersLayer as any).bringToFront();
      }

      this.map.invalidateSize(true);
    } catch (err) {
      console.error('Error updating tile layer:', err);
    }
  }

  private setMapHeightBound = this.setMapHeight.bind(this);
  // #endregion

  // #region Cleanup
  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.map) this.map.remove();
    window.removeEventListener('resize', this.setMapHeightBound);
    window.removeEventListener('orientationchange', this.setMapHeightBound);
  }
  // #endregion

  isReceivedCigarSubmittingDesktop = false;

  async onReceivedCigarClickDesktop() {
    const shopId = this.sidebarShopDetails?.id;
    if (!shopId || this.isReceivedCigarSubmittingDesktop) return;

    this.isReceivedCigarSubmittingDesktop = true;
    this.shopService.receivedCigar(shopId).subscribe({
      next: async () => {
        const toast = await this.toastCtrl.create({
          message: 'Rögzítve: kaptam szivart',
          duration: 2000,
          color: 'success',
          position: 'top'
        });
        await toast.present();
        this.isReceivedCigarSubmittingDesktop = false;
        this.cdr.detectChanges();
      },
      error: async (err) => {
        console.error('receivedCigar (desktop) error:', err);
        const toast = await this.toastCtrl.create({
          message: 'Nem sikerült rögzíteni',
          duration: 2500,
          color: 'danger',
          position: 'top'
        });
        await toast.present();
        this.isReceivedCigarSubmittingDesktop = false;
        this.cdr.detectChanges();
      }
    });
  }
}
