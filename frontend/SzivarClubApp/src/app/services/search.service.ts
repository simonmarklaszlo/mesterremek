import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subject } from 'rxjs';

export interface SearchState {
  searchText: string;
  filterCigars: boolean;
  maxDistance: number;
}

export interface SearchTrigger extends SearchState {
  source: 'list' | 'map';
}

@Injectable({
  providedIn: 'root'
})
export class SearchService {
  private searchStateSubject = new BehaviorSubject<SearchState>({
    searchText: '',
    filterCigars: false,
    maxDistance: 10
  });

  private searchTriggeredSubject = new Subject<SearchTrigger>();

  public searchState$: Observable<SearchState> = this.searchStateSubject.asObservable();
  public searchTriggered$: Observable<SearchTrigger> = this.searchTriggeredSubject.asObservable();

  constructor() {}

  /**
   * Update the search state (used by both list and map pages)
   */
  updateSearchState(state: Partial<SearchState>) {
    const currentState = this.searchStateSubject.getValue();
    this.searchStateSubject.next({
      ...currentState,
      ...state
    });
  }

  /**
   * Trigger a search from a specific source page
   */
  triggerSearch(source: 'list' | 'map', state: Partial<SearchState>) {
    const currentState = this.searchStateSubject.getValue();
    const newState = {
      ...currentState,
      ...state,
      source
    };
    this.searchStateSubject.next(newState);
    this.searchTriggeredSubject.next(newState as SearchTrigger);
  }

  /**
   * Get the current search state
   */
  getCurrentSearchState(): SearchState {
    return this.searchStateSubject.getValue();
  }

  /**
   * Update only the search text
   */
  updateSearchText(searchText: string) {
    this.updateSearchState({ searchText });
  }

  /**
   * Update only the cigar filter
   */
  updateFilterCigars(filterCigars: boolean) {
    this.updateSearchState({ filterCigars });
  }

  /**
   * Update only the max distance
   */
  updateMaxDistance(maxDistance: number) {
    this.updateSearchState({ maxDistance });
  }

  /**
   * Reset search state
   */
  resetSearchState() {
    this.searchStateSubject.next({
      searchText: '',
      filterCigars: false,
      maxDistance: 10
    });
  }
}

