import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ListingService } from '../../services/listing.service';
import { Listing } from '../../models/listing';
import { PaypalService } from '../../services/paypal.service';
import { PaymentService } from '../../services/payment.service';
import { loadStripe } from '@stripe/stripe-js';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-payment',
  imports: [CommonModule, FormsModule],
  templateUrl: './payment.component.html',
  styleUrls: ['./payment.component.scss']
})
export class PaymentComponent implements OnInit {
  listing: Listing | null = null; // Store tutor details
  listingId!: number;
  loading = true; // Loading state for fetching tutor details
  stripePromise = loadStripe(environment.stripePublishableKey);

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
      this.listingId = Number(params.get('id'));
      if (!isNaN(this.listingId)) {
        this.loadListing(this.listingId);
      } else {
        console.error('Listing Id is missing');
        this.loading = false;
      }
    });
    this.paypalService.loadPayPalScript().then(() => {
      this.renderPayPalButton();
    });
  }

  loadListing(listingId: number): void {
    this.listingService.getListing(listingId).subscribe({
      next: (listing) => {
        this.listing = listing;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch listing:', err);
      },
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  async onPaymentMethodChange(method: 'card' | 'paypal'): Promise<void> {
    this.paymentMethod = method;
    if (method === 'card') {
      await this.handleStripePayment();
    }
  }

  renderPayPalButton(): void {
    const paypal = (window as any).paypal;
  
    if (paypal && paypal.Buttons) {
      paypal.Buttons({
        createOrder: (data: any, actions: any) => {
          return this.paymentService.createPayment(69.00, 'AUD').toPromise().then((order) => {
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
    if (this.listingId) {
      // Navigate to the booking route with the listing Id
      this.router.navigate(['/booking', this.listingId]);
      console.log(`Payment initiated via ${this.paymentMethod}`);
    } else {
      console.error('Listing Id is missing');
    }
  }

  async handleStripePayment(): Promise<void> {
    const stripe = await this.stripePromise;
    if (!stripe) {
      console.error('Stripe could not be initialized');
      return;
    }
  
    try {
      // Call backend to create a Stripe Checkout Session
      this.paymentService.createStripeSession(this.listingId, this.subscription.price).subscribe({
        next: (session: { id: string, url: string }) => {
          if (session.url) {
            // Redirect to the Stripe Checkout Session URL
            window.location.href = session.url;
          } else {
            console.error('Failed to retrieve Stripe session URL');
          }
        },
        error: (error) => {
          console.error('Error creating Stripe session:', error);
        },
      });
    } catch (error) {
      console.error('Error initiating Stripe payment:', error);
    }
  }
}
