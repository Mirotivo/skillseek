import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { Review } from '../../models/review';
import { EvaluationService } from '../../services/evaluation.service';
import { ModalComponent } from '../../components/modal/modal.component';
import { LeaveReviewComponent } from '../../components/leave-review/leave-review.component';

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

  constructor(private evaluationService: EvaluationService) {}

  ngOnInit(): void {
    this.loadAllReviews();
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
