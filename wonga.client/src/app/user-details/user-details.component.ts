import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service'

@Component({
  selector: 'app-user-details',
  templateUrl: './user-details.component.html',
  standalone: false,
  styleUrls: ['./user-details.component.css']
})
export class UserDetailsComponent {
  user: any;

  constructor(private router: Router, private auth: AuthService) {
    const navigation = this.router.getCurrentNavigation();
    this.user = navigation?.extras.state?.['user'];
  }

  ngOnInit() {
    let token = this.auth.getAccessToken();
    this.auth.getUser().subscribe({
      next: (user: any) => {
        this.user = user
      } 
    })
    if (token == null) {
      this.router.navigate(['/login']);
    }
  }
}

