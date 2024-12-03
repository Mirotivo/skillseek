import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { Listing } from '../../models/listing';
import { LessonCategory } from '../../models/lesson-category';
import { ListingService } from '../../services/listing.service';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-listings',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent],
  templateUrl: './listings.component.html',
  styleUrl: './listings.component.scss'
})
export class ListingsComponent {
  locationOptions: string[] = ['Webcam', 'TutorLocation', 'StudentLocation'];
  socialPlatformOptions: string[] = ['Facebook', 'Instagram', 'Twitter', 'LinkedIn', 'Email']; // Predefined platforms

  listings: Listing[] = []; // Listings array can contain null
  selectedListing: Listing | null = null; // Selected listing can be null
  lessonCategories: LessonCategory[] = [];

  showCreateListing: boolean = false;

  newListing: Partial<Listing> = {
    title: '',
    image: '',
    lessonsTaught: '',
    locations: [],
    aboutLesson: '',
    aboutYou: '',
    rates: {
      hourly: '',
      fiveHours: '',
      tenHours: ''
    },
    socialPlatforms: []
  };

  constructor(
    private categoryService: CategoryService,
    private listingService: ListingService,
  ) { }

  ngOnInit(): void {
    this.loadListings();
    this.loadLessonCategories();
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
  loadLessonCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.lessonCategories = data;
      },
      error: (err) => {
        console.error('Failed to fetch lesson categories', err);
      }
    });
  }

  selectListing(listing: Listing) {
    this.selectedListing = listing;
  }


  openCreateListing(): void {
    this.showCreateListing = true;
  }
  closeCreateListing(): void {
    this.showCreateListing = false;
    this.newListing = {
      title: '',
      image: '',
      lessonsTaught: '',
      locations: [],
      aboutLesson: '',
      aboutYou: '',
      rates: {
        hourly: '',
        fiveHours: '',
        tenHours: ''
      },
      socialPlatforms: []
    };
  }

  submitCreateListing(): void {
    const processedListing: Listing = {
      ...this.newListing,
      lessonCategoryId: Number(this.newListing.lessonCategoryId),
      locations: Array.isArray(this.newListing.locations)
        ? this.newListing.locations
        : (this.newListing.locations || '').split(',').map((loc) => loc.trim()),
      socialPlatforms: Array.isArray(this.newListing.socialPlatforms)
        ? this.newListing.socialPlatforms
        : (this.newListing.socialPlatforms || '').split(',').map((platform) => platform.trim())
    } as Listing;

    this.listingService.createListing(processedListing).subscribe({
      next: (newListing) => {
        this.listings.push(newListing);
        this.closeCreateListing();
      },
      error: (err) => {
        console.error('Failed to create listing:', err);
      }
    });
  }

}
