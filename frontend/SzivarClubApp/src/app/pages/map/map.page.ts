// #region Imports and Setup
import { Component, AfterViewInit, OnDestroy, NgZone, ChangeDetectorRef, ApplicationRef, OnInit } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { IonInput, IonButton, IonIcon, IonHeader, IonToolbar, IonTitle, 
         IonLabel, IonChip, IonText, IonSpinner } from '@ionic/angular/standalone';
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
         timeOutline, starOutline, star, checkmarkCircle, closeCircle, locationOutline, mapOutline } from 'ionicons/icons';
import { ThemeService } from '../../services/theme.service';
import { ErrorLogService } from '../../services/error-log.service';
import { SearchService } from '../../services/search.service';
import { ShopService, ShopDetails } from '../../services/shop.service';
import { ShopDetailsModalComponent } from '../../components/shop-details-modal/shop-details-modal.component';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

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
    IonLabel, IonChip, IonText, IonSpinner, FormsModule,
    ShopDetailsModalComponent
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
  private currentTheme = true;

  // Modal state for mobile
  isModalOpen: boolean = false;
  selectedShopId: number = 0;
  
  // Window width for responsive design
  windowWidth: number = window.innerWidth;
  windowHeight: number = window.innerHeight;
  isWideLayout: boolean = (window.innerWidth / window.innerHeight) > 1;
  
  // Mobile search visibility
  showMobileSearch: boolean = false;

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
    private shopService: ShopService
  ) {
    addIcons({ radioButtonOn, locationSharp, storefront, search, locate, arrowBack,
               timeOutline, starOutline, star, checkmarkCircle, closeCircle, locationOutline, mapOutline });
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

    // Subscribe to search service changes for text sync
    this.searchService.searchState$
      .pipe(takeUntil(this.destroy$))
      .subscribe((state) => {
        this.searchText = state.searchText;
      });

    // Subscribe to search triggers from list page
    this.searchService.searchTriggered$
      .pipe(takeUntil(this.destroy$))
      .subscribe((trigger) => {
        // Only respond if the search was triggered from the list page
        if (trigger.source === 'list') {
          this.searchText = trigger.searchText;
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

  varos_kereses(event?: Event) {
    // Manual search - trigger other page
    this.executeVarosKereses(true, event);
  }

  toggleMobileSearch() {
    if (!this.showMobileSearch) {
      // Show search bar and focus on input
      this.showMobileSearch = true;
      setTimeout(() => {
        const input = document.getElementById('varos') as HTMLInputElement;
        if (input) {
          input.focus();
        }
      }, 100);
    } else {
      // Perform search
      this.varos_kereses();
    }
  }

  private executeVarosKereses(triggerOtherPage: boolean = false, event?: Event) {
    this.Varos_Coords = [];
    this.Shop_Data = [];
    this.sidebar = null;
    let raw = this.searchText ?? '';
    if (!raw) {
      const el = document.getElementById('varos') as HTMLInputElement | null;
      raw = el?.value ?? '';
    }
    if (!raw && event) {
      const tgt = event.target as any;
      raw = tgt?.value ?? '';
    }
    const cap_value = raw ? raw.toString().charAt(0).toUpperCase() + raw.toString().slice(1).toLowerCase() : '';

    this.http.get(`http://localhost:3000/api/shops/cities/${cap_value}`).subscribe({
      next: (response) => {
        const shopRequests: any[] = [];
        Object.entries(response).forEach(([key, row]: [string, any]) => {
          if (Array.isArray(row)) {
            row.forEach((element: any) => {
              const shopId = (typeof element === 'object')
                ? (element.id ?? element.shopId ?? element._id ?? element)
                : element;
              if (!shopId) return;
              shopRequests.push(this.http.get(`http://localhost:3000/api/shops/${shopId}`));
            });
          }
        });

        if (shopRequests.length > 0) {
          forkJoin(shopRequests).subscribe({
            next: (allShopResponses) => {
              allShopResponses.forEach((shopResponse: any) => {
                const shops = Array.isArray(shopResponse) ? shopResponse : Object.values(shopResponse);
                shops.forEach((row2: any) => {
                  const latNum = parseFloat(row2.latitude ?? row2.lat ?? row2.coords?.latitude ?? NaN);
                  const lngNum = parseFloat(row2.longitude ?? row2.lng ?? row2.coords?.longitude ?? NaN);
                  if (!isNaN(latNum) && !isNaN(lngNum)) {
                    this.Varos_Coords.push([latNum, lngNum]);
                    this.Shop_Data.push(row2);
                  }
                });
              });
              this.updateMarkers();
              // Center map on first result
              if (this.Varos_Coords.length > 0 && this.map) {
                this.map.setView(this.Varos_Coords[0] as [number, number], 13);
              }
              // Show all shops in sidebar as list by default
              if (this.Shop_Data.length > 0) {
                this.showShopList(this.Shop_Data);
              }

              // Only trigger other page after search completes
              if (triggerOtherPage) {
                this.searchService.triggerSearch('map', {
                  searchText: this.searchText
                });
              }
            },
            error: (err) => console.error('Error loading shop data:', err)
          });
        }
      },
      error: (error) => console.error('Error:', error)
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
      let iconFilename = 'storefront.svg';
      if (count > 1 && count <= 5) iconFilename = 'Store_cluster_2.svg';
      else if (count > 5 && count < 20) iconFilename = 'Store_cluster_3.svg';
      else if (count >= 20) iconFilename = 'Store_cluster_4.svg';

      const clusterIcon = L.icon({
        iconUrl: `assets/${iconFilename}`,
        iconSize: [48,48],
        iconAnchor: [24,24],
        popupAnchor: [0,-20]
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
            this.errorLog.logError(`Browser geolocation error: ${msg}`);
            resolve();
          });
        });
      }
      if (!lat && !lng) return;
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
  // #endregion

  // #region Google Maps Integration
  openInGoogleMaps(lat: number, lng: number, label?: string, city?: string, address?: string) {
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
    const tabBar = document.querySelector('.main-tab-bar');
    const header = document.querySelector('.map-header');
    const tabBarHeight = tabBar ? (tabBar as HTMLElement).offsetHeight : 0;
    const headerHeight = header ? (header as HTMLElement).offsetHeight : 0;
    const viewportHeight = window.innerHeight || document.documentElement.clientHeight;
    const heightPx = Math.max(400, viewportHeight - tabBarHeight - headerHeight);
    el.style.height = `${heightPx}px`;
    el.style.width = '100%';
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
}
