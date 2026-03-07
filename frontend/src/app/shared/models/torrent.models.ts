export interface TorrentCategory {
  id: string;
  name: string;
  url: string;
}

export interface TorrentResult {
  name: string;
  torrentPageUrl: string;
  imageUrl?: string | null;
}

export interface PagedTorrentResponse {
  site: string;
  page: number;
  results: TorrentResult[];
}

export interface CategoriesResponse {
  site: string;
  categories: TorrentCategory[];
}

export interface ErrorResponse {
  status: string;
  message: string;
  details?: string;
}

export type TorrentSite = 'rutor' | 'rutracker';
