import { signal, computed } from '@angular/core';
import { TorrentResult, TorrentSite, TorrentCategory } from '../../shared/models/torrent.models';

export interface ResultsState {
  site: TorrentSite;
  categories: TorrentCategory[];
  selectedCategory: TorrentCategory | null;
  page: number;
  torrents: TorrentResult[];
  totalResults: number;
  loading: boolean;
  categoriesLoading: boolean;
  error: string | null;
}

export class ResultsViewModel {
  private readonly state = signal<ResultsState>({
    site: 'rutor',
    categories: [],
    selectedCategory: null,
    page: 1,
    torrents: [],
    totalResults: 0,
    loading: false,
    categoriesLoading: false,
    error: null
  });

  // Read-only state accessors
  readonly site = computed(() => this.state().site);
  readonly categories = computed(() => this.state().categories);
  readonly selectedCategory = computed(() => this.state().selectedCategory);
  readonly page = computed(() => this.state().page);
  readonly torrents = computed(() => this.state().torrents);
  readonly totalResults = computed(() => this.state().totalResults);
  readonly loading = computed(() => this.state().loading);
  readonly categoriesLoading = computed(() => this.state().categoriesLoading);
  readonly error = computed(() => this.state().error);

  setSite(site: TorrentSite): void {
    this.state.update(s => ({ ...s, site, page: 1, selectedCategory: null, categories: [] }));
  }

  setCategories(categories: TorrentCategory[]): void {
    this.state.update(s => ({ ...s, categories }));
  }

  setSelectedCategory(category: TorrentCategory | null): void {
    this.state.update(s => ({ ...s, selectedCategory: category, page: 1 }));
  }

  setPage(page: number): void {
    this.state.update(s => ({ ...s, page }));
  }

  setLoading(loading: boolean): void {
    this.state.update(s => ({ ...s, loading }));
  }

  setCategoriesLoading(categoriesLoading: boolean): void {
    this.state.update(s => ({ ...s, categoriesLoading }));
  }

  setTorrents(torrents: TorrentResult[], totalResults: number): void {
    this.state.update(s => ({ ...s, torrents, totalResults, error: null }));
  }

  setError(error: string): void {
    this.state.update(s => ({ ...s, error, torrents: [], totalResults: 0 }));
  }

  clearError(): void {
    this.state.update(s => ({ ...s, error: null }));
  }
}
