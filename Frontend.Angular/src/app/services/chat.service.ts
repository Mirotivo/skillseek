import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Chat } from '../models/chat';
import { environment } from '../environments/environment';
import { SendMessage } from '../models/send-message';

@Injectable({
  providedIn: 'root',
})
export class ChatService {
  private apiUrl = `${environment.apiUrl}/chats`;

  constructor(private http: HttpClient) {}

  getChats(): Observable<Chat[]> {
    const token = localStorage.getItem('token');

    if (!token) {
      console.error('No token found in localStorage');
      throw new Error('No authentication token available');
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });

    return this.http.get<Chat[]>(this.apiUrl, { headers });
  }

  getMessages(): Observable<{ sender: string; content: string; time: string }[]> {
    return this.getChats().pipe(
      map((chats) =>
        chats.map((chat) => {
          const lastMessage = chat.messages[chat.messages.length - 1]; // Get the last message
          return {
            sender: chat.name,
            content: lastMessage?.text || 'No messages yet', // Fallback if no messages
            time: lastMessage?.timestamp || 'N/A', // Fallback if no timestamp
          };
        })
      )
    );
  }


  sendMessage(message: SendMessage): Observable<any> {
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`,
    });
  
    return this.http.post(`${this.apiUrl}/send`, message, { headers });
  }  
}
