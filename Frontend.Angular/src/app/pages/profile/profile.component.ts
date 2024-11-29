import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HeaderComponent } from '../../components/header/header.component';
import { NavigationBarComponent } from '../../components/navigation-bar/navigation-bar.component';
import { User } from '../../models/user';
import { UserService } from '../../services/user.service';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, HeaderComponent, NavigationBarComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  profile: User | null = null;

  notifications = {
    sms: [
      { label: 'Lesson Requests', enabled: true },
    ],
    email: [
      { label: 'Account activity', enabled: true },
      { label: 'Lesson Requests', enabled: true },
      { label: 'Offers concerning my listings', enabled: false },
      { label: 'Newsletter', enabled: true },
    ],
  };

  deleteConfirmation = false;
  constructor(private userService: UserService) { }

  ngOnInit(): void {
    this.loadUserProfile();
  }

  private loadUserProfile(): void {
    this.userService.getUser().subscribe(
      (user) => {
        this.profile = user;
      },
      (error) => {
        console.error('Failed to fetch user profile', error);
      }
    );
  }
}
