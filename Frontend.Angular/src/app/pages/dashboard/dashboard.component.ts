import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// Services
import { UserService } from '../../services/user.service';
import { ChatService } from '../../services/chat.service';
import { PaymentService } from '../../services/payment.service';
import { ListingService } from '../../services/listing.service';
import { EvaluationService } from '../../services/evaluation.service';

// Models
import { Listing } from '../../models/listing';
import { Review } from '../../models/review';
import { Transaction } from '../../models/transaction';
import { User } from '../../models/user';

// Components
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { LeaveReviewComponent } from '../../components/leave-review/leave-review.component';
import { ModalComponent } from '../../components/modal/modal.component';
import { ProfileImageComponent } from '../../components/profile-image/profile-image.component';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent, ModalComponent, LeaveReviewComponent, ProfileImageComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  user!: User;
  messages: any[] = [];
  reviewsPending: Review[] = [];
  payments: Transaction[] = [];
  listings: Listing[] = [];
  selectedRevieweeId!: number;

  constructor(
    private userService: UserService,
    private chatService: ChatService,
    private paymentService: PaymentService,
    private evaluationService: EvaluationService,
    private listingService: ListingService
  ) { }

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.userService.getUser().subscribe({
      next: (userData) => (this.user = userData),
      error: (err) => console.error('Failed to load user data:', err),
    });
    this.chatService.getMessages().subscribe({
      next: (data) => (this.messages = data),
      error: (err) => console.error('Failed to load chat messages:', err),
    });
    this.evaluationService.getAllReviews().subscribe({
      next: (data) => (this.reviewsPending = data.pendingReviews),
      error: (err) => console.error('Failed to load reviews:', err),
    });
    this.paymentService.getPaymentHistory().subscribe({
      next: (response) => (this.payments = response.transactions),
      error: (err) => console.error('Failed to load payment history:', err),
    });
    this.listingService.getListings().subscribe({
      next: (data) => (this.listings = data),
      error: (err) => console.error('Failed to fetch listings:', err),
    });
  }


  isModalOpen = false;

  openModal(revieweeId: number): void {
    this.selectedRevieweeId = revieweeId;
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }
}
