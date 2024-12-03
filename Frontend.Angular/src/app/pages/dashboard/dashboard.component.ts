import { Component, OnInit } from '@angular/core';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../services/user.service';
import { ChatService } from '../../services/chat.service';
import { PaymentService } from '../../services/payment.service';
import { ListingService } from '../../services/listing.service';
import { EvaluationService } from '../../services/evaluation.service';
import { Listing } from '../../models/listing';
import { Review } from '../../models/review';
import { Contact } from '../../models/contact';
import { Transaction } from '../../models/transaction';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  user: any;
  messages: any[] = [];
  reviewsPending: Review[] = [];
  payments: Transaction[] = [];
  listings: Listing[] = [];

  constructor(
    private userService: UserService,
    private chatService: ChatService,
    private paymentService: PaymentService,
    private evaluationService: EvaluationService,
    private listingService: ListingService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.userService.getUser().subscribe((userData) => {
      this.user = userData;
    });
    this.chatService.getMessages().subscribe((data) => {
      this.messages = data;
    });
    this.evaluationService.getAllReviews().subscribe((data) => {
      this.reviewsPending = data.pendingReviews;
    });
    this.paymentService.getPaymentHistory().subscribe({
      next: (response) => {
        this.payments = response.transactions;
      },
      error: (err) => {
        console.error('Failed to load payment history:', err);
      }
    });
    this.listingService.getListings().subscribe({
      next: (data) => (this.listings = data),
      error: (err) => console.error('Failed to fetch listings:', err),
    });
  }
}
