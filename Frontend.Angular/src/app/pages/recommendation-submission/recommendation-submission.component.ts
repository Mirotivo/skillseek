import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { EvaluationService } from '../../services/evaluation.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Review } from '../../models/review';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-recommendation-submission',
  imports: [CommonModule, FormsModule],
  templateUrl: './recommendation-submission.component.html',
  styleUrl: './recommendation-submission.component.scss'
})
export class RecommendationSubmissionComponent implements OnInit {
  recommendation: Review = {
    revieweeId: 0,
    name: '',
    subject: '',
    feedback: '',
    avatar: null,
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private evaluationService: EvaluationService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const userToken = params.get('tokenId');
      if (userToken) {
        this.fetchRevieweeDetails(userToken);
      } else {
        console.error('No user token provided in route.');
        this.router.navigate(['/']);
      }
    });
  }

  fetchRevieweeDetails(userToken: string): void {
    this.userService.getUserByToken(userToken).subscribe({
      next: (user) => {
        this.recommendation.revieweeId = user.id;
        this.recommendation.name = user.name;
        this.recommendation.avatar = user.avatar || null;
      },
      error: (err) => {
        console.error('Error fetching user details:', err);
        this.router.navigate(['/']);
      }
    });
  }

  submitRecommendation(): void {
    this.evaluationService.submitRecommendation(this.recommendation).subscribe({
      next: () => {
        alert('Recommendation submitted successfully!');
        this.router.navigate(['/']);
      },
      error: (err) => {
        console.error('Error submitting recommendation:', err);
      }
    });
  }
}

