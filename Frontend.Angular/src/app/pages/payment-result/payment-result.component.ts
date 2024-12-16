import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../services/payment.service';
import { SubscriptionService } from '../../services/subscription.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-payment-result',
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-result.component.html',
  styleUrl: './payment-result.component.scss'
})
export class PaymentResultComponent implements OnInit {
  success: boolean = false;
  listingId!: number;
  gateway!: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private paymentService: PaymentService,
    private subscriptionService: SubscriptionService
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      this.success = params['success'] === 'true';
      this.listingId = Number(params['listingId']);
      this.gateway = params['gateway'];

      if (this.listingId && this.gateway) {
        if (this.success) {
          this.handlePaymentSuccess();
        } else {
          this.handlePaymentCancel();
        }
      } else {
        console.error('Missing required parameters for payment result.');
        this.router.navigate(['/payment', this.listingId]);
      }
    });
  }

  handlePaymentSuccess(): void {
    console.log('Processing successful payment...');

    if (this.gateway === 'PayPal' || this.gateway === 'Stripe') {
      const paymentId = this.route.snapshot.queryParams['paymentId'] || '0';

      if (!paymentId) {
        console.error('Missing payment ID for successful payment.');
        this.router.navigate(['/payment', this.listingId]);
        return;
      }

      // Capture the payment
      this.paymentService.capturePayment(this.gateway, paymentId).subscribe({
        next: () => {
          console.log('Payment successfully captured.');
          this.createSubscription();
        },
        error: (err) => {
          console.error('Error capturing payment:', err);
          this.router.navigate(['/payment', this.listingId]);
        },
      });
    }
  }

  handlePaymentCancel(): void {
    console.log('Payment canceled by the user.');
    // Optionally redirect to the payment page or show a cancellation message
    this.router.navigate(['/']);
  }

  createSubscription(): void {
    const subscriptionRequest = {
      amount: 69, // Replace with dynamic amount if needed
      paymentMethod: this.gateway
    };

    this.subscriptionService.createSubscription(subscriptionRequest).subscribe({
      next: (response) => {
        console.log('Subscription created successfully:', response);
        this.router.navigate(['/booking', this.listingId]);
      },
      error: (err) => {
        console.error('Error creating subscription:', err);
        this.router.navigate(['/payment', this.listingId]);
      }
    });
  }

  retryPayment(): void {
    this.router.navigate(['/payment', this.listingId]);
  }  
}
