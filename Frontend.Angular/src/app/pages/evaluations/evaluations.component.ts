import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { Review } from '../../models/review';
import { EvaluationService } from '../../services/evaluation.service';

@Component({
  selector: 'app-evaluations',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent],
  templateUrl: './evaluations.component.html',
  styleUrl: './evaluations.component.scss'
})
export class EvaluationsComponent implements OnInit {
  pendingReviews: Review[] = [];
  receivedReviews: Review[] = [];
  sentReviews: Review[] = [];
  remainingReviews = 0;
  activeTab = 'received';

  constructor(private evaluationService: EvaluationService) {}

  ngOnInit(): void {
    this.loadAllReviews();
  }

  loadAllReviews(): void {
    this.evaluationService.getAllReviews().subscribe((data) => {
      this.pendingReviews = data.pendingReviews;
      this.receivedReviews = data.receivedReviews;
      this.sentReviews = data.sentReviews;
      // this.remainingReviews = data.pendingReviews.length;
    });
    }

  setActiveTab(tab: string): void {
    this.activeTab = tab;
  }
}
