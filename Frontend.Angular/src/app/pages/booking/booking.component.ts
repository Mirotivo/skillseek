import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ListingService } from '../../services/listing.service';
import { Listing } from '../../models/listing';
import { ChatService } from '../../services/chat.service';
import { PropositionService } from '../../services/proposition.service';
import { LessonProposition } from '../../models/lesson-proposotion';

@Component({
  selector: 'app-booking',
  imports: [CommonModule, FormsModule],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.scss'],
})
export class BookingComponent implements OnInit {
  tutor: Listing | null = null;
  newMessage: string = '';
  lessonDate: string = ''; // ISO date string
  lessonDuration: number = 1; // Duration in hours
  lessonPrice: number = 0; // Price in dollars
  loading: boolean = true;
  messageSuccess: boolean = false; // Indicates whether the message was sent successfully
  proposeSuccess: boolean = false; // Indicates whether the lesson proposal was successful
  showMessageSection: boolean = true; // Toggle between message and propose lesson sections

  constructor(
    private route: ActivatedRoute,
    private listingService: ListingService,
    private chatService: ChatService,
    private propositionService: PropositionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Fetch the tutor ID from route params
    this.route.paramMap.subscribe((params) => {
      const listingId = params.get('id');
      if (listingId) {
        this.fetchTutorDetails(listingId);
      } else {
        console.error('Tutor ID not found');
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  fetchTutorDetails(id: string): void {
    // Fetch the listing details for the given tutor ID
    this.listingService.getRandomListings().subscribe({
      next: (listings) => {
        this.tutor = listings.find((listing) => listing.id.toString() === id) || null;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch tutor details:', err);
        this.loading = false;
      },
    });
  }

  sendMessage(): void {
    if (this.newMessage.trim()) {
      console.log(`Message to ${this.tutor?.name}: ${this.newMessage}`);
      this.chatService.sendMessage({
        recipientId: this.tutor?.id!,
        content: this.newMessage,
      }).subscribe({
        next: () => {
          this.messageSuccess = true;
          this.showMessageSection = false; // Hide the message section
          this.newMessage = '';
          setTimeout(() => {
            this.messageSuccess = false;
          }, 3000); // Keep the message success indicator for 3 seconds
        },
        error: (err) => {
          console.error('Failed to send message:', err);
        },
      });
    }
  }

  proposeLesson(): void {
    if (!this.tutor) {
      console.error('Tutor details not available');
      return;
    }

    const lessonProposition: LessonProposition = {
      date: this.lessonDate,
      duration: this.lessonDuration, // Convert to "HH:mm:ss"
      price: this.lessonPrice,
      tutorId: this.tutor.id,
    };

    this.propositionService.proposeLesson(lessonProposition).subscribe({
      next: () => {
        this.proposeSuccess = true;
        setTimeout(() => {
          this.proposeSuccess = false;
        }, 3000); // Keep the success message for 3 seconds
      },
      error: (err) => {
        console.error('Failed to propose lesson:', err);
      },
    });
  }
}
