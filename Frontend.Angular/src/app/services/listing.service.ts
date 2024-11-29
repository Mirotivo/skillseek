import { Injectable } from '@angular/core';
import { Listing } from '../models/listing';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ListingService {

  private apiUrl = `${environment.apiUrl}/listings`;

  constructor(private http: HttpClient) {}

  getRandomListings(): Observable<Listing[]> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<Listing[]>(`${this.apiUrl}/dashboard`, { headers });
  }

  getListings(): Observable<Listing[]> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<Listing[]>(this.apiUrl, { headers });
  }

}
