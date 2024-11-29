import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingService } from '../../services/listing.service';
import { Listing } from '../../models/listing';

@Component({
  selector: 'app-payment',
  imports: [CommonModule, FormsModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit {
  tutor: Listing | null = null; // Store tutor details
  tutorId: string | null = null; // Store the tutor ID from the route
  loading = true; // Loading state for fetching tutor details

  subscription = {
    title: 'Student Pass',
    subtitle: 'Non-Binding Monthly Subscription',
    price: 69,
    benefits: [
      'Your card is debited only if the tutor accepts your request.',
      'Contact unlimited tutors in all subjects with this pass.'
    ]
  };

  paymentMethod: 'card' | 'paypal' = 'paypal';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private listingService: ListingService
  ) {}

  ngOnInit(): void {
    // Get the tutor ID from the route parameters
    this.route.paramMap.subscribe((params) => {
      this.tutorId = params.get('id');
      if (this.tutorId) {
        this.fetchTutorDetails(this.tutorId);
      } else {
        console.error('Tutor ID is missing');
        this.loading = false;
      }
    });
  }

  fetchTutorDetails(id: string): void {
    this.listingService.getRandomListings().subscribe({
      next: (listings) => {
        this.tutor = listings.find((listing) => listing.id.toString() === id) || null;
        if (!this.tutor) {
          console.error('Tutor details not found for ID:', id);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch tutor details:', err);
        this.loading = false;
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  onPaymentMethodChange(method: 'card' | 'paypal'): void {
    this.paymentMethod = method;
  }

  onPay(): void {
    if (this.tutorId) {
      // Navigate to the booking route with the tutor ID
      this.router.navigate(['/booking', this.tutorId]);
      console.log(`Payment initiated via ${this.paymentMethod}`);
    } else {
      console.error('Tutor ID is missing');
    }
  }
}
