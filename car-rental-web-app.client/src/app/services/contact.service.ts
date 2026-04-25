import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  ContactMessageRequest,
  ContactMessageResponse
} from '../models/contact.models';

@Injectable({ providedIn: 'root' })
export class ContactService {

  private readonly apiUrl = `${environment.apiUrl}/contact`;

  constructor(private http: HttpClient) {}

  sendMessage(request: ContactMessageRequest): Observable<ContactMessageResponse> {
    return this.http.post<ContactMessageResponse>(this.apiUrl, request);
  }
}
