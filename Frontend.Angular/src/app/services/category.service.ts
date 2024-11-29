import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { LessonCategory } from '../models/lesson-category';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {
  private apiUrl = `${environment.apiUrl}/lesson/categories`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<LessonCategory[]> {
    return this.http.get<LessonCategory[]>(`${this.apiUrl}/dashboard`);
  }
}
