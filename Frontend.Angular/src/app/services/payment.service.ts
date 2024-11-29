import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  getPayments() {
    return [
      { date: '01/11/2024', description: 'Transfer by PayPal account', amount: -30 },
      { date: '01/11/2024', description: 'Payment from Lole', amount: 30 },
      { date: '24/10/2024', description: 'Transfer by PayPal account', amount: -30 },
    ];
  }
}