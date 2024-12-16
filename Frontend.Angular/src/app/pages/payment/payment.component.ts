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
import { SubscriptionService } from '../../services/subscription.service';

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
    private paymentService: PaymentService,
    private subscriptionService: SubscriptionService
  ) { }

  ngOnInit(): void {
    // Get the tutor Id from the route parameters
    this.route.paramMap.subscribe((params) => {
      this.listingId = Number(params.get('id'));
      if (!isNaN(this.listingId)) {
        this.loadListing(this.listingId);
        this.checkSubscriptionStatus(); // Check subscription status
      } else {
        console.error('Listing Id is missing');
        this.loading = false;
      }
    });
    this.paypalService.loadPayPalScript().then(() => {
      this.renderPayPalButton();
    });
  }

  checkSubscriptionStatus(): void {
    this.subscriptionService.checkActiveSubscription().subscribe({
      next: (response: { isActive: boolean }) => {
        if (response.isActive) {
          this.router.navigate(['/booking', this.listingId]); // Redirect to booking if already subscribed
        }
        // else {
        //   this.renderPaymentOptions(); // Show payment options if not subscribed
        // }
      },
      error: (err) => {
        console.error('Error checking subscription status:', err);
      },
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

  async handleStripePayment(): Promise<void> {
    const stripe = await this.stripePromise;
    if (!stripe) {
      console.error('Stripe could not be initialized');
      return;
    }

    try {
      // Call backend to create a Stripe Checkout Session
      this.paymentService.createPayment("Stripe", this.listingId, this.subscription.price).subscribe({
        next: (session: { id: string, approvalUrl: string }) => {
          if (session.approvalUrl) {
            // Redirect to the Stripe Checkout Session URL
            window.location.href = session.approvalUrl;
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

  renderPayPalButton(): void {
    const paypal = (window as any).paypal;

    if (paypal && paypal.Buttons) {
      paypal.Buttons({
        createOrder: (data: any, actions: any) => {
          return new Promise<string>((resolve, reject) => {
            this.paymentService.createPayment("PayPal", this.listingId, 69.00).subscribe({
              next: (order) => {
                if (!order || !order.paymentId) {
                  console.error('Payment Id not returned from the server.');
                  reject('Payment Id not returned from the server.');
                  return;
                }
                resolve(order.paymentId);
              },
              error: (error) => {
                console.error('Error creating order:', error);
                reject(error);
              },
            });
          });
        },
        onApprove: (data: any, actions: any) => {
          console.log('Payment approved, redirecting to result...');
          this.router.navigate(['/payment-result'], {
            queryParams: {
              success: true,
              listingId: this.listingId,
              gateway: 'PayPal',
              paymentId: data.orderID,
            },
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
}
