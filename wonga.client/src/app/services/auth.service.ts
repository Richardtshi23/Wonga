import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
interface AuthResponse {
  accessToken: string;
  expiresAt: string;
  email: string;
  name: string;
  surname: string;
}
export interface User {
  id: number;
  name: string;
  surname: string;
  email: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private api = `http://127.0.0.1:7025/api/auth`;
  private accessToken: string | null = null;
  private auth$ = new BehaviorSubject<boolean>(false);
  private currentUser$ = new BehaviorSubject<any>(null);
  constructor(private http: HttpClient) {
    const savedToken = localStorage.getItem("accessToken");
    if (savedToken) {
      this.accessToken = savedToken;
    }
  }

  isAuthenticated$() { return this.auth$.asObservable(); }

  getAccessToken() {
    return this.accessToken;
  }

  login(email: string, password: string) {
    return this.http.post<AuthResponse>(`${this.api}/login`, { email, password }, { withCredentials: true })
      .pipe(
        tap(r => {
          localStorage.setItem("accessToken", r.accessToken);
          this.accessToken = r.accessToken;

          const userData = { email: r.email, name: r.name, surname: r.surname };
          this.currentUser$.next(userData);
          this.auth$.next(true);
        })
      );
  }

  register(formvalue: any) {
    return this.http.post(`${this.api}/register`, formvalue);
  }

  getUser() {
    return this.http.get<User>(`${this.api}/me`, { withCredentials: true });
  }

  logout() {
    return this.http.post(`${this.api}/logout`, {}, { withCredentials: false }).pipe(
      tap(() => {
        this.accessToken = null;
        this.auth$.next(false);
      })
    );
  }

  refreshAccessToken() {
    return this.http.post<AuthResponse>(`${this.api}/refresh`, {}, { withCredentials: true }).pipe(
      tap(r => {
        this.accessToken = r.accessToken;
        this.auth$.next(true);
      })
    );
  }
}
