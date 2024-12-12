import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { PropositionService } from '../../services/proposition.service';
import { Proposition } from '../../models/proposition';
import { Listing } from '../../models/listing';

@Component({
  selector: 'app-propose-lesson',
  imports: [FormsModule, CommonModule],
  templateUrl: './propose-lesson.component.html',
  styleUrl: './propose-lesson.component.scss'
})
export class ProposeLessonComponent {
  @Input() listing!: Listing;
  @Input() studentId: number | null = null;
  @Output() onPropose = new EventEmitter<{ date: string; duration: number; price: number }>();
  @Output() onClose = new EventEmitter<void>();
  minDate: string = this.getTodayDate();
  lessonDate: string = this.getTodayDate();
  lessonDuration: number = 1;
  lessonPrice: number = 0;
  proposeSuccess = false;

  /**
   *
   */
  constructor(
    private propositionService: PropositionService,
  ) { }

  ngOnInit(): void {
    // Set the lesson price to the hourRate of the listing
    if (this.listing) {
      this.lessonPrice = this.listing.rates.hourly;
    } else {
      throw new Error('Listing input is required for ProposeLessonComponent.');
    }
  }

  ngAfterViewInit(): void {

  }
  closeModal(): void {
    this.onClose.emit();
  }

  private getTodayDate(): string {
    const today = new Date();
    return today.toISOString().split('T')[0];
  }

  proposeLesson(): void {
    if (!this.listing) {
      console.error('Tutor details not available');
      return;
    }

    if (this.lessonDate && this.lessonDuration && this.lessonPrice !== null) {
      const proposition: Proposition = {
        date: this.lessonDate,
        duration: this.lessonDuration,
        price: this.lessonPrice,
        listingId: this.listing.id,
        studentId: this.studentId
      };

      this.propositionService.proposeLesson(proposition).subscribe({
        next: () => {
          this.onPropose.emit({
            date: this.lessonDate,
            duration: this.lessonDuration,
            price: this.lessonPrice,
          });
          this.proposeSuccess = true;
          setTimeout(() => {
            this.proposeSuccess = false;
            this.closeModal();
          }, 3000); // Keep the success message for 3 seconds
        },
        error: (err) => {
          console.error('Failed to propose lesson:', err);
        },
      });
    }
  }
}
