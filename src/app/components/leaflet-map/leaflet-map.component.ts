import { Component, ElementRef, ViewChild, AfterViewInit, OnDestroy, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonicModule } from '@ionic/angular';
import * as L from 'leaflet';

// Fix default Leaflet marker asset paths (use app assets)
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'assets/leaflet/images/marker-icon-2x.png',
  iconUrl: 'assets/leaflet/images/marker-icon.png',
  shadowUrl: 'assets/leaflet/images/marker-shadow.png'
});

@Component({
  selector: 'app-leaflet-map',
  templateUrl: './leaflet-map.component.html',
  styleUrls: [
    './leaflet-map.component.scss',
    '../../../assets/leaflet/leaflet.css'
  ],
  standalone: true,
  imports: [CommonModule, IonicModule]
})
export class LeafletMapComponent implements AfterViewInit, OnDestroy, OnChanges {
  @ViewChild('mapContainer', { static: true }) mapContainer!: ElementRef<HTMLDivElement>;
  @Input() coords: [number, number] = [47.4979, 19.0402];
  @Input() shops: any[] = [];
  @Input() isThemeDark = true;
  @Input() bounds: [number, number][] | null = null;

  private map?: L.Map;
  private tileLayer?: L.TileLayer;
  private shopMarkersLayer?: L.LayerGroup;
  private clusterMarkersLayer?: L.LayerGroup;
  private userMarker?: L.Marker;

  // icons
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

  ngAfterViewInit() {
    this.initMap();
    this.placeUserMarker();
    this.updateMarkers();
    // sizing
    this.setMapHeight();
    window.addEventListener('resize', this.setMapHeightBound);
    window.addEventListener('orientationchange', this.setMapHeightBound);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.map) return;
    if (changes['isThemeDark']) {
      this.applyTileLayer();
    }
    if (changes['coords'] && this.coords) {
      try { this.map.setView(this.coords, this.map.getZoom() || 13); } catch {}
    }
    if (changes['shops']) {
      this.updateMarkers();
    }
  }

  private initMap() {
    if (this.map) return;
    const el = this.mapContainer?.nativeElement;
    if (!el) return;

    console.log('Map container element:', el);
    console.log('Container dimensions:', el.offsetWidth, 'x', el.offsetHeight);

    // If container has no dimensions, try to set them manually
    if (el.offsetHeight === 0 || el.offsetWidth === 0) {
      el.style.height = '500px';
      el.style.width = '100%';
      // Force reflow
      void el.offsetHeight;
      console.log('Forced dimensions, new:', el.offsetWidth, 'x', el.offsetHeight);
    }

    // Create map
    this.map = L.map(el, { 
      center: this.coords, 
      zoom: 13, 
      zoomControl: true,
      preferCanvas: true,
      trackResize: true
    });

    console.log('Map created:', this.map);
    console.log('Map container size:', this.map.getSize());

    // Add tile layer and other layers once map is ready
    const mapInstance = this.map;
    this.map.whenReady(() => {
      try {
        console.log('Map ready, adding tile layer');
        console.log('Map size after ready:', mapInstance.getSize());
        
        // Add tile layer
        const url = this.isThemeDark
          ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
          : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';
        console.log('Tile URL:', url);
        this.tileLayer = L.tileLayer(url, { 
          attribution: '&copy; OpenStreetMap & CARTO',
          maxZoom: 19,
          subdomains: ['a', 'b', 'c', 'd']
        }).addTo(mapInstance);
        console.log('Tile layer added:', this.tileLayer);

        // Now add marker layers
        this.shopMarkersLayer = L.layerGroup().addTo(mapInstance);
        this.clusterMarkersLayer = L.layerGroup().addTo(mapInstance);

        // If bounds provided, fit to bounds
        if (this.bounds && this.bounds.length === 2) {
          try { mapInstance.fitBounds(this.bounds as any); } catch {}
        }

        // Attach zoom listener
        mapInstance.on('zoomend', () => this.updateMarkers());

        // Invalidate size after a delay
        setTimeout(() => { 
          try { 
            mapInstance.invalidateSize(); 
            console.log('Size invalidated, container:', el.offsetWidth, 'x', el.offsetHeight);
            console.log('Map size after invalidate:', mapInstance.getSize());
          } catch {} 
        }, 200);
      } catch (e) {
        console.warn('Error during map.whenReady:', e);
      }
    });
  }

  private applyTileLayer() {
    if (!this.map) return;
    // getCenter/getZoom may throw if map has not been fully initialized in some Leaflet builds
    let center: L.LatLng | null = null;
    let zoom: number | null = null;
    try { center = this.map.getCenter(); zoom = this.map.getZoom(); } catch (e) { /* ignore */ }
    if (this.tileLayer) { this.tileLayer.remove(); this.tileLayer = undefined; }
    const url = this.isThemeDark
      ? 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png'
      : 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png';
    this.tileLayer = L.tileLayer(url, { attribution: '&copy; OpenStreetMap & CARTO' });
    this.tileLayer.addTo(this.map);
    try { if (center && zoom != null) this.map.setView(center, zoom); } catch (e) { /* ignore */ }
  }

  private updateMarkers() {
    if (!this.shopMarkersLayer || !this.clusterMarkersLayer || !this.map) return;
    this.shopMarkersLayer.clearLayers();
    this.clusterMarkersLayer.clearLayers();
    const currentZoom = this.map.getZoom() || 0;
    if (currentZoom <= 13) this.placeClusteredMarkers(); else this.placeIndividualMarkers();
  }

  private placeUserMarker(lat?: number, lng?: number) {
    if (!navigator.geolocation && (lat == null || lng == null)) return;
    if (lat != null && lng != null) { this._placeUserMarker(lat, lng); return; }
    navigator.geolocation.getCurrentPosition((position) => {
      this._placeUserMarker(position.coords.latitude, position.coords.longitude);
    }, (err) => console.warn('Geolocation error', err));
  }

  private _placeUserMarker(lat: number, lng: number) {
    if (!this.map) return;
    if (this.userMarker) { this.map.removeLayer(this.userMarker); this.userMarker = undefined; }
    this.userMarker = L.marker([lat, lng], { icon: this.UserIcon }).addTo(this.map);
  }

  private placeClusteredMarkers() {
    if (!this.clusterMarkersLayer) return;
    const clusters: any[] = [];
    const processed = new Set<number>();
    const coords = this.shops.map(s => [Number(s.latitude ?? s.lat ?? s.coords?.[0] ?? s.location?.lat), Number(s.longitude ?? s.lng ?? s.coords?.[1] ?? s.location?.lng)]);
    const currentZoom = this.map?.getZoom() || 0;
    let radiusKm = 3;
    if (currentZoom >= 13 && currentZoom < 15) radiusKm = 1;
    else if (currentZoom >= 11 && currentZoom < 13) radiusKm = 1.5;
    else if (currentZoom < 9) radiusKm = 6;

    const calculateDistance = (lat1:number, lon1:number, lat2:number, lon2:number) => {
      const R = 6371; const dLat = (lat2 - lat1) * Math.PI / 180; const dLon = (lon2 - lon1) * Math.PI / 180;
      const a = Math.sin(dLat/2) * Math.sin(dLat/2) + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon/2) * Math.sin(dLon/2);
      const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a)); return R * c;
    };

    coords.forEach((coord, index) => {
      if (processed.has(index) || !coord || Number.isNaN(coord[0]) || Number.isNaN(coord[1])) return;
      const cluster = { shops: [this.shops[index]], indices: [index] };
      coords.forEach((other, otherIndex) => {
        if (index === otherIndex || processed.has(otherIndex)) return;
        const distance = calculateDistance(coord[0], coord[1], other[0], other[1]);
        if (distance <= radiusKm) { cluster.shops.push(this.shops[otherIndex]); cluster.indices.push(otherIndex); processed.add(otherIndex); }
      });
      processed.add(index); clusters.push(cluster);
    });

    clusters.forEach(cluster => {
      const avgLat = cluster.indices.reduce((sum:number, idx:number) => sum + (coords[idx]?.[0]||0), 0) / cluster.indices.length;
      const avgLng = cluster.indices.reduce((sum:number, idx:number) => sum + (coords[idx]?.[1]||0), 0) / cluster.indices.length;
      const count = cluster.shops.length; let iconFilename = 'storefront.svg';
      if (count > 1 && count <= 5) iconFilename = 'Store_cluster_2.svg'; else if (count > 5 && count < 20) iconFilename = 'Store_cluster_3.svg'; else if (count >= 20) iconFilename = 'Store_cluster_4.svg';
      const iconUrl = `assets/${iconFilename}`;
      const clusterIcon = L.icon({ iconUrl, iconSize: [48,48], iconAnchor: [24,24], popupAnchor: [0,-20], className: 'cluster-svg-icon' });
      const marker = L.marker([avgLat, avgLng], { icon: clusterIcon }).addTo(this.clusterMarkersLayer!);
      const popupContent = `
        <div style="min-width: 250px; max-height: 400px; overflow-y: auto;">
          <h3 style="margin: 0 0 10px 0; color: #3880ff;">${cluster.shops.length} Shops in this area</h3>
          ${cluster.shops.map((shop:any) => `
            <div style="border-bottom: 1px solid #ccc; padding: 8px 0;">
              <strong>${shop.name || 'Shop'}</strong><br>
              <small>${shop.address || 'N/A'}, ${shop.city || 'N/A'}</small>
            </div>
          `).join('')}
        </div>
      `;
      marker.bindPopup(popupContent);
    });
  }

  private placeIndividualMarkers() {
    if (!this.shopMarkersLayer) return;
    const coords = this.shops.map(s => [s.latitude ?? s.lat ?? s.coords?.[0] ?? s.location?.lat, s.longitude ?? s.lng ?? s.coords?.[1] ?? s.location?.lng]);
    coords.forEach((coord:any, index:number) => {
      if (!coord || Number.isNaN(Number(coord[0])) || Number.isNaN(Number(coord[1]))) return;
      const marker = L.marker([Number(coord[0]), Number(coord[1])], { icon: this.storeIcon }).addTo(this.shopMarkersLayer!);
      const shopData = this.shops[index];
      const openingHoursHtml = this.formatOpeningHoursHTML(shopData);
      const popupContent = `
        <div style="min-width: 200px;">
          <h3 style="margin: 0 0 10px 0; color: #3880ff;">${shopData.name || 'Shop'}</h3>
          <p style="margin: 5px 0;"><strong>Address:</strong> ${shopData.address || 'N/A'}</p>
          <p style="margin: 5px 0;"><strong>City:</strong> ${shopData.city || 'N/A'}</p>
          <p style="margin: 5px 0;"><strong>Phone:</strong> ${shopData.phone || 'N/A'}</p>
          ${shopData.email ? `<p style="margin: 5px 0;"><strong>Email:</strong> ${shopData.email}</p>` : ''}
          ${shopData.website ? `<p style="margin: 5px 0;"><a href="${shopData.website}" target="_blank">Visit Website</a></p>` : ''}
          ${openingHoursHtml}
          <div style="margin-top:8px;">
            <button id="open-google-${index}" style="background:#3880ff;color:white;border:none;padding:6px 10px;border-radius:6px;cursor:pointer;">
              <ion-icon name="location-sharp" style="vertical-align: middle; font-size: 16px;"></ion-icon>
            </button>
          </div>
        </div>
      `;
      marker.bindPopup(popupContent);
      const lat = Number(coord[0]); const lng = Number(coord[1]);
      marker.on('popupopen', () => {
        const btn = document.getElementById(`open-google-${index}`);
        if (btn) { btn.onclick = null; btn.addEventListener('click', () => this.openInGoogleMaps(lat, lng, shopData.name)); }
      });
    });
  }

  ngOnDestroy() {
    if (this.map) { this.map.remove(); this.map = undefined; }
    window.removeEventListener('resize', this.setMapHeightBound);
    window.removeEventListener('orientationchange', this.setMapHeightBound);
  }

  private formatOpeningHoursHTML(shopData: any): string {
    const oh = shopData.openingHours ?? shopData.opening_hours ?? shopData.hours ?? shopData.opening;
    if (!oh) return '<p style="margin:5px 0;"><strong>Opening Hours:</strong> N/A</p>';
    let rows = '';
    const formatTime = (val: any) => { if (!val && val !== 0) return 'N/A'; const s = String(val); const parts = s.split(':'); if (parts.length >= 2) return parts[0].padStart(2,'0') + ':' + parts[1].padStart(2,'0'); return s; };
    if (Array.isArray(oh)) {
      oh.forEach((day: any) => { if (!day) return; if (typeof day === 'string') { rows += `<tr><td colspan="2">${day}</td></tr>`; } else { const dow = day.dayOfWeek; const open = day.openHour; const close = day.closeHour; rows += `<tr><td><strong>${dow || ''}</strong></td><td>${formatTime(open)} - ${formatTime(close)}</td></tr>`; } });
    } else if (typeof oh === 'object') {
      Object.entries(oh).forEach(([k, v]: [string, any]) => { if (!v && v !== 0) return; if (typeof v === 'string') { rows += `<tr><td><strong>${k}</strong></td><td>${v}</td></tr>`; } else if (typeof v === 'object') { const open = v.open ?? v.opens ?? v.openingTime ?? v.start ?? v.from ?? 'N/A'; const close = v.close ?? v.closes ?? v.closingTime ?? v.end ?? v.to ?? 'N/A'; rows += `<tr><td><strong>${k}</strong></td><td>${open} - ${close}</td></tr>`; } else { rows += `<tr><td><strong>${k}</strong></td><td>${String(v)}</td></tr>`; } });
    } else { rows = `<tr><td colspan="2">${String(oh)}</td></tr>`; }
    return `<table style="width:100%; margin-top:6px;">${rows}</table>`;
  }

  private openInGoogleMaps(lat: number, lng: number, label?: string) {
    const query = encodeURIComponent(label ? `${lat} ${lng}` : `${lat},${lng}`);
    const webUrl = `https://www.google.com/maps/search/?api=1&query=${query}`;
    const ua = navigator.userAgent || ''; const isAndroid = /android/i.test(ua); const isIOS = /iPhone|iPad|iPod/i.test(ua);
    if (isAndroid) { const intentUrl = `intent://maps.google.com/maps?daddr=${lat},${lng}#Intent;package=com.google.android.apps.maps;scheme=https;end`; try { window.location.href = intentUrl; setTimeout(() => { window.location.href = webUrl; }, 1200); } catch { window.open(webUrl, '_blank'); } return; }
    if (isIOS) { const appleScheme = `maps://?q=${lat},${lng}`; try { window.location.href = appleScheme; } catch { window.open(webUrl, '_blank'); } return; }
    window.open(webUrl, '_blank');
  }

  private setMapHeight = () => {
    const el = this.mapContainer?.nativeElement; if (!el) return; const tabBar = document.querySelector('.main-tab-bar'); const header = document.querySelector('.map-header'); const tabBarHeight = tabBar ? (tabBar as HTMLElement).offsetHeight : 0; const headerHeight = header ? (header as HTMLElement).offsetHeight : 0; const viewportHeight = window.innerHeight || document.documentElement.clientHeight; const heightPx = Math.max(0, viewportHeight - tabBarHeight - headerHeight); el.style.height = `${heightPx}px`; el.style.width = '100%'; try { this.map?.invalidateSize(); } catch {}
  }

  private setMapHeightBound = this.setMapHeight.bind(this);
}
