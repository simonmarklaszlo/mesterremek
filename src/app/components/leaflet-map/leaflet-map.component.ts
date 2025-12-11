
import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import * as L from 'leaflet';

// Fix leaflet's default icon paths (for markers)
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'assets/leaflet/marker-icon-2x.png',
  iconUrl: 'assets/leaflet/marker-icon.png',
  shadowUrl: 'assets/leaflet/marker-shadow.png'
});

@Component({
  selector: 'app-leaflet-map',
  standalone: true,
  imports: [CommonModule],
  template: `<div id="map"></div>`,
  styleUrls: ['./leaflet-map.component.scss']
})
export class LeafletMapComponent implements OnInit, AfterViewInit {

  constructor() { }

  ngOnInit() {
    // keep for now
  }

  ngAfterViewInit() {
    // Set map div height to 95% dynamically
    const mapDiv = document.getElementById('map');
    if (mapDiv) {
      mapDiv.style.height = '100%';
      
    }
    
    this.initMap();
  }

  private initMap(): void {
    const hungaryBounds = L.latLngBounds(
      [46.637, 16.113], // south-west
      [48.685, 22.897]  // north-east
    );
    const map = L.map('map', { maxBounds: hungaryBounds, maxBoundsViscosity: 1.0 }).setView([47.50713217562947, 19.044920454284487], 13);
    L.tileLayer('https://tiles.stadiamaps.com/tiles/alidade_smooth/{z}/{x}/{y}{r}.png', {
      attribution: '&copy; Stadia Maps, &copy; OpenMapTiles, &copy; OpenStreetMap',
      maxZoom: 18,
      minZoom: 8,

       subdomains: ['mt0', 'mt1', 'mt2', 'mt3']
    }).addTo(map);


  }
}