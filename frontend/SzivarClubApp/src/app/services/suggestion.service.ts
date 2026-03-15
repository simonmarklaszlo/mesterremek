import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import {
  Suggestion,
  CreateSuggestionRequest,
  SuggestionsResponse,
  SuggestionResponse,
  SuggestionFilters,
  VoteType
} from '../models/suggestion.model';

@Injectable({
  providedIn: 'root'
})
export class SuggestionService {
  private apiUrl = `${environment.apiUrl}/suggestions`;

  constructor(private http: HttpClient) {}

  getSuggestions(filters?: SuggestionFilters): Observable<Suggestion[]> {
    let params = new HttpParams();

    if (filters?.filter) {
      params = params.set('filter', filters.filter);
    }
    if (filters?.status) {
      params = params.set('status', filters.status);
    }
    if (filters?.type) {
      params = params.set('type', filters.type);
    }
    if (filters?.shopId !== undefined) {
      params = params.set('shopId', filters.shopId.toString());
    }

    return this.http.get<SuggestionsResponse>(this.apiUrl, { params })
      .pipe(map(response => response.suggestions));
  }

  getSuggestionById(id: number): Observable<Suggestion> {
    return this.http.get<SuggestionResponse>(`${this.apiUrl}/${id}`)
      .pipe(map(response => response.suggestion));
  }

  createSuggestion(data: CreateSuggestionRequest): Observable<Suggestion> {
    return this.http.post<SuggestionResponse>(this.apiUrl, data)
      .pipe(map(response => response.suggestion));
  }

  voteSuggestion(id: number, voteType: VoteType): Observable<Suggestion> {
    return this.http.post<SuggestionResponse>(`${this.apiUrl}/${id}/vote`, { voteType })
      .pipe(map(response => response.suggestion));
  }

  removeVote(id: number): Observable<Suggestion> {
    return this.http.delete<SuggestionResponse>(`${this.apiUrl}/${id}/vote`)
      .pipe(map(response => response.suggestion));
  }

  deleteSuggestion(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  applySuggestion(id: number): Observable<Suggestion> {
    return this.http.post<SuggestionResponse>(`${this.apiUrl}/${id}/apply`, {})
      .pipe(map(response => response.suggestion));
  }
}

