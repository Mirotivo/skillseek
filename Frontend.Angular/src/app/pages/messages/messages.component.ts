import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { ChatService } from '../../services/chat.service';
import { Contact } from '../../models/contact';
import { PropositionService } from '../../services/proposition.service';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent],
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
})
export class MessagesComponent implements OnInit {
  contacts: Contact[] = [];
  selectedContact: Contact | null = null; // Default to no selection
  newMessage = '';
  loading = true; // Add a loading state
  messageSuccess: boolean = false; // Feedback for successful message sending

  activeTab: string = 'propositions'; // Default active tab
  propositions: any[] = [];
  lessons: any[] = [];

  constructor(
    private chatService: ChatService,
    private propositionService: PropositionService
  ) {}

  ngOnInit(): void {
    this.loadContacts();
  }

  loadContacts(): void {
    this.chatService.getChats().subscribe({
      next: (data) => {
        this.contacts = data;
        this.selectedContact = this.contacts[0] || null; // Set the first contact as selected if available
        if (this.selectedContact) {
          this.loadPropositions(this.selectedContact.studentId);
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to fetch contacts:', err);
        this.loading = false;
      },
    });
  }

  selectContact(contact: Contact): void {
    this.selectedContact = contact;
    this.loadPropositions(contact.studentId);
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

  sendMessage(): void {
    if (this.newMessage.trim() && this.selectedContact) {
      console.log(`Message to ${this.selectedContact.name}: ${this.newMessage}`);

      this.chatService.sendMessage({
        recipientId: this.selectedContact.studentId,
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
}
