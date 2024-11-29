import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/users`;

  async register(email: string, password: string, confirmPassword: string): Promise<string | null> {
    try {
      const response = await fetch(`${this.apiUrl}/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password, confirmPassword }),
      });

      if (response.ok) {
        return null; // Success, no error message
      }

      if (response.status === 400) {
        const errorMessage = await response.text(); // Backend error message
        return errorMessage;
      }

      return 'An unexpected error occurred. Please try again.';
    } catch (error) {
      console.error('Error during registration:', error);
      return 'An unexpected error occurred. Please check your connection.';
    }
  }

  async login(email: string, password: string): Promise<{ token: string } | null> {
    try {
      const response = await fetch(`${this.apiUrl}/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (response.ok) {
        return response.json(); // Return token if login is successful
      }

      return null; // Return null if login fails
    } catch (error) {
      console.error('Login error:', error);
      return null;
    }
  }

  saveToken(token: string): void {
    localStorage.setItem('token', token);
  }

  saveEmail(email: string): void {
    localStorage.setItem('email', email);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  logout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('email');
  }
}
