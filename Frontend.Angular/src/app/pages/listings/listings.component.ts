import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { Listing } from '../../models/listing';
import { ListingService } from '../../services/listing.service';

@Component({
  selector: 'app-listings',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent],
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent {
  listings: Listing[] = [];

  selectedListing: Listing = this.listings[0];

  constructor(private listingService: ListingService) {}

  ngOnInit(): void {
    this.loadListings();
  }

  loadListings(): void {
    this.listingService.getListings().subscribe({
      next: (data) => {
        this.listings = data;
        if (this.listings.length > 0) {
          this.selectedListing = this.listings[0];
        }
      },
      error: (err) => {
        console.error('Failed to fetch listings:', err);
      },
    });
  }

  selectListing(listing: Listing) {
    this.selectedListing = listing;
  }
}
