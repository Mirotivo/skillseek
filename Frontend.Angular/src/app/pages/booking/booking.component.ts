import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ListingService } from '../../services/listing.service';
import { Listing } from '../../models/listing';
import { ChatService } from '../../services/chat.service';
import { PropositionService } from '../../services/proposition.service';
import { Proposition } from '../../models/proposition';
import { ProposeLessonComponent } from '../../components/propose-lesson/propose-lesson.component';

@Component({
  selector: 'app-booking',
  imports: [CommonModule, FormsModule, ProposeLessonComponent],
  templateUrl: './booking.component.html',
  styleUrls: ['./booking.component.scss'],
})
export class BookingComponent implements OnInit {
  listing!: Listing;
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
  ) { }

  ngOnInit(): void {
    // Fetch the tutor ID from route params
    this.route.paramMap.subscribe((params) => {
      const listingId = Number(params.get('id'));
      if (!isNaN(listingId)) {
        this.loadListing(listingId);
      } else {
        console.error('Tutor ID not found');
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/']);
  }

  loadListing(listingId: number): void {
    this.listingService.getListing(listingId).subscribe({
      next: (listing) => {
        this.listing = listing;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch listing:', err);
      },
    });
  }

  sendMessage(): void {
    if (this.newMessage.trim()) {
      console.log(`Message to ${this.listing?.name}: ${this.newMessage}`);
      this.chatService.sendMessage({
        listingId: this.listing?.id!,
        recipientId: 0,
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

  handleProposeLesson(): void {
    this.proposeSuccess = true;
    setTimeout(() => {
      this.proposeSuccess = false;
    }, 3000); // Keep the success message for 3 seconds
  }
}
