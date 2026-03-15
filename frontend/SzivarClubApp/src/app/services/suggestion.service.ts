import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
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

    console.log('[SuggestionService.getSuggestions] request:', {
      url: this.apiUrl,
      params: filters ?? {}
    });

    return this.http.get<SuggestionsResponse>(this.apiUrl, { params })
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.getSuggestions] response:', response);
        }),
        map(response => response.suggestions),
        tap((suggestions) => {
          console.log('[SuggestionService.getSuggestions] suggestions:', suggestions);
        })
      );
  }

  getSuggestionById(id: number): Observable<Suggestion> {
    const url = `${this.apiUrl}/${id}`;
    console.log('[SuggestionService.getSuggestionById] request:', { url, id });

    return this.http.get<SuggestionResponse>(url)
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.getSuggestionById] response:', response);
        }),
        map(response => response.suggestion)
      );
  }

  createSuggestion(data: CreateSuggestionRequest): Observable<Suggestion> {
    console.log('[SuggestionService.createSuggestion] request:', { url: this.apiUrl, body: data });

    return this.http.post<SuggestionResponse>(this.apiUrl, data)
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.createSuggestion] response:', response);
        }),
        map(response => response.suggestion)
      );
  }

  voteSuggestion(id: number, voteType: VoteType): Observable<Suggestion> {
    const url = `${this.apiUrl}/${id}/vote`;
    const body = { voteType };
    console.log('[SuggestionService.voteSuggestion] request:', { url, id, body });

    return this.http.post<SuggestionResponse>(url, body)
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.voteSuggestion] response:', response);
        }),
        map(response => response.suggestion)
      );
  }

  removeVote(id: number): Observable<Suggestion> {
    const url = `${this.apiUrl}/${id}/vote`;
    console.log('[SuggestionService.removeVote] request:', { url, id });

    return this.http.delete<SuggestionResponse>(url)
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.removeVote] response:', response);
        }),
        map(response => response.suggestion)
      );
  }

  deleteSuggestion(id: number): Observable<void> {
    const url = `${this.apiUrl}/${id}`;
    console.log('[SuggestionService.deleteSuggestion] request:', { url, id });

    return this.http.delete<void>(url)
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.deleteSuggestion] response:', response);
        })
      );
  }

  applySuggestion(id: number): Observable<Suggestion> {
    const url = `${this.apiUrl}/${id}/apply`;
    console.log('[SuggestionService.applySuggestion] request:', { url, id, body: {} });

    return this.http.post<SuggestionResponse>(url, {})
      .pipe(
        tap((response) => {
          console.log('[SuggestionService.applySuggestion] response:', response);
        }),
        map(response => response.suggestion)
      );
  }
}
