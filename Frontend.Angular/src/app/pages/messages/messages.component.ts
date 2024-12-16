import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { ChatService } from '../../services/chat.service';
import { Chat } from '../../models/chat';
import { PropositionService } from '../../services/proposition.service';
import { ModalComponent } from '../../components/modal/modal.component';
import { ProposeLessonComponent } from '../../components/propose-lesson/propose-lesson.component';
import { Listing } from '../../models/listing';
import { ListingService } from '../../services/listing.service';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent, ProposeLessonComponent, ModalComponent],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
})
export class MessagesComponent implements OnInit {
  contacts: Chat[] = [];
  selectedContact: Chat | null = null;
  selectedListing!: Listing;
  newMessage = '';
  loading = true; // Add a loading state
  messageSuccess: boolean = false; // Feedback for successful message sending

  activeTab: string = 'propositions'; // Default active tab
  propositions: any[] = [];
  lessons: any[] = [];

  constructor(
    private chatService: ChatService,
    private propositionService: PropositionService,
    private listingService: ListingService
  ) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts(): void {
    this.chatService.getChats().subscribe({
      next: (data) => {
        this.contacts = data;
        if (this.contacts.length > 0) {
          this.selectContact(this.contacts[0]);
        }  
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch contacts:', err);
        this.loading = false;
      },
    });
  }

  // Reload a specific contact by ID
  selectContactById(contactId: number): void {
    const contact = this.contacts.find((c) => c.id === contactId);
    if (contact) {
      this.selectContact(contact);
    }
  }
  
  selectContact(contact: Chat): void {
    this.selectedContact = contact;
    this.loadPropositions(contact.studentId);
    this.loadListing(this.selectedContact.listingId);
  }

  loadPropositions(contactId: number): void {
    this.propositionService.getPropositions(contactId).subscribe({
      next: (response) => {
        this.propositions = response.propositions;
        this.lessons = response.lessons;
      },
      error: (err) => {
        console.error('Failed to fetch contact details:', err);
      }
    });
  }

  loadListing(listingId: number): void {
    this.listingService.getListing(listingId).subscribe({
      next: (listing) => {
        this.selectedListing = listing;
      },
      error: (err) => {
        console.error('Failed to fetch listing:', err);
      },
    });
  }

  sendMessage(): void {
    if (this.newMessage.trim() && this.selectedContact) {
      console.log(`Message to ${this.selectedContact.name}: ${this.newMessage}`);

      this.chatService.sendMessage({
        listingId: this.selectedContact.listingId,
        recipientId: this.selectedContact.recipientId,
        content: this.newMessage,
      }).subscribe({
        next: () => {
          this.selectedContact?.messages.push({
            text: this.newMessage,
            sentBy: 'me',
            timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }),
          });

          this.messageSuccess = true; // Show success feedback
          this.newMessage = ''; // Clear the input field
          setTimeout(() => (this.messageSuccess = false), 3000); // Hide success after 3 seconds
        },
        error: (err) => {
          console.error('Failed to send message:', err);
        },
      });
    }
  }

  respondToProposition(propositionId: number, accept: boolean): void {
    this.propositionService.respondToProposition(propositionId, accept).subscribe({
      next: () => {
        // Update the UI after successful response
        this.propositions = this.propositions.filter(p => p.id !== propositionId);
      },
      error: (err) => {
        console.error('Failed to respond to proposition:', err);
      }
    });
  }



  isModalOpen = false;

  openModal(): void {
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
    // Reload the listing for the selected contact
    if (this.selectedContact) {
      this.selectContact(this.selectedContact);
    }
  }

  handleProposeLesson(event: { date: string; duration: number; price: number }): void {
    console.log('Lesson proposed:', event);
    // Perform the action, e.g., send the proposal to the backend
  }

}
