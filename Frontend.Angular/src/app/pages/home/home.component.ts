import { Component } from '@angular/core';
import { HeaderComponent } from '../../components/header/header.component';
import { CommonModule } from '@angular/common';
import { CategoryService } from '../../services/category.service';
import { LessonCategory } from '../../models/lesson-category';
import { Listing } from '../../models/listing';
import { ListingService } from '../../services/listing.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  imports: [CommonModule, FormsModule, HeaderComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  searchQuery: string = '';

  categories: LessonCategory[] = [];

  tutors: Listing[] = [];

  testimonials = [
    {
      name: 'Hiruni',
      subject: 'Maths',
      feedback:
        'Hiruni is great. She takes the time to understand where the problem areas are and is very patient and explains math concepts well.',
      reviewer: 'Barry',
    },
    {
      name: 'Jasmin',
      subject: 'German',
      feedback: 'Jasmin has helped me improve my language skills and is very engaging in her lessons.',
      reviewer: 'Richard',
    },
    // Add more testimonials as needed...
  ];

  constructor(
    private categoryService: CategoryService,
    private listingService: ListingService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchCategories();
    this.fetchTutors();
  }

  fetchCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: (err) => {
        console.error('Failed to fetch categories', err);
      },
    });
  }

  fetchTutors(): void {
    this.listingService.getRandomListings().subscribe({
      next: (data) => {
        this.tutors = data;
      },
      error: (err) => {
        console.error('Failed to fetch tutors', err);
      },
    });
  }

  performSearch(): void {
    if (!this.searchQuery.trim()) {
      return;
    }

    // Navigate to the SearchResultsComponent with the query string
    this.router.navigate(['/search-results'], { queryParams: { query: this.searchQuery } });
  }

  navigateToPayment(listingId: number): void {
    this.router.navigate(['/payment', listingId]);
  }

}
