import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { Proposition } from '../models/proposition';

@Injectable({
  providedIn: 'root',
})
export class PropositionService {
  private apiUrl = `${environment.apiUrl}/lessons`;

  constructor(private http: HttpClient) {}

  proposeLesson(lesson: Proposition): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    // Ensure duration is in "HH:mm:ss" format
    const formattedLesson = {
      ...lesson,
      duration: this.formatDuration(lesson.duration),
    };

    return this.http.post(`${this.apiUrl}/proposeLesson`, formattedLesson, { headers });
  }

  respondToProposition(propositionId: number, accept: boolean) {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.post(`${this.apiUrl}/respondToProposition/${propositionId}`, accept, { headers });
  }

  private formatDuration(hours: number): string {
    const totalSeconds = hours * 3600; // Convert hours to seconds
    const h = Math.floor(totalSeconds / 3600);
    const m = Math.floor((totalSeconds % 3600) / 60);
    const s = totalSeconds % 60;
    return `${h.toString().padStart(2, '0')}:${m
      .toString()
      .padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
  }

  getPropositions(contactId: number): Observable<{ propositions: any[]; lessons: any[] }> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  
    return this.http.get<{ propositions: any[]; lessons: any[] }>(`${this.apiUrl}/${contactId}`, { headers });
  }
  
}