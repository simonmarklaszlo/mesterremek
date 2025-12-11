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
  IonFabButton
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addOutline, thumbsUpOutline, thumbsDownOutline, thumbsUp, thumbsDown, timeOutline, personOutline, chatbubblesOutline, chevronUpOutline, chevronDownOutline } from 'ionicons/icons';

interface OpeningHour {
  dayOfWeek: string;
  openHour: string;
  closeHour: string;
}

interface Suggestion {
  id: number;
  type: 'shop' | 'hours' | 'address' | 'name';
  shopName?: string;
  shopAddress?: string;
  proposedValue: string;
  author: string;
  createdAt: Date;
  likes: number;
  dislikes: number;
  userVote?: 'like' | 'dislike' | null;
  status: 'pending' | 'approved' | 'rejected';
  expanded?: boolean;
  openingHours?: OpeningHour[];
}

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
    IonCardSubtitle,
    IonCardContent,
    IonChip,
    IonText,
    IonFab,
    IonFabButton,
    CommonModule,
    FormsModule
  ]
})
export class CommunityPage implements OnInit {
  selectedSegment: 'community' | 'own' = 'community';
  suggestions: Suggestion[] = [];
  filteredSuggestions: Suggestion[] = [];

  constructor() {
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
    // Mock adatok - később API-ból jönnek
    this.suggestions = [
      {
        id: 1,
        type: 'shop',
        proposedValue: 'Új Szivar Bolt',
        shopAddress: '1051 Budapest, Nádor utca 12.',
        author: 'user123',
        createdAt: new Date('2025-11-10'),
        likes: 3,
        dislikes: 1,
        userVote: null,
        status: 'pending',
        expanded: false,
        openingHours: [
          { dayOfWeek: 'Hétfő', openHour: '09:00', closeHour: '18:00' },
          { dayOfWeek: 'Kedd', openHour: '09:00', closeHour: '18:00' },
          { dayOfWeek: 'Szerda', openHour: '09:00', closeHour: '18:00' },
          { dayOfWeek: 'Csütörtök', openHour: '09:00', closeHour: '18:00' },
          { dayOfWeek: 'Péntek', openHour: '09:00', closeHour: '18:00' },
          { dayOfWeek: 'Szombat', openHour: '10:00', closeHour: '14:00' },
          { dayOfWeek: 'Vasárnap', openHour: 'Zárva', closeHour: '' }
        ]
      },
      {
        id: 2,
        type: 'shop',
        proposedValue: 'Premium Cigar Shop',
        shopAddress: '1066 Budapest, Andrássy út 45.',
        author: 'cigarfan',
        createdAt: new Date('2025-11-11'),
        likes: 2,
        dislikes: 0,
        userVote: null,
        status: 'pending',
        expanded: false,
        openingHours: [
          { dayOfWeek: 'Hétfő', openHour: '10:00', closeHour: '20:00' },
          { dayOfWeek: 'Kedd', openHour: '10:00', closeHour: '20:00' },
          { dayOfWeek: 'Szerda', openHour: '10:00', closeHour: '20:00' },
          { dayOfWeek: 'Csütörtök', openHour: '10:00', closeHour: '20:00' },
          { dayOfWeek: 'Péntek', openHour: '10:00', closeHour: '20:00' },
          { dayOfWeek: 'Szombat', openHour: '10:00', closeHour: '20:00' },
          { dayOfWeek: 'Vasárnap', openHour: '10:00', closeHour: '20:00' }
        ]
      }
    ];
    this.filterSuggestions();
  }

  filterSuggestions() {
    if (this.selectedSegment === 'community') {
      this.filteredSuggestions = this.suggestions;
    } else {
      // Később: csak a saját javaslatok
      this.filteredSuggestions = this.suggestions.filter(s => s.author === 'currentUser');
    }
  }

  addNewShop() {
    // Később: navigálás az új bolt hozzáadása modal/page-hez
    console.log('Új bolt hozzáadása');
  }

  vote(suggestion: Suggestion, voteType: 'like' | 'dislike') {
    if (suggestion.userVote === voteType) {
      // Visszavonás
      if (voteType === 'like') {
        suggestion.likes--;
      } else {
        suggestion.dislikes--;
      }
      suggestion.userVote = null;
    } else {
      // Új szavazat vagy módosítás
      if (suggestion.userVote === 'like') {
        suggestion.likes--;
      } else if (suggestion.userVote === 'dislike') {
        suggestion.dislikes--;
      }

      if (voteType === 'like') {
        suggestion.likes++;
      } else {
        suggestion.dislikes++;
      }
      suggestion.userVote = voteType;
    }

    // Ellenőrzés: ha eléri a +5-öt
    const netVotes = suggestion.likes - suggestion.dislikes;
    if (netVotes >= 5) {
      suggestion.status = 'approved';
      console.log('Javaslat jóváhagyva!', suggestion);
    }
  }

  getVoteCount(suggestion: Suggestion): number {
    return suggestion.likes - suggestion.dislikes;
  }

  getVoteColor(count: number): string {
    if (count >= 5) return 'success';
    if (count >= 0) return 'primary';
    return 'danger';
  }

  getSuggestionTypeLabel(type: string): string {
    const labels: { [key: string]: string } = {
      'shop': 'Új bolt',
      'hours': 'Nyitvatartás',
      'address': 'Cím',
      'name': 'Név'
    };
    return labels[type] || type;
  }

  formatDate(date: Date): string {
    const now = new Date();
    const diff = now.getTime() - new Date(date).getTime();
    const days = Math.floor(diff / (1000 * 60 * 60 * 24));

    if (days === 0) return 'Ma';
    if (days === 1) return 'Tegnap';
    if (days < 7) return `${days} napja`;
    return new Date(date).toLocaleDateString('hu-HU');
  }

  toggleExpand(suggestion: Suggestion) {
    suggestion.expanded = !suggestion.expanded;
  }
}
