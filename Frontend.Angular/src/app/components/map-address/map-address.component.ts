/// <reference types="google.maps" />
import { Component, AfterViewInit, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GoogleMapsService } from '../../services/google-maps.service';

@Component({
  selector: 'app-map-address',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './map-address.component.html',
  styleUrls: ['./map-address.component.scss']
})
export class MapAddressComponent implements AfterViewInit {
  @Input() initialAddress: string | null = null;
  @Output() addressSelected = new EventEmitter<string>();
  selectedAddress: string | null = null;

  constructor(private googleMapsService: GoogleMapsService) { }

  ngAfterViewInit(): void {
    this.googleMapsService.loadGoogleMaps()
    .then(() => this.initMap())
    .catch((error) => console.error(error));

    // this.initMap();
  }

  initMap(): void {
    const map = new google.maps.Map(document.getElementById('map') as HTMLElement, {
      center: { lat: -33.8688, lng: 151.2093 }, // Default to Sydney
      zoom: 13,
    });
    const input = document.getElementById('search-box') as HTMLInputElement;
    const searchBox = new google.maps.places.SearchBox(input);
    // If initialAddress is set, geocode and update the map
    if (this.initialAddress) {
      this.geocodeAddress(this.initialAddress, map);
    }

    searchBox.addListener('places_changed', () => {
      const places = searchBox.getPlaces();

      if (!places || places.length === 0) {
        console.log('No places found');
        return;
      }

      const markers: google.maps.Marker[] = [];
      const bounds = new google.maps.LatLngBounds();

      places.forEach((place: google.maps.places.PlaceResult) => {
        if (!place.geometry || !place.geometry.location) {
          console.log('Returned place contains no geometry');
          return;
        }

        markers.push(
          new google.maps.Marker({
            map,
            title: place.name,
            position: place.geometry.location,
          })
        );

        this.selectedAddress = place.formatted_address || place.name || 'Unknown Address';
        this.addressSelected.emit(this.selectedAddress);

        if (place.geometry.viewport) {
          bounds.union(place.geometry.viewport);
        } else {
          bounds.extend(place.geometry.location);
        }
      });

      map.fitBounds(bounds);
    });
  }

  geocodeAddress(address: string, map: google.maps.Map): void {
    const geocoder = new google.maps.Geocoder();
  
    geocoder.geocode({ address }, (results, status) => {
      if (status === 'OK' && results && results[0]?.geometry) {
        const location = results[0].geometry.location;
        map.setCenter(location);
        map.setZoom(13);
  
        new google.maps.Marker({
          map,
          position: location,
        });
  
        this.selectedAddress = results[0].formatted_address || address;
        this.addressSelected.emit(this.selectedAddress);
      } else if (!results) {
        console.error('No results found for the provided address.');
      } else {
        console.error(`Geocode was not successful for the following reason: ${status}`);
      }
    });
  }
}
