import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';
import { PaypalService } from '../../services/paypal.service';
import { PaymentService } from '../../services/payment.service';
import { Transaction } from '../../models/transaction';
import { MapAddressComponent } from '../../components/map-address/map-address.component';
import { PaymentHistory } from '../../models/payment-history';
import { ManageCardsComponent } from '../../components/manage-cards/manage-cards.component';
import { ActivatedRoute } from '@angular/router';
import { Card, CardType } from '../../models/card';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent, MapAddressComponent, ManageCardsComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  CardType = CardType;
  activeTab: string = 'profile';
  activeSubTab: string = 'history';
  profile: User | null = null;
  paypalAccountAdded: boolean = false; // Flag to track if the PayPal account is added

  notifications = {
    sms: [
      { label: 'Lesson Requests', enabled: true },
    ],
    email: [
      { label: 'Account activity', enabled: true },
      { label: 'Lesson Requests', enabled: true },
      { label: 'Offers concerning my listings', enabled: false },
      { label: 'Newsletter', enabled: true },
    ],
  };
  payment: PaymentHistory | null = null;


  deleteConfirmation = false;

  paymentPreference: string = 'afterEachLesson';

  compensationPercentage: number = 50; // Default value


  constructor(
    private userService: UserService,
    private paypalService: PaypalService,
    private paymentService: PaymentService,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.activeTab = params['section'] || 'profile';
      this.activeSubTab = params['detail'] || 'history';
    });

    this.loadUserProfile();
    this.loadPaymentHistory();
    // this.initializePayPalButton();
  }

  async onSubTabChange(subTab: string): Promise<void> {
    this.activeSubTab = subTab;

    // Check if receiving sub-tab is activated
    if (this.activeSubTab === 'receiving') {
      setTimeout(() => {
        // this.initializePayPalButton();
      }, 1000); // Delay of 1 second
    }
  }
 
  loadUserProfile(): void {
    this.userService.getUser().subscribe(
      (user) => {
        this.profile = user;
      },
      (error) => {
        console.error('Failed to fetch user profile', error);
      }
    );
  }

  loadPaymentHistory(): void {
    this.paymentService.getPaymentHistory().subscribe({
      next: (data) => {
        this.payment = data;
      },
      error: (err) => {
        console.error('Failed to fetch payment history', err);
      }
    });
  }

  setPaymentPreference(preference: string) {
    this.paymentPreference = preference;
  }


  adjustCompensation(value: number) {
    const newPercentage = this.compensationPercentage + value;
    if (newPercentage >= 0 && newPercentage <= 100) {
      this.compensationPercentage = newPercentage;
    }
  }

  initializePayPalButton(): void {
    this.paypalService.loadPayPalScript().then(() => {
      (window as any).paypal
        .Buttons({
          style: {
            layout: 'vertical',
            label: 'paypal', // Use login to indicate account linking
          },
          fundingSource: (window as any).paypal.FUNDING.PAYPAL, // Use the PayPal funding source for payouts
          onInit: (data: any, actions: any) => {
            console.log('PayPal button initialized.');
          },
          onClick: async (data: any, actions: any) => {
            console.log('PayPal button clicked for account linking.');
          },
          onApprove: async (data: any, actions: any) => {
            // Capture the authorization
            console.log('PayPal account successfully linked:', data);

            // Save linked account information
            const linkedAccountEmail = 'example@paypal.com'; // Replace with actual email retrieval
            this.paymentService.addPayPalAccount(linkedAccountEmail).subscribe(() => {
              console.log('PayPal account saved in the backend.');
              alert('Your PayPal account has been linked successfully!');
              this.paypalAccountAdded = true;
            });
          },
          onError: (err: any) => {
            console.error('Error linking PayPal account:', err);
            alert('Failed to link your PayPal account. Please try again.');
          },
        })
        .render('#paypal-button-container'); // Render button in the specified container
    });
  }


  confirmPayPalAccount(): void {
    // Placeholder for confirming the PayPal account
    alert('Your PayPal account has been added successfully!');
  }

  updateAddress(newAddress: string): void {
    if (this.profile) {
      this.profile.address = newAddress;
    } else {
      console.warn('Profile is null. Unable to update the address.');
    }
  }

  saveProfile(): void {
    if (this.profile) {
      this.userService.updateUser(this.profile).subscribe({
        next: () => {

        },
        error: (err) => {
          console.error('Failed to update profile', err);
          alert('Failed to update profile. Please try again.');
        }
      });
    } else {
      console.warn('Profile is null. Nothing to update.');
    }
  }
}
