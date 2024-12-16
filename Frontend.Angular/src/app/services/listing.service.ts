import { Injectable } from '@angular/core';
import { Listing } from '../models/listing';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ListingService {
  private apiUrl = `${environment.apiUrl}/listings`;

  constructor(private http: HttpClient) { }

  searchListings(query: string, page: number = 1, pageSize: number = 10): Observable<any> {
    const token = localStorage.getItem('token');
  
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  
    const params = {
      query: query,
      page: page.toString(),
      pageSize: pageSize.toString(),
    };
  
    return this.http.get<any>(`${this.apiUrl}/search`, { headers, params });
  }
  
  getRandomListings(): Observable<Listing[]> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<Listing[]>(`${this.apiUrl}/dashboard`, { headers });
  }

  getListing(listingId: number): Observable<Listing> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<Listing>(`${this.apiUrl}/${listingId}`, { headers });
  }

  getListings(): Observable<Listing[]> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<Listing[]>(this.apiUrl, { headers });
  }

  createListing(newListing: Listing): Observable<Listing> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    // Prepare FormData with the image and listing details
    const formData = new FormData();
    formData.append('image', newListing.imageFile as File);
    formData.append('title', newListing.title || '');
    formData.append('aboutLesson', newListing.aboutLesson || '');
    formData.append('aboutYou', newListing.aboutYou || '');
    formData.append('lessonCategoryId', String(newListing.lessonCategoryId || 0));
    formData.append('locations', (newListing.locations || []).join(','));
    formData.append('rates.hourly', String(newListing.rates?.hourly || 0));
    formData.append('rates.fiveHours', String(newListing.rates?.fiveHours || 0));
    formData.append('rates.tenHours', String(newListing.rates?.tenHours || 0));
    
    return this.http.post<Listing>(`${this.apiUrl}/create-listing`, formData, { headers });
  }
}
