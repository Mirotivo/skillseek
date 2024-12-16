import { Component, Input, OnInit } from '@angular/core';
import { loadStripe, Stripe } from '@stripe/stripe-js';
import { environment } from '../../environments/environment';
import { PaymentService } from '../../services/payment.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Card, CardType } from '../../models/card';

@Component({
  selector: 'app-manage-cards',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-cards.component.html',
  styleUrl: './manage-cards.component.scss'
})
export class ManageCardsComponent implements OnInit {
  stripe: Stripe | null = null;
  cardElement: any;
  cardHolderName = '';
  loading = false;
  cardInitialized = false;
  savedCards: Card[] = [];

  @Input() cardPurpose: CardType = CardType.Receiving; // Input for card purpose

  constructor(private paymentService: PaymentService) {}

  ngOnInit(): void {
    this.loadSavedCards();
    this.initializeCardElement();
  }

  loadSavedCards(): void {
    this.paymentService.getSavedCards().subscribe({
      next: (cards) => {
        // Filter cards based on the purpose input
        this.savedCards = cards.filter(card => card.purpose === this.cardPurpose);
        console.log('Saved cards:', this.savedCards);
      },
      error: (err) => {
        console.error('Failed to load saved cards', err);
      }
    });
  }

  async initializeCardElement() {
    if (this.cardInitialized) {
      return; // Avoid reinitializing
    }

    this.stripe = await loadStripe(environment.stripePublishableKey);
    if (!this.stripe) {
      console.error('Stripe could not be initialized');
      return;
    }

    const elements = this.stripe.elements();
    this.cardElement = elements.create('card');

    const retryMounting = setInterval(() => {
      const cardElementContainer = document.getElementById('card-element');
      if (cardElementContainer) {
        this.cardElement.mount('#card-element');
        console.log('Card element mounted successfully.');
        this.cardInitialized = true;
        clearInterval(retryMounting);
      }
    }, 500);
  }

  async saveCard(event: Event) {
    event.preventDefault();

    if (!this.stripe || !this.cardElement || !this.cardHolderName) {
      alert('Please fill in all details.');
      return;
    }

    const { token, error } = await this.stripe.createToken(this.cardElement, {
      name: this.cardHolderName,
    });

    if (error) {
      console.error('Error creating token:', error);
      return;
    }

    this.paymentService.saveCard(token.id, this.cardPurpose).subscribe({
      next: () => {
        this.loadSavedCards();
      },
      error: (err) => {
        console.error('Error saving card:', err);
        alert('Failed to save card.');
      },
    });
  }

  setAsDefault(id: number): void {
    this.savedCards.forEach(method => (method.isDefault = false));
    const selectedMethod = this.savedCards.find(method => method.id === id);
    if (selectedMethod) {
      selectedMethod.isDefault = true;
    }
  }

  removeCard(id: number): void {
    this.paymentService.removeCard(id).subscribe({
      next: () => {
        this.loadSavedCards();
      },
      error: (err) => {
        console.error('Failed to remove card', err);
      }
    });
  }
}
