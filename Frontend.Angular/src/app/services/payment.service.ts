import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentHistory } from '../models/payment-history';
import { Card } from '../models/card';

@Injectable({
  providedIn: 'root',
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    if (!token) {
      throw new Error('User is not authenticated.');
    }

    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  getPaymentHistory(): Observable<PaymentHistory> {
    return this.http.get<PaymentHistory>(`${this.apiUrl}/history`, {
      headers: this.getAuthHeaders(),
    });
  }

  createPayment(amount: number, currency: string): Observable<any> {
    const body = { amount, currency };

    return this.http.post(`${this.apiUrl}/create-payment`, body, {
      headers: this.getAuthHeaders(),
    });
  }

  createStripeSession(listingId: number | null, price: number): Observable<any> {
    const body = { listingId, price };

    return this.http.post(`${this.apiUrl}/create-stripe-session`, body, {
      headers: this.getAuthHeaders(),
    });
  }

  capturePayment(orderId: string): Observable<any> {
    const body = { orderId };

    return this.http.post(`${this.apiUrl}/capture-payment`, body, {
      headers: this.getAuthHeaders(),
    });
  }

  addPayPalAccount(payPalEmail: string): Observable<any> {
    const body = { payPalEmail };

    return this.http.post(`${this.apiUrl}/add-paypal-account`, body, {
      headers: this.getAuthHeaders(),
    });
  }

  getSavedCards(): Observable<Card[]> {
    return this.http.get<Card[]>(`${this.apiUrl}/saved-cards`, {
      headers: this.getAuthHeaders(),
    });
  }

  saveCard(stripeToken: string, purpose: string): Observable<any> {
    const body = { stripeToken, purpose };

    return this.http.post(`${this.apiUrl}/save-card`, body, {
      headers: this.getAuthHeaders(),
    });
  }

  removeCard(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/remove-card/${id}`, {
      headers: this.getAuthHeaders(),
    });
  }
}
