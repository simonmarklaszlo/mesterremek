import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonIcon,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonList,
  IonItem,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardSubtitle,
  IonCardContent,
  IonChip,
  IonText,
  IonFab,
  IonFabButton,
  IonSpinner,
  ModalController
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addOutline, thumbsUpOutline, thumbsDownOutline, thumbsUp, thumbsDown, timeOutline, personOutline, chatbubblesOutline, chevronUpOutline, chevronDownOutline } from 'ionicons/icons';
import { Suggestion, VoteType, OpeningHour } from '../../models/suggestion.model';
import { SuggestionService } from '../../services/suggestion.service';
import { CreateSuggestionModalComponent } from './create-suggestion-modal/create-suggestion-modal.component';

@Component({
  selector: 'app-community',
  templateUrl: './community.page.html',
  styleUrls: ['./community.page.scss'],
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonIcon,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonList,
    IonItem,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonChip,
    IonText,
    IonFab,
    IonFabButton,
    IonSpinner,
    CommonModule,
    FormsModule
  ]
})
export class CommunityPage implements OnInit {
  selectedSegment: 'community' | 'own' = 'community';
  suggestions: Suggestion[] = [];
  filteredSuggestions: Suggestion[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(
    private suggestionService: SuggestionService,
    private modalCtrl: ModalController
  ) {
    addIcons({ addOutline, thumbsUpOutline, thumbsDownOutline, thumbsUp, thumbsDown, timeOutline, personOutline, chatbubblesOutline, chevronUpOutline, chevronDownOutline });
  }

  ngOnInit() {
    this.loadSuggestions();
  }

  segmentChanged(event: any) {
    this.selectedSegment = event.detail.value;
    this.filterSuggestions();
  }

  loadSuggestions() {
    this.isLoading = true;
    this.errorMessage = '';

    const filters = {
      filter: this.selectedSegment === 'community' ? 'all' as const : 'own' as const,
      status: 'pending' as const
    };

    this.suggestionService.getSuggestions(filters).subscribe({
      next: (suggestions) => {
        this.suggestions = suggestions;
        this.filteredSuggestions = suggestions;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading suggestions:', error);
        this.errorMessage = 'Nem sikerült betölteni a javaslatokat.';
        this.isLoading = false;
      }
    });
  }

  filterSuggestions() {
    this.loadSuggestions();
  }

  async addNewShop() {
    const modal = await this.modalCtrl.create({
      component: CreateSuggestionModalComponent
    });

    modal.onDidDismiss().then((result) => {
      if (result.role === 'submit' && result.data) {
        this.loadSuggestions();
      }
    });

    return await modal.present();
  }

  vote(suggestion: Suggestion, voteType: VoteType) {
    // Ha ugyanarra kattint, akkor visszavonás
    if (suggestion.userVote === voteType) {
      this.suggestionService.removeVote(suggestion.id).subscribe({
        next: (updatedSuggestion) => {
          const index = this.suggestions.findIndex(s => s.id === suggestion.id);
          if (index !== -1) {
            this.suggestions[index] = updatedSuggestion;
            this.filterSuggestions();
          }
        },
        error: (error) => {
          console.error('Error removing vote:', error);
        }
      });
    } else {
      // Új szavazat vagy módosítás
      this.suggestionService.voteSuggestion(suggestion.id, voteType).subscribe({
        next: (updatedSuggestion) => {
          const index = this.suggestions.findIndex(s => s.id === suggestion.id);
          if (index !== -1) {
            this.suggestions[index] = updatedSuggestion;
            this.filterSuggestions();
          }
        },
        error: (error) => {
          console.error('Error voting:', error);
        }
      });
    }
  }

  getVoteCount(suggestion: Suggestion): number {
    return suggestion.netVotes;
  }

  getVoteColor(count: number): string {
    if (count >= 5) return 'success';
    if (count >= 0) return 'primary';
    return 'danger';
  }

  getSuggestionTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'new_shop': 'Új bolt',
      'edit_hours': 'Nyitvatartás',
      'edit_address': 'Cím',
      'edit_name': 'Név'
    };
    return labels[type] || type;
  }

  formatDate(date: string | Date): string {
    const now = new Date();
    const targetDate = typeof date === 'string' ? new Date(date) : date;
    const diff = now.getTime() - targetDate.getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) return 'Ma';
    if (days === 1) return 'Tegnap';
    if (days < 7) return `${days} napja`;
    return targetDate.toLocaleDateString('hu-HU');
  }

  toggleExpand(suggestion: Suggestion) {
    suggestion.expanded = !suggestion.expanded;
  }
}
