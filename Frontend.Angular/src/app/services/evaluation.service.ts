import { Injectable } from '@angular/core';
import { Review } from '../models/review';
import { Observable } from 'rxjs';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EvaluationService {
  private apiUrl = `${environment.apiUrl}/evaluations`;

  constructor(private http: HttpClient) { }

  getAllReviews(): Observable<{ pendingReviews: Review[]; receivedReviews: Review[]; sentReviews: Review[], recommendations: Review[] }> {
    const token = localStorage.getItem('token');

    if (!token) {
      console.error('No token found in localStorage');
      throw new Error('No authentication token available');
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<{ pendingReviews: Review[]; receivedReviews: Review[]; sentReviews: Review[], recommendations: Review[] }>(this.apiUrl, {
      headers,
    });
  }

  submitReview(review: Review): Observable<any> {
    const token = localStorage.getItem('token');

    if (!token) {
      console.error('No token found in localStorage');
      throw new Error('No authentication token available');
    }
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.post(`${this.apiUrl}/review`, review, { headers });
  }

  submitRecommendation(recommendation: Review): Observable<any> {
    const token = localStorage.getItem('token');

    if (!token) {
      console.error('No token found in localStorage');
      throw new Error('No authentication token available');
    }
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.post(`${this.apiUrl}/recommendation`, recommendation, { headers });
  }
}
