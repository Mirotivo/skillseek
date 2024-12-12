import { Component, EventEmitter, Output } from '@angular/core';
import { Listing } from '../../models/listing';
import { ListingService } from '../../services/listing.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LessonCategory } from '../../models/lesson-category';
import { CategoryService } from '../../services/category.service';

@Component({
  selector: 'app-create-listing',
  imports: [CommonModule, FormsModule],
  templateUrl: './create-listing.component.html',
  styleUrl: './create-listing.component.scss'
})
export class CreateListingComponent {
  @Output() onClose = new EventEmitter<void>();
  locationOptions: string[] = ['Webcam', 'TutorLocation', 'StudentLocation'];
  socialPlatformOptions: string[] = ['Facebook', 'Instagram', 'Twitter', 'LinkedIn', 'Email']; // Predefined platforms
  lessonCategories: LessonCategory[] = [];
  selectedImageFile: File | null = null;

  newListing: Partial<Listing> = {
    title: '',
    image: '',
    lessonsTaught: '',
    locations: [],
    aboutLesson: '',
    aboutYou: '',
    rates: {
      hourly: 0,
      fiveHours: 0,
      tenHours: 0
    },
    socialPlatforms: []
  };

  get rates(): { hourly: number; fiveHours: number; tenHours: number } {
    if (!this.newListing.rates) {
      this.newListing.rates = { hourly: 0, fiveHours: 0, tenHours: 0 };
    }
    return this.newListing.rates;
  }

  constructor(
    private categoryService: CategoryService,
    private listingService: ListingService
  ) { }

  ngOnInit(): void {
    this.loadLessonCategories();
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

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedImageFile = input.files[0];
    }
  }

  submitCreateListing(): void {
    const processedListing: Listing = {
      ...this.newListing,
      imageFile: this.selectedImageFile,
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
        this.closeModal();
      },
      error: (err) => {
        console.error('Failed to create listing:', err);
      }
    });
  }
  
  closeModal(): void {
    this.onClose.emit();
  }
}
