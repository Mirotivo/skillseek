import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingService } from '../../services/listing.service';
import { Listing } from '../../models/listing';
import { PaypalService } from '../../services/paypal.service';
import { PaymentService } from '../../services/payment.service';

@Component({
  selector: 'app-payment',
  imports: [CommonModule, FormsModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit {
  tutor: Listing | null = null; // Store tutor details
  tutorId: string | null = null; // Store the tutor Id from the route
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
    private listingService: ListingService,
    private paypalService: PaypalService,
    private paymentService: PaymentService
  ) {}

  ngOnInit(): void {
    // Get the tutor Id from the route parameters
    this.route.paramMap.subscribe((params) => {
      this.tutorId = params.get('id');
      if (this.tutorId) {
        this.fetchTutorDetails(this.tutorId);
      } else {
        console.error('Tutor Id is missing');
        this.loading = false;
      }
    });
    this.paypalService.loadPayPalScript().then(() => {
      this.renderPayPalButton();
    });
  }

  fetchTutorDetails(id: string): void {
    this.listingService.getRandomListings().subscribe({
      next: (listings) => {
        this.tutor = listings.find((listing) => listing.id.toString() === id) || null;
        if (!this.tutor) {
          console.error('Tutor details not found for Id:', id);
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

  renderPayPalButton(): void {
    const paypal = (window as any).paypal;
  
    if (paypal && paypal.Buttons) {
      paypal.Buttons({
        createOrder: (data: any, actions: any) => {
          return this.paymentService.createPayment(69.00, 'USD').toPromise().then((order) => {
            if (!order || !order.orderID) {
              throw new Error('Order Id not returned from the server.');
            }
            return order.orderID;
          }).catch((error) => {
            console.error('Error creating order:', error);
            throw error;
          });
        },
        onApprove: (data: any, actions: any) => {
          const orderID = data.orderID;
          if (!orderID) {
            console.error('Order Id is missing in onApprove.');
            return;
          }
  
          this.paymentService.capturePayment(orderID).toPromise().then(() => {
            console.log('Payment successfully captured.');
            this.onPay();
          }).catch((error) => {
            console.error('Error capturing payment:', error);
          });
        },
        onError: (err: any) => {
          console.error('PayPal Button Error:', err);
        },
      }).render('#paypal-button-container');
    } else {
      console.error('PayPal SDK not loaded.');
    }
  }
      

  onPay(): void {
    if (this.tutorId) {
      // Navigate to the booking route with the tutor Id
      this.router.navigate(['/booking', this.tutorId]);
      console.log(`Payment initiated via ${this.paymentMethod}`);
    } else {
      console.error('Tutor Id is missing');
    }
  }
}
