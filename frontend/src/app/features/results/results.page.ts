import { Component, OnInit, inject, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { TorrentsApiService } from '../../shared/services/torrents-api.service';
import { ResultsViewModel } from './results.state';
import { TorrentSite, TorrentCategory } from '../../shared/models/torrent.models';

@Component({
  selector: 'app-results',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatPaginatorModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './results.page.html',
  styleUrl: './results.page.scss'
})
export class ResultsPage implements OnInit {
  private readonly torrentsApi = inject(TorrentsApiService);
  protected readonly vm = new ResultsViewModel();

  readonly sites: TorrentSite[] = ['rutor', 'rutracker'];

  constructor() {
    // Auto-fetch categories when site changes
    effect(() => {
      const site = this.vm.site();
      this.loadCategories(site);
    });
  }

  ngOnInit(): void {
    // No automatic loading - user must click Load button
  }

  onSiteChange(site: TorrentSite): void {
    this.vm.setSite(site);
    this.vm.setPage(1);
  }

  onCategoryChange(category: TorrentCategory | null): void {
    this.vm.setSelectedCategory(category);
    this.vm.setPage(1);
  }

  onLoadClick(): void {
    const site = this.vm.site();
    const category = this.vm.selectedCategory();
    const page = this.vm.page();
    
    if (category) {
      this.loadTorrents(site, category, page);
    }
  }

  onPageChange(event: PageEvent): void {
    this.vm.setPage(event.pageIndex + 1); // pageIndex is 0-based, our API is 1-based
    this.onLoadClick(); // Trigger load when page changes
  }

  private loadCategories(site: TorrentSite): void {
    this.vm.setCategoriesLoading(true);
    
    this.torrentsApi.getCategories(site).subscribe({
      next: (response) => {
        this.vm.setCategories(response.categories);
        this.vm.setCategoriesLoading(false);
        
        // Auto-select first category if available
        if (response.categories.length > 0) {
          this.vm.setSelectedCategory(response.categories[0]);
        }
      },
      error: (error) => {
        console.error('Failed to load categories:', error);
        this.vm.setCategories([]);
        this.vm.setCategoriesLoading(false);
      }
    });
  }

  private loadTorrents(site: TorrentSite, category: TorrentCategory | null, page: number): void {
    this.vm.setLoading(true);
    this.vm.clearError();

    const categoryUrl = category?.url || null;
    this.torrentsApi.getTorrents(site, categoryUrl, page).subscribe({
      next: (response) => {
        this.vm.setTorrents(response.results, response.results.length);
        this.vm.setLoading(false);
      },
      error: (error) => {
        const errorMessage = error.error?.message || 'Failed to load torrents. Please try again.';
        this.vm.setError(errorMessage);
        this.vm.setLoading(false);
      }
    });
  }

  compareCategories(c1: TorrentCategory | null, c2: TorrentCategory | null): boolean {
    return c1?.id === c2?.id;
  }

  openTorrentPage(url: string): void {
    window.open(url, '_blank', 'noopener,noreferrer');
  }
}
