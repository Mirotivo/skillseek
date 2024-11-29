import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  isMenuOpen = false;

  constructor(private router: Router) {}

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  logout() {
    // Remove token from localStorage
    localStorage.removeItem('token');

    // Navigate to the home page
    this.router.navigate(['/']);
  }
}