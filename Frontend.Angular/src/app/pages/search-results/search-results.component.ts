import { Component, OnInit } from '@angular/core';
import { Listing } from '../../models/listing';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingService } from '../../services/listing.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-search-results',
  imports: [CommonModule, FormsModule],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.scss'
})
export class SearchResultsComponent implements OnInit {
  searchQuery: string = '';
  searchResults: Listing[] = [];
  isLoading: boolean = true;
  hasError: boolean = false;
  currentPage: number = 1;
  totalResults: number = 0;
  pageSize: number = 10;
  isLoadingMore: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private listingService: ListingService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.searchQuery = params['query'] || '';
      if (this.searchQuery) {
        this.performSearch();
      }
    });
  }

  performSearch(loadMore: boolean = false): void {
    if (!loadMore) {
      this.isLoading = true;
      this.searchResults = []; // Reset results for a fresh search
      this.currentPage = 1;
    } else {
      this.isLoadingMore = true;
    }

    this.listingService.searchListings(this.searchQuery, this.currentPage, this.pageSize).subscribe({
      next: (response) => {
        this.totalResults = response.totalResults;
        this.searchResults = loadMore
          ? [...this.searchResults, ...response.results]
          : response.results;
        this.isLoading = false;
        this.isLoadingMore = false;
      },
      error: (err) => {
        console.error('Search failed:', err);
        this.hasError = true;
        this.isLoading = false;
        this.isLoadingMore = false;
      },
    });
  }

  loadMoreResults(): void {
    this.currentPage++;
    this.performSearch(true);
  }

  
  navigateToPayment(listingId: number): void {
    this.router.navigate(['/payment', listingId]);
  }
}
