import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-header',
  imports: [CommonModule],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent {
  isMenuOpen = false;
  roles: string[] = [];
  currentRole: 'student' | 'tutor' = 'student';

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }


  ngOnInit(): void {
    this.roles = this.authService.getRoles();

    const savedRole = this.authService.getCurrentRole();
    if (savedRole && this.roles.includes(savedRole)) {
      this.currentRole = savedRole as 'student' | 'tutor';
    } else if (this.roles.length > 0) {
      this.currentRole = this.roles[0] as 'student' | 'tutor';
      this.authService.saveCurrentRole(this.currentRole);
    }
  }

  switchRole(role: 'student' | 'tutor'): void {
    if (this.currentRole !== role) {
      this.currentRole = role;
      this.authService.saveCurrentRole(role);
    }
  }

  toggleMenu() {
    this.isMenuOpen = !this.isMenuOpen;
  }

  hasMultipleRoles(): boolean {
    return this.roles.length > 1;
  }

  isLoggedIn(): boolean {
    return !!this.authService.getToken();
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/']);
  }
}