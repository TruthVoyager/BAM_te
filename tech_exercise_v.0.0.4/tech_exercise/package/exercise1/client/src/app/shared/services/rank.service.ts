import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Rank } from '../models/rank';
import { apiBase } from '../api-base';

@Injectable({ providedIn: 'root' })
export class RankService {
  private readonly baseUrl = `${apiBase}/Rank`;

  constructor(private readonly http: HttpClient) {}

  getRanks(): Observable<Rank[]> {
    return this.http.get<Rank[]>(this.baseUrl);
  }
}
