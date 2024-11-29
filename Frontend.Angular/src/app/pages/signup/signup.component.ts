import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signup',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss'],
})
export class SignupComponent {
  signupModel = {
    email: '',
    password: '',
    verifyPassword: '',
  };
  emailInvalid = false;
  passwordMismatch = false;
  signupError = '';

  constructor(private router: Router, private authService: AuthService) {}

  // Validate the email format
  isValidEmail(email: string): boolean {
    const emailPattern = /^[A-Za-z0-9._%-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}$/;
    return emailPattern.test(email);
  }

  // Check if the form is valid
  isFormValid(): boolean {
    const isEmailValid = this.isValidEmail(this.signupModel.email);
    const isPasswordMatch =
      this.signupModel.password === this.signupModel.verifyPassword;

    if (!isEmailValid) {
      this.emailInvalid = true;
    }
    if (!isPasswordMatch) {
      this.passwordMismatch = true;
    }
    return (
      this.signupModel.email.trim() !== '' &&
      this.signupModel.password.trim() !== '' &&
      this.signupModel.verifyPassword.trim() !== '' &&
      isEmailValid &&
      isPasswordMatch
    );
  }

  // Clear the email input
  clearEmail() {
    this.signupModel.email = '';
    this.emailInvalid = false;
  }

  // Handle the form submission
  async onSignup(): Promise<void> {
    if (!this.isFormValid()) {
      return;
    }

    const errorMessage = await this.authService.register(
      this.signupModel.email,
      this.signupModel.password,
      this.signupModel.verifyPassword
    );

    if (!errorMessage) {
      // Navigate to the sign-in page on success
      this.router.navigate(['/signin']);
    } else {
      // Display the error message
      this.signupError = errorMessage;
    }
  }
}
