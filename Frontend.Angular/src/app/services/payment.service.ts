import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PaymentHistory } from '../models/payment-history';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private apiUrl = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) { }

  getPaymentHistory(): Observable<PaymentHistory> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<PaymentHistory>(`${this.apiUrl}/history`, { headers });
  }

  createPayment(amount: number, currency: string): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });

    const body = { amount, currency };

    return this.http.post(`${this.apiUrl}/create-payment`, body, { headers });
  }

  capturePayment(orderId: string): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`
    });

    const body = JSON.stringify(orderId);

    return this.http.post(`${this.apiUrl}/capture-payment`, body, { headers });
  }

  addPayPalAccount(payPalEmail: string): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
  
    return this.http.post(`${this.apiUrl}/add-paypal-account`, { payPalEmail }, { headers });
  }
  
}