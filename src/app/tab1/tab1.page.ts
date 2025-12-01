import { Component, AfterViewInit, OnDestroy } from '@angular/core';
import { IonHeader, IonToolbar, IonTitle, IonToggle,
             IonContent, IonInput } from '@ionic/angular/standalone';
import { HttpClient } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import * as L from 'leaflet';
import { Capacitor } from '@capacitor/core';
import { Geolocation } from '@capacitor/geolocation';
import { addIcons } from 'ionicons';
import { radioButtonOn, person, locationSharp, storefront} from 'ionicons/icons';
import { ThemeService } from '../services/theme.service';

// Fix Leaflet marker paths
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'assets/leaflet/images/marker-icon-2x.png',
  iconUrl: 'assets/leaflet/images/marker-icon.png',
  shadowUrl: 'assets/leaflet/images/marker-shadow.png'
});


@Component({
  selector: 'app-tab1',
  standalone: true,
  templateUrl: 'tab1.page.html',
  styleUrls: ['tab1.page.scss'],
  imports: [IonHeader, IonToolbar, IonTitle, 
            IonContent, IonInput, IonToggle]})

export class Tab1Page implements AfterViewInit, OnDestroy {
  map?: L.Map;
  Varos_Coords: any[] = [];
  Shop_Data: any[] = [];
  isThemeDark = true;
  userLat = 0;
  userLong = 0;
  
  private tileLayer?: L.TileLayer;
  private shopMarkersLayer?: L.LayerGroup;
  private clusterMarkersLayer?: L.LayerGroup;
  private userMarker?: L.Marker;

  private storeIcon = L.divIcon({
    className: 'store-marker',
    html: '<ion-icon name="storefront" style="font-size:16px; color: #bbbbbb;"></ion-icon>',
    iconSize: [25, 25],
    iconAnchor: [16, 16],
    popupAnchor: [0, -16]
  });

  private UserIcon = L.divIcon({
    className: 'user-location-marker',
    html: '<ion-icon name="radio-button-on" style="font-size: 20px; color: #3880ff;"></ion-icon>',
    iconSize: [28, 28],
    iconAnchor: [14, 14],
    popupAnchor: [0, -16]
  });

  hungaryBounds: L.LatLngBoundsExpression = [[45.637, 16.113], [48.685, 22.897]];

  constructor(private http: HttpClient, private theme: ThemeService) {
    addIcons({ radioButtonOn, person, locationSharp, storefront });
  }

  ngOnInit() {
    // Keep local state in sync with global theme and update tiles if needed
    this.theme.theme$.subscribe(isDark => {
      const prev = this.isThemeDark;
      this.isThemeDark = isDark;
      if (this.map && this.tileLayer && prev !== isDark) {
        const url = this.isThemeDark
          ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
          : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';
        try {
          (this.tileLayer as any).setUrl(url);
          this.map.invalidateSize(true);
        } catch {}
      }
    });
  }

  ngAfterViewInit() {
    setTimeout(() => this.initMap(), 100);
  }

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

    this.map.on('zoomend', () => this.updateMarkers());

    this.User_Marker_Place();
    this.setMapHeight();
    window.addEventListener('resize', this.setMapHeightBound);
    window.addEventListener('orientationchange', this.setMapHeightBound);

    setTimeout(() => {
      this.map?.invalidateSize();
    }, 100);
  }

  varos_kereses(event: Event) {
    this.Varos_Coords = [];
    this.Shop_Data = [];
    const value = (event.target as HTMLIonInputElement | null)?.value ?? '';
    const cap_value = value ? value.toString().charAt(0).toUpperCase() + value.toString().slice(1).toLowerCase() : '';

    this.http.get(`http://192.168.137.1:3000/api/shops/cities/${cap_value}`).subscribe({
      next: (response) => {
        const shopRequests: any[] = [];
        Object.entries(response).forEach(([key, row]: [string, any]) => {
          if (Array.isArray(row)) {
            row.forEach((element: any) => {
              const shopId = (typeof element === 'object')
                ? (element.id ?? element.shopId ?? element._id ?? element)
                : element;
              if (!shopId) return;
              shopRequests.push(this.http.get(`http://192.168.137.1:3000/api/shops/${shopId}`));
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
            },
            error: (err) => console.error('Error loading shop data:', err)
          });
        }
      },
      error: (error) => console.error('Error:', error)
    });
  }

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
      const popupContent = `
        <div style="min-width: 250px; max-height: 400px; overflow-y: auto;">
          <h3 style="margin: 0 0 10px 0; color: #3880ff;">${count} Shops</h3>
          ${cluster.shops.map((shop:any) => `
            <div style="border-bottom: 1px solid #ccc; padding: 8px 0;">
              <strong>${shop.name || 'Shop'}</strong><br>
              <small>${shop.address || 'N/A'},</small>
            </div>
          `).join('')}
        </div>
      `;
      marker.bindPopup(popupContent);
    });
  }

  private placeIndividualMarkers() {
    if (!this.shopMarkersLayer) return;
    this.Shop_Data.forEach((shop, index) => {
      const lat = Number(shop.latitude ?? shop.lat);
      const lng = Number(shop.longitude ?? shop.lng);
      if (Number.isNaN(lat) || Number.isNaN(lng)) return;
      
      const marker = L.marker([lat, lng], { icon: this.storeIcon }).addTo(this.shopMarkersLayer!);
      const openingHoursHtml = this.formatOpeningHoursHTML(shop);
      const popupContent = `
        <div style="min-width: 200px;">
          <h3 style="margin: 0 0 10px 0; color: #3880ff;">${shop.name || 'Shop'}</h3>
          <p style="margin: 5px 0;"><strong>Address:</strong> ${shop.address || 'N/A'}</p>
          <p style="margin: 5px 0;"><strong>Phone:</strong> ${shop.phone || 'N/A'}</p>
          ${openingHoursHtml}
          <div style="margin-top:8px;">
            <button id="open-google-${index}" style="background:#3880ff;color:white;border:none;padding:6px 10px;border-radius:6px;cursor:pointer;">
              Open in Maps
            </button>
          </div>
        </div>
      `;
      marker.bindPopup(popupContent);
      marker.on('popupopen', () => {
        const btn = document.getElementById(`open-google-${index}`);
        if (btn) btn.addEventListener('click', () => this.openInGoogleMaps(lat, lng, shop.name));
      });
    });
  }

  private async User_Marker_Place() {
    try {
      let lat = 0, lng = 0;
      if (Capacitor.getPlatform() !== 'web') {
        try {
          await Geolocation.requestPermissions();
          const pos = await Geolocation.getCurrentPosition({ enableHighAccuracy: true, timeout: 10000 });
          lat = pos.coords.latitude; lng = pos.coords.longitude;
        } catch (e) {
          console.warn('Capacitor Geolocation failed, falling back to navigator', e);
        }
      }
      if ((lat === 0 && lng === 0) || Capacitor.getPlatform() === 'web') {
        await new Promise<void>((resolve) => {
          navigator.geolocation.getCurrentPosition((position) => {
            lat = position.coords.latitude;
            lng = position.coords.longitude;
            resolve();
          }, () => resolve());
        });
      }
      if (!lat && !lng) return;
      this.userLat = lat; this.userLong = lng;
      if (this.userMarker) this.map?.removeLayer(this.userMarker);
      this.userMarker = L.marker([this.userLat, this.userLong], { icon: this.UserIcon }).addTo(this.map!);
      this.map?.setView([this.userLat, this.userLong], 13);
    } catch (err) {
      console.warn('Geolocation error', err);
    }
  }

  formatOpeningHoursHTML(shopData: any): string {
    const oh = shopData.openingHours ?? shopData.opening_hours ?? shopData.hours ?? shopData.opening;
    if (!oh) return '<p style="margin:5px 0;"><strong>Opening Hours:</strong> N/A</p>';
    let rows = '';
    const formatTime = (val: any) => {
      if (!val && val !== 0) return 'N/A';
      const s = String(val);
      const parts = s.split(':');
      if (parts.length >= 2) return parts[0].padStart(2,'0') + ':' + parts[1].padStart(2,'0');
      return s;
    };
    if (Array.isArray(oh)) {
      oh.forEach((day: any) => {
        if (!day) return;
        if (typeof day === 'string') rows += `<tr><td colspan="2">${day}</td></tr>`;
        else {
          const dow = day.dayOfWeek;
          const open = day.openHour;
          const close = day.closeHour;
          rows += `<tr><td><strong>${dow || ''}</strong></td><td>${formatTime(open)} - ${formatTime(close)}</td></tr>`;
        }
      });
    } else if (typeof oh === 'object') {
      Object.entries(oh).forEach(([k, v]: [string, any]) => {
        if (!v && v !== 0) return;
        if (typeof v === 'string') rows += `<tr><td><strong>${k}</strong></td><td>${v}</td></tr>`;
        else if (typeof v === 'object') {
          const open = v.open ?? v.opens ?? v.openingTime ?? 'N/A';
          const close = v.close ?? v.closes ?? v.closingTime ?? 'N/A';
          rows += `<tr><td><strong>${k}</strong></td><td>${open} - ${close}</td></tr>`;
        }
      });
    } else rows = `<tr><td colspan="2">${String(oh)}</td></tr>`;
    return `<table style="width:100%; margin-top:6px;">${rows}</table>`;
  }

  openInGoogleMaps(lat: number, lng: number, label?: string) {
    const query = encodeURIComponent(label ? `${lat} ${lng}` : `${lat},${lng}`);
    const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
    const ua = navigator.userAgent || '';
    const isAndroid = /android/i.test(ua);
    const isIOS = /iPhone|iPad|iPod/i.test(ua);
    if (isAndroid) {
      const intentUrl = `intent://maps.google.com/maps?daddr=${lat},${lng}#Intent;package=com.google.android.apps.maps;scheme=https;end`;
      try {
        window.location.href = intentUrl;
        setTimeout(() => { window.location.href = webUrl; }, 1200);
      } catch { window.open(webUrl, '_blank'); }
      return;
    }
    if (isIOS) {
      const appleScheme = `maps://?q=${lat},${lng}`;
      try { window.location.href = appleScheme; } catch { window.open(webUrl, '_blank'); }
      return;
    }
    window.open(webUrl, '_blank');
  }

  ToggleTheme() {
    // Persist desired theme; subscription above updates the tiles and body class
    this.theme.setTheme(!this.isThemeDark);
  }

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

  private setMapHeightBound = this.setMapHeight.bind(this);

  ngOnDestroy() {
    if (this.map) this.map.remove();
    window.removeEventListener('resize', this.setMapHeightBound);
    window.removeEventListener('orientationchange', this.setMapHeightBound);
  }
}
