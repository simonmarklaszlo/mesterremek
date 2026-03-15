// Suggestion típusok és státuszok
export interface SuggestionType {
  id: number;
  code: 'new_shop' | 'edit_name' | 'edit_address' | 'edit_hours';
  name: string;
  description?: string;
}

export interface SuggestionStatus {
  id: number;
  code: 'pending' | 'approved' | 'rejected' | 'applied';
  name: string;
  description?: string;
}

// Nyitvatartás
export interface OpeningHour {
  dayOfWeek: string;
  openHour: string;
  closeHour: string;
}

// Javaslat létrehozási kérés
export interface CreateSuggestionRequest {
  type: 'new_shop' | 'edit_name' | 'edit_address' | 'edit_hours';
  shopId?: number; // Csak módosítási javaslatoknál
  proposedValue: string; // JSON string lehet (complex data esetén)
  additionalData?: any; // További adatok (pl. nyitvatartás)
}

// Szavazat típus
export type VoteType = 'like' | 'dislike';

// Teljes javaslat objektum (API response)
export interface Suggestion {
  id: number;
  typeId: number;
  typeCode: 'new_shop' | 'edit_name' | 'edit_address' | 'edit_hours';
  typeName: string;
  statusId: number;
  statusCode: 'pending' | 'approved' | 'rejected' | 'applied';
  statusName: string;
  shopId?: number;
  shopName?: string;
  shopAddress?: string;
  shopCity?: string;
  proposedValue: string;
  additionalData?: any;
  userId: number;
  userName?: string;
  createdAt: string;
  netVotes: number;
  userVote?: VoteType | null;

  // Frontend segédváltozók
  expanded?: boolean;
}

// API Response típusok
export interface SuggestionsResponse {
  success: boolean;
  suggestions: Suggestion[];
  total: number;
}

export interface SuggestionResponse {
  success: boolean;
  suggestion: Suggestion;
}

// Szűrési paraméterek
export interface SuggestionFilters {
  filter?: 'all' | 'own'; // all = közösségi, own = saját
  status?: 'pending' | 'approved' | 'rejected' | 'applied';
  type?: 'new_shop' | 'edit_name' | 'edit_address' | 'edit_hours';
  shopId?: number; // Egy adott bolthoz tartozó javaslatok
}
