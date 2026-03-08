import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetPeopleResult } from '../models/get-people-result';
import { GetPersonByNameResult } from '../models/get-person-by-name-result';
import { apiBase } from '../api-base';

@Injectable({ providedIn: 'root' })
export class PersonService {
  private readonly baseUrl = `${apiBase}/Person`;

  constructor(private readonly http: HttpClient) {}

  getPeople(): Observable<GetPeopleResult> {
    return this.http.get<GetPeopleResult>(this.baseUrl);
  }

  getPersonByName(name: string): Observable<GetPersonByNameResult> {
    const encoded = encodeURIComponent(name);
    return this.http.get<GetPersonByNameResult>(`${this.baseUrl}/${encoded}`);
  }

  createPerson(name: string): Observable<unknown> {
    return this.http.post<unknown>(this.baseUrl, JSON.stringify(name), {
      headers: { 'Content-Type': 'application/json' },
    });
  }

  updatePerson(currentName: string, newName: string): Observable<unknown> {
    const encoded = encodeURIComponent(currentName);
    return this.http.put<unknown>(`${this.baseUrl}/${encoded}`, JSON.stringify(newName), {
      headers: { 'Content-Type': 'application/json' },
    });
  }
}
