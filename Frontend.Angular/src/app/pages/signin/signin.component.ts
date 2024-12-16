import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signin',
  templateUrl: './signin.component.html',
  styleUrls: ['./signin.component.scss'],
  imports: [CommonModule, FormsModule, RouterModule],
})
export class SigninComponent {
  loginModel = {
    email: '',
    password: ''
  };
  invalidLogin = false;
  emailInvalid = false;

  constructor(private router: Router, private authService: AuthService) {}

  // Validate the email format
  isValidEmail(email: string): boolean {
    const emailPattern = /^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$/;
    return emailPattern.test(email);
  }

  // Check if the form is valid
  isFormValid(): boolean {
    const isEmailValid = this.isValidEmail(this.loginModel.email);
    if (!isEmailValid) {
      this.emailInvalid = true;
    }
    return (
      this.loginModel.email.trim() !== '' &&
      this.loginModel.password.trim() !== '' &&
      isEmailValid
    );
  }

  // Handle the login form submission
  async onLogin(): Promise<void> {
    if (!this.isFormValid()) {
      return;
    }

    try {
      const result = await this.authService.login(
        this.loginModel.email,
        this.loginModel.password
      );

      if (result) {
        this.authService.saveToken(result.token);
        this.authService.saveRoles(result.roles);
        this.authService.saveEmail(this.loginModel.email);

        // Redirect to returnUrl or dashboard
        const returnUrl = new URLSearchParams(window.location.search).get(
          'returnUrl'
        );
        this.router.navigateByUrl(returnUrl || '/dashboard');
      } else {
        this.invalidLogin = true;
      }
    } catch (error) {
      console.error(error);
      this.invalidLogin = true;
    }
  }
}
