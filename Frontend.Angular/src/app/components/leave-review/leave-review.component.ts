import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Review } from '../../models/review';
import { EvaluationService } from '../../services/evaluation.service';

@Component({
  selector: 'app-leave-review',
  imports: [CommonModule, FormsModule],
  templateUrl: './leave-review.component.html',
  styleUrl: './leave-review.component.scss'
})
export class LeaveReviewComponent {
  @Input() revieweeId!: number; // Accept revieweeId as input
  @Output() onClose = new EventEmitter<void>();

  newReview: Review = {
    revieweeId: 0,
    subject: '',
    feedback: '',
    name: ''
  };

  responseMessage: string = '';

  constructor(private evaluationService: EvaluationService) {}

  ngOnInit() {
    if (this.revieweeId) {
      this.newReview.revieweeId = this.revieweeId; // Initialize revieweeId
    }
  }

  submitReview(form: NgForm) {
    if (form.invalid) {
      this.responseMessage = 'Please fill in all required fields.';
      return;
    }

    this.evaluationService.submitReview(this.newReview).subscribe({
      next: (response: any) => {
        this.responseMessage = response.message;
        form.resetForm(); // Reset the form after successful submission
      },
      error: (error) => {
        this.responseMessage = error.error || 'An error occurred while submitting the review.';
      }
    });
  }

  closeModal(): void {
    this.onClose.emit();
  }
}
