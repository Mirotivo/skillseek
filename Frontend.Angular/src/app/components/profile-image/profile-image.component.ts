import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-profile-image',
  imports: [CommonModule, FormsModule],
  templateUrl: './profile-image.component.html',
  styleUrl: './profile-image.component.scss'
})
export class ProfileImageComponent {
  @Input() profileImage?: string; // Profile image URL
  @Input() firstName?: string;    // User's first name
  @Input() lastName?: string;     // User's last name

  getInitials(): string {
    const firstInitial = this.firstName ? this.firstName.charAt(0).toUpperCase() : '';
    const lastInitial = this.lastName ? this.lastName.charAt(0).toUpperCase() : '';
    return `${firstInitial}${lastInitial}`;
  }

  get showImage(): boolean {
    // Show the image only if profileImage is defined and non-empty
    return !!this.profileImage;
  }

  get showInitials(): string | boolean | undefined {
    // Show initials only if firstName or lastName is available
    return !this.profileImage && (this.firstName || this.lastName);
  }
}
