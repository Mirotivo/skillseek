import { Transaction } from "./transaction";

export interface PaymentHistory {
  walletBalance: number;
  totalAmountCollected: number;
  transactions: Transaction[];
}
