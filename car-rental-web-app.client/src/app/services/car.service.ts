import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import { Car, CarFilters } from '../models/car.models';

@Injectable({ providedIn: 'root' })
export class CarService {

  private readonly apiUrl = `${environment.apiUrl}/vehicles`;

  constructor(private http: HttpClient) {}

  // ── Citire ────────────────────────────────────────────────────

  getAll(filters?: CarFilters): Observable<Car[]> {
    let params = new HttpParams();

    if (filters?.branch && filters.branch !== 'All') {
      params = params.set('branch', filters.branch);
    }
    if (filters?.category && filters.category !== 'All') {
      params = params.set('category', filters.category);
    }
    if (filters?.pickupDate) {
      params = params.set('pickupDate', filters.pickupDate);
    }
    if (filters?.returnDate) {
      params = params.set('returnDate', filters.returnDate);
    }
    if (filters?.transmission) {
      params = params.set('transmission', filters.transmission);
    }
    if (filters?.isOffer !== undefined) {
      params = params.set('isOffer', String(filters.isOffer));
    }

    return this.http.get<Car[]>(this.apiUrl, { params }).pipe(
      map(cars => cars.map(c => this.addAliases(c)))
    );
  }

  getById(id: number): Observable<Car> {
    return this.http.get<Car>(`${this.apiUrl}/${id}`).pipe(
      map(c => this.addAliases(c))
    );
  }

  getOffers(category?: string): Observable<Car[]> {
    let params = new HttpParams().set('isOffer', 'true');
    if (category && category !== 'All') {
      params = params.set('category', category);
    }
    return this.http.get<Car[]>(this.apiUrl, { params }).pipe(
      map(cars => cars.map(c => this.addAliases(c)))
    );
  }

  getBranches(): Observable<string[]> {
    return this.http.get<string[]>(`${this.apiUrl}/branches`);
  }

  // ── Helper ────────────────────────────────────────────────────

  private addAliases(car: Car): Car {
    return {
      ...car,
      color: car.colorHex,
      fuel: car.fuelType,
      isFavorite: car.isFavorite ?? false,
    };
  }
}