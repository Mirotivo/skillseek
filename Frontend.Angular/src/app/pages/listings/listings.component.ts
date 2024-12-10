import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { Listing } from '../../models/listing';
import { LessonCategory } from '../../models/lesson-category';
import { ListingService } from '../../services/listing.service';
import { CategoryService } from '../../services/category.service';
import { ModalComponent } from '../../components/modal/modal.component';
import { CreateListingComponent } from '../../components/create-listing/create-listing.component';

@Component({
  selector: 'app-listings',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent, ModalComponent, CreateListingComponent],
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent {
  listings: Listing[] = []; // Listings array can contain null
  selectedListing: Listing | null = null; // Selected listing can be null
  
  constructor(
    private categoryService: CategoryService,
    private listingService: ListingService,
  ) { }

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



  isModalOpen = false;

  openModal(): void {
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.loadListings();
    this.isModalOpen = false;
  }
}
