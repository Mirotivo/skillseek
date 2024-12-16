import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { Review } from '../../models/review';
import { EvaluationService } from '../../services/evaluation.service';
import { ModalComponent } from '../../components/modal/modal.component';
import { LeaveReviewComponent } from '../../components/leave-review/leave-review.component';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../environments/environment';
import { UserService } from '../../services/user.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-evaluations',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent, ModalComponent, LeaveReviewComponent],
  templateUrl: './evaluations.component.html',
  styleUrl: './evaluations.component.scss'
})
export class EvaluationsComponent implements OnInit {
  pendingReviews: Review[] = [];
  receivedReviews: Review[] = [];
  sentReviews: Review[] = [];
  recommendations: Review[] = [];
  remainingReviews = 0;
  activeTab = 'reviews';
  activeSubTab = 'received';
  selectedRevieweeId!: number;
  recommendationLink: string = '';
  sponsorLink: string = '';

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private evaluationService: EvaluationService
  ) {}

  ngOnInit(): void {
    this.loadAllReviews();
    // Fetch the logged-in user's ID or username
    this.userService.getUser().subscribe({
      next: (user: User) => {
        if (user.recommendationToken) {
          this.recommendationLink = `${environment.frontendUrl}/recommendation/${user.recommendationToken}`;
          this.sponsorLink = `${environment.frontendUrl}/signup?referral=${user.recommendationToken}`;
        }
      },
      error: (err) => {
        console.error('Failed to fetch user data:', err);
      }
    });
  }

  copyLink(): void {
    navigator.clipboard.writeText(this.recommendationLink).then(() => {
      alert('Link copied to clipboard!');
    }).catch(err => {
      console.error('Could not copy link: ', err);
    });
  }

  loadAllReviews(): void {
    this.evaluationService.getAllReviews().subscribe((data) => {
      this.pendingReviews = data.pendingReviews;
      this.receivedReviews = data.receivedReviews;
      this.sentReviews = data.sentReviews;
      this.recommendations = data.recommendations;
      // this.remainingReviews = data.pendingReviews.length;
    });
    }

  setActiveTab(tab: string): void {
    this.activeSubTab = tab;
  }

  isModalOpen = false;

  openModal(revieweeId: number): void {
    this.selectedRevieweeId = revieweeId;
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  handleProposeLesson(event: { date: string; duration: number; price: number }): void {
    console.log('Lesson proposed:', event);
    // Perform the action, e.g., send the proposal to the backend
  }
}
