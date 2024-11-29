import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root',
})
export class UserService {

  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getUser(): Observable<User> {
    const token = localStorage.getItem('token');

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<User>(`${this.apiUrl}/me`, { headers });
  }
}
