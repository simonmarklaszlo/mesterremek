import { Component } from '@angular/core';
import { IonHeader, IonToolbar, IonTitle,IonSegment,IonSegmentButton,IonLabel,
             IonContent,IonItem,IonInput,IonMenu,IonButtons,IonMenuButton, } from '@ionic/angular/standalone';
import { HttpClient } from '@angular/common/http';
import  * as L from 'leaflet';

// Fix Leaflet marker icon paths
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
  imports: [IonHeader, IonToolbar, IonTitle,IonSegment,IonSegmentButton,IonLabel, 
            IonContent,IonItem,IonInput,IonMenu,IonButtons,IonMenuButton]})


export class Tab1Page {
  map: L.Map | undefined;
  Varos_Coords: any[] = [];

  storeIcon = L.icon({
    iconUrl: 'assets/leaflet/images/store_icon.png',
    iconSize: [25, 25],
    iconAnchor: [12, 25],
    popupAnchor: [1, -34],
    shadowUrl: 'assets/leaflet/images/marker-shadow.png',
    shadowSize: [41, 41]
  });

  constructor(private http: HttpClient) {}
  hungaryBounds = L.latLngBounds(
    [45.637, 16.113], // south-west
    [48.685, 22.897]  // north-east
  );

  ngOnInit() {
    this.User_Marker_Place();
    const Varos_Coords = [];
    this.map = L.map('map',{
      center: [ 47.50713217562947, 19.044920454284487 ],
        maxZoom: 18,
      minZoom: 8,
      maxBounds: this.hungaryBounds, maxBoundsViscosity: 1.0,
      zoom: 13
    });
    this.map.fitBounds(this.hungaryBounds);

    // Set #map div height to 95%

    L.tileLayer( 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
      attribution: '&copy; OpenStreetMap & CARTO'}).addTo(this.map);

    setTimeout(function () {
      window.dispatchEvent(new Event('resize'));
    }, 10);
  }

  varos_kereses(event: Event) {
      this.Delete_Markers();
      const value = (event.target as HTMLIonInputElement | null)?.value ?? '';
      const cap_value = value ? value.toString().charAt(0).toUpperCase() + value.toString().slice(1).toLowerCase() : '';

      this.http.get(`http://10.31.16.3:3000/api/cities/${cap_value}`).subscribe({
        next: (response) => {
          console.log('API Response:', response);
          Object.entries(response).forEach(([key, row]: [string, any]) => {
            const Lat = parseFloat(row.coords.latitude);
           // console.log(Lat);
            const Lng = parseFloat(row.coords.longitude);

            this.Varos_Coords.push([parseFloat(row.coords.latitude), parseFloat(row.coords.longitude)]);

          });
          
          // Move these inside the subscribe - they need to run AFTER the data arrives
          console.log(this.Varos_Coords);
          this.Varos_Marker_Place();
        },
        error: (error) => {
          console.error('Error:', error);
        }
      });

  }


  Varos_Marker_Place(){
    this.User_Marker_Place();
    this.Varos_Coords.forEach(coord => {
      //var lista = [];
      //lista = coord[3].split('|')
      const marker = L.marker([coord[0], coord[1]], { icon: this.storeIcon }).addTo(this.map!);
      //marker.bindPopup(coord[2] + '<br>' + lista.join('<br>'));
    });
  }

  Delete_Markers(){
    this.map?.eachLayer((layer) => {
      if (layer instanceof L.Marker) {
        this.map?.removeLayer(layer);
      }
    });
    this.Varos_Coords = [];
  }

  User_Marker_Place(){
    navigator.geolocation.getCurrentPosition((position) => {
      const userLat = position.coords.latitude;
      const userLng = position.coords.longitude;
      const userMarker = L.marker([userLat, userLng]).addTo(this.map!);
      this.map?.setView([userLat, userLng], 13);
    }, (error) => {
      console.error('Error getting location:', error);
    });
  }
}