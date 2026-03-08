import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetAstronautDutiesByNameResult } from '../models/get-astronaut-duties-by-name-result';
import { apiBase } from '../api-base';

export interface CreateAstronautDutyRequest {
  name: string;
  rankId: number;
  dutyTitle: string;
  dutyStartDate: string;
}

@Injectable({ providedIn: 'root' })
export class AstronautDutyService {
  private readonly baseUrl = `${apiBase}/AstronautDuty`;

  constructor(private readonly http: HttpClient) {}

  getDutiesByName(name: string): Observable<GetAstronautDutiesByNameResult> {
    const encoded = encodeURIComponent(name);
    return this.http.get<GetAstronautDutiesByNameResult>(`${this.baseUrl}/${encoded}`);
  }

  createDuty(request: CreateAstronautDutyRequest): Observable<unknown> {
    return this.http.post<unknown>(this.baseUrl, request);
  }

  updateDutyRank(dutyId: number, rankId: number): Observable<unknown> {
    return this.http.put<unknown>(`${this.baseUrl}/${dutyId}/rank`, { rankId });
  }
}
