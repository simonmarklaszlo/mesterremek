// Angular core component decorator
import { Component } from '@angular/core';
// Ionic UI components used in the template
import { IonHeader, IonToolbar, IonTitle,IonSegment,IonSegmentButton,IonLabel,IonToggle,
             IonContent,IonItem,IonInput,IonMenu,IonButtons,IonMenuButton,IonButton } from '@ionic/angular/standalone';
// HTTP client for making API requests
import { HttpClient } from '@angular/common/http';
// Leaflet library for interactive maps
import  * as L from 'leaflet';
// Ionicons for using icons
import { addIcons } from 'ionicons';
import { radioButtonOn, person, locationSharp } from 'ionicons/icons';

// Fix Leaflet's default marker icon paths for Angular/Webpack builds
// This prevents broken marker icons by explicitly setting the icon URLs
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'assets/leaflet/images/marker-icon-2x.png',  // High-resolution marker icon
  iconUrl: 'assets/leaflet/images/marker-icon.png',           // Standard marker icon
  shadowUrl: 'assets/leaflet/images/marker-shadow.png'        // Marker shadow image
});


// Component decorator that defines metadata for this component
@Component({
  selector: 'app-tab1',                  // HTML tag to use this component: <app-tab1>
  standalone: true,                      // Makes this a standalone component (no NgModule needed)
  templateUrl: 'tab1.page.html',        // Path to the HTML template file
  styleUrls: ['tab1.page.scss'],        // Path to the SCSS stylesheet file
  imports: [IonHeader, IonToolbar, IonTitle,IonSegment,IonSegmentButton,IonLabel, 
            IonContent,IonItem,IonInput,IonMenu,IonButtons,IonMenuButton,IonToggle,IonButton]})  // Ionic components used in template

export class Tab1Page {
  // Leaflet map instance - holds reference to the interactive map
  map: L.Map | undefined;
  
  // Array to store coordinates of cities/stores from API responses
  Varos_Coords: any[] = [];
  
  // Boolean flag to track current theme state (true = dark, false = light)
  isThemeDark = true;
  
  // User's current latitude coordinate
  userLat = 0;
  
  // User's current longitude coordinate
  userLong = 0;

  // Custom Leaflet icon for store/city markers
  storeIcon = L.icon({
    iconUrl: 'assets/leaflet/images/store_icon.png',  // Path to store icon image
    iconSize: [25, 25],                                // Size of the icon in pixels [width, height]
    iconAnchor: [12, 25],                              // Point of icon that corresponds to marker location
    popupAnchor: [1, -34],                             // Point where popup opens relative to iconAnchor
    shadowUrl: 'assets/leaflet/images/marker-shadow.png',  // Path to shadow image
    shadowSize: [41, 41]                               // Size of shadow in pixels
  });

  // Custom Leaflet div icon for user location marker using ion-icon
  UserIcon = L.divIcon({
    className: 'user-location-marker',                 // CSS class for styling
    html: '<ion-icon name="radio-button-on" style="font-size: 16px; color: #3880ff;"></ion-icon>',  // ion-icon with inline styles
    iconSize: [32, 32],                                // Size of the icon
    iconAnchor: [16, 16],                              // Center point of the icon
    popupAnchor: [0, -16]                              // Point where popup opens
  });

  // Constructor - injects HttpClient for making API requests and registers icons
  constructor(private http: HttpClient) {
    // Register ionicons for use in the app
    addIcons({ radioButtonOn, person, locationSharp });
  }
  
  // Define geographical bounds for Hungary to restrict map panning
  hungaryBounds = L.latLngBounds(
    [45.637, 16.113],  // South-west corner coordinates [latitude, longitude]
    [48.685, 22.897]   // North-east corner coordinates [latitude, longitude]
  );





  // Angular lifecycle hook - runs when component initializes
  ngOnInit() {
    // Set initial theme to dark by adding 'dark' class to body
    document.body.classList.toggle('dark', this.isThemeDark);
    
    // Get and display user's current location on map
    this.User_Marker_Place();
    
    // Initialize empty array for city coordinates (local variable, not used)
    const Varos_Coords = [];
    
    // Initialize Leaflet map with configuration options
    this.map = L.map('map',{
      center: [ 47.50713217562947, 19.044920454284487 ],  // Initial center point [lat, lng] - Budapest
      maxZoom: 18,                                         // Maximum zoom level allowed
      minZoom: 8,                                          // Minimum zoom level allowed
      maxBounds: this.hungaryBounds,                       // Restrict panning to Hungary bounds
      maxBoundsViscosity: 1.0,                             // How strongly to enforce bounds (1.0 = hard boundary)
      zoom: 13                                             // Initial zoom level
    });
    
    // Fit the map view to show all of Hungary
    this.map.fitBounds(this.hungaryBounds);

    // Add dark theme tile layer to the map
    // Tile layers provide the map imagery (streets, terrain, etc.)
    L.tileLayer( 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
      attribution: '&copy; OpenStreetMap & CARTO'  // Copyright attribution text
    }).addTo(this.map);

    // Dispatch window resize event after 10ms to ensure map renders correctly
    // This fixes rendering issues that can occur when map initializes
    setTimeout(function () {
      window.dispatchEvent(new Event('resize'));
    }, 10);
  }







  // Method to search for cities and display them on the map
  // Called when user enters a city name in the search input
  varos_kereses(event: Event) {
      // Remove all existing markers from the map before adding new ones
      this.Delete_Markers();
      
      // Get the search value from the input field (safely handle null)
      const value = (event.target as HTMLIonInputElement | null)?.value ?? '';
      
      // Capitalize the first letter and lowercase the rest (e.g., "budapest" -> "Budapest")
      const cap_value = value ? value.toString().charAt(0).toUpperCase() + value.toString().slice(1).toLowerCase() : '';

      // Make HTTP GET request to API to search for cities matching the input
      this.http.get(`http://10.30.41.15:3000/api/shops/cities/${cap_value}`).subscribe({
        // Success callback - called when API responds successfully
        next: (response) => {
          console.log('API Response:', response);  // Log the response for debugging

          // Ensure we handle different possible response shapes and avoid implicit 'any'

          Object.entries(response).forEach(([key, row]: [string, any]) => {
            // Log the key and the entire row object
            console.log('Key:', key);
            console.log('Row:', row);
            
            // Check if row is an array and loop through it
            if (Array.isArray(row)) {
              row.forEach((element: any) => {
                this.http.get(`http://10.30.41.15:3000/api/shops/${element}`).subscribe({
                  next: (shopResponse) => {
                    console.log('Shop API Response:', shopResponse);
                    // process shopResponse as needed
                  },
                  error: (err) => console.error('Shop API error:', err)
                });
              });
            } else {
              // If row is an object, log its properties
              console.log('Row is an object with properties:', Object.keys(row));
            }
            // For each city found, make another API call to get shops in that city
          
          });

          // Iterate over each city in the response object
          Object.entries(response).forEach(([key, row]: [string, any]) => {
            // Parse latitude coordinate from string to number
            const Lat = parseFloat(row.coords.latitude);
            
            // Parse longitude coordinate from string to number
            const Lng = parseFloat(row.coords.longitude);

            // Add coordinates to array as [latitude, longitude] pair
            this.Varos_Coords.push([parseFloat(row.coords.latitude), parseFloat(row.coords.longitude)]);
          });
          
          // Log all collected coordinates
          console.log(this.Varos_Coords);
          
          // Place markers on the map for all cities found
          this.Varos_Marker_Place();
        },
        // Error callback - called if API request fails
        error: (error) => {
          console.error('Error:', error);  // Log error for debugging
        }
      });
  }






  // Method to place city/store markers on the map
  Varos_Marker_Place(){
    // First, update the user's location marker
    this.User_Marker_Place();
    
    // Loop through all city coordinates and create markers
    this.Varos_Coords.forEach(coord => {
      // Create a marker at the coordinate using the store icon, and add it to the map
      // coord[0] = latitude, coord[1] = longitude
      const marker = L.marker([coord[0], coord[1]], { icon: this.storeIcon }).addTo(this.map!);
      
      // Optional: Bind popup to marker (currently commented out)
      // Would show city name and additional info when marker is clicked
      //marker.bindPopup(coord[2] + '<br>' + lista.join('<br>'));
    });
  }

  // Method to remove all markers from the map and clear the coordinates array
  Delete_Markers(){
    // Iterate through all layers (markers, tiles, etc.) on the map
    this.map?.eachLayer((layer) => {
      // Check if the layer is a marker (not a tile layer or other type)
      if (layer instanceof L.Marker) {
        // Remove the marker from the map
        this.map?.removeLayer(layer);
      }
    });
    
    // Clear the coordinates array to prepare for new search results
    this.Varos_Coords = [];
  }


  // Method to get user's current location and place a marker on the map
  // Parameters: userLat, userLong (optional) - use provided coordinates or get from browser
  User_Marker_Place(userLat = 0, userLong = 0){
    // Use the browser's Geolocation API to get the user's current position
    // This will prompt the user for location permission if not already granted
    navigator.geolocation.getCurrentPosition(
      // === Success callback - executed when location is successfully obtained ===
      (position) => {
        // Check if custom coordinates were provided (non-zero values)
        if (userLat != 0 && userLong != 0 ){
          // Use the provided coordinates instead of browser location
          // This allows placing a user marker at a specific location
          this.map?.addLayer(L.marker([userLat, userLong], { icon: this.UserIcon }));
        } else {
          // No custom coordinates provided - use browser's geolocation
          // Extract latitude and longitude from the position object
          this.userLat = position.coords.latitude;
          this.userLong = position.coords.longitude;
          
          // Create a marker at the user's location using the custom UserIcon
          // The UserIcon is a blue SVG circle defined in the class properties
          this.map?.addLayer(L.marker([this.userLat, this.userLong], { icon: this.UserIcon }));
        }
        
        // Center the map view on the user's location and set zoom level to 13
        // This provides a good balance between context and detail
        this.map?.setView([this.userLat, this.userLong], 13);
        
      }, 
      // === Error callback - executed if location cannot be obtained ===
      (error) => {
        // Log the error to the console for debugging purposes
        // Common errors: user denied permission, location unavailable, timeout
        console.error('Error getting location:', error);
      }
    );
  }

  // Method to toggle between dark and light themes for both the map and Ionic app
  // Called when user clicks the theme toggle button
  ToggleTheme() {
    // Toggle the theme flag (true -> false or false -> true)
    this.isThemeDark = !this.isThemeDark;
    
    // Toggle the 'dark' class on the body element to switch Ionic app theme
    // This applies dark theme CSS variables defined in variables.scss
    document.body.classList.toggle('dark', this.isThemeDark);
    
    // Check if switching to dark theme
    if (this.isThemeDark) {
      // === Dark Theme Setup ===
      
      // Remove existing map instance if it exists
      if (this.map) this.map.remove();
      
      // Create new map instance with dark theme
      this.map = L.map('map',{
        center: [this.userLat, this.userLong],              // Center on user's location
        maxZoom: 18,                                        // Maximum zoom level
        minZoom: 8,                                         // Minimum zoom level
        maxBounds: this.hungaryBounds,                      // Restrict to Hungary
        maxBoundsViscosity: 1.0,                            // Hard boundary enforcement
        zoom: 13                                            // Initial zoom level
      });
      
      // Add dark tile layer (CartoDB dark theme)
      L.tileLayer( 'https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; OpenStreetMap & CARTO'
      }).addTo(this.map);
      
      // Place user location marker on new map
      this.User_Marker_Place(this.userLat, this.userLong);
      
    } else {
      // === Light Theme Setup ===
      
      // Remove existing map instance if it exists
      if (this.map) this.map.remove();
      
      // Create new map instance with light theme
      this.map = L.map('map',{
        center: [ this.userLat, this.userLong ],            // Center on user's location
        maxZoom: 18,                                        // Maximum zoom level
        minZoom: 8,                                         // Minimum zoom level
        maxBounds: this.hungaryBounds,                      // Restrict to Hungary
        maxBoundsViscosity: 1.0,                            // Hard boundary enforcement
        zoom: 13                                            // Initial zoom level
      });
      
      // Add light tile layer (CartoDB light theme)
      L.tileLayer( 'https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
        attribution: '&copy; Stadia Maps, &copy; OpenMapTiles, &copy; OpenStreetMap',
      }).addTo(this.map);
      
      // Place user location marker on new map
      this.User_Marker_Place(this.userLat, this.userLong);
    }
  }
}