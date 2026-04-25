import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  CreateBookingRequest,
  BookingResponse,
  Rental
} from '../models/booking.models';

@Injectable({ providedIn: 'root' })
export class BookingService {

  private readonly apiUrl = `${environment.apiUrl}/rentals`;

  constructor(private http: HttpClient) {}

  // ── Creare rezervare ──────────────────────────────────────────

  create(booking: CreateBookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(this.apiUrl, booking);
  }

  // ── Istoric rezervări utilizator curent ───────────────────────

  getMyBookings(): Observable<Rental[]> {
    return this.http.get<Rental[]>(`${this.apiUrl}/my`);
  }

  // ── Detalii rezervare după ID ─────────────────────────────────

  getById(id: number): Observable<BookingResponse> {
    return this.http.get<BookingResponse>(`${this.apiUrl}/${id}`);
  }

  // ── Anulare rezervare ─────────────────────────────────────────

  cancel(id: number, reason: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/cancel`, { reason });
  }
}
