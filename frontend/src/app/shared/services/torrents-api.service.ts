import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PagedTorrentResponse, TorrentSite, CategoriesResponse } from '../models/torrent.models';

@Injectable({
  providedIn: 'root'
})
export class TorrentsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/api/torrents`;

  getCategories(site: TorrentSite): Observable<CategoriesResponse> {
    const params = new HttpParams().set('site', site);
    return this.http.get<CategoriesResponse>(`${this.baseUrl}/categories`, { params });
  }

  getTorrents(site: TorrentSite, categoryUrl: string | null, page: number): Observable<PagedTorrentResponse> {
    let params = new HttpParams()
      .set('site', site)
      .set('page', page.toString());

    if (categoryUrl) {
      params = params.set('category', categoryUrl);
    }

    return this.http.get<PagedTorrentResponse>(this.baseUrl, { params });
  }
}
