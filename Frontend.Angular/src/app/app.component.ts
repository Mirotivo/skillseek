import { Component, OnInit } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { NotificationService } from './services/notification.service';
import { AuthService } from './services/auth.service';
import { NotificationEvent } from './models/notification';
import { ToastrService } from 'ngx-toastr';
import { MessagesComponent } from './pages/messages/messages.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`
})
export class AppComponent implements OnInit {
  currentRoute: string = '';
  constructor(
    private notificationService: NotificationService,
    private authService: AuthService,
    private toastr: ToastrService,
    private router: Router
  ) { }

  ngOnInit(): void {
    // Check if the user is logged in
    if (this.authService.isLoggedIn()) {
      // Monitor the active route
      this.router.events.subscribe((event) => {
        if (event instanceof NavigationEnd) {
          this.currentRoute = event.urlAfterRedirects; // Update current route
        }
      });


      // Start the notification service
      this.notificationService.startConnection(this.authService.getToken() ?? "");

      // Listen for notifications
      this.notificationService.onReceiveNotification((notification) => {
        this.toastr.info(notification.data.content, notification.message);
        // if (notification.eventName === NotificationEvent.NewMessage) {

        //   // Reload messages if the current route is the messages page
        //   if (this.currentRoute === '/messages') {
        //     this.reloadMessages(notification.data.recipientId);
        //   }
        //   else {
        //     this.toastr.info(notification.data.content, notification.message);
        //   }
        // }
      });
    }
  }

  reloadMessages(contactId: number): void {
    const messagesComponent = this.router.routerState.root.firstChild?.component as MessagesComponent | undefined;
    if (messagesComponent) {
      messagesComponent.selectContactById(contactId);
    }
  }
}
