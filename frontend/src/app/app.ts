import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';

const THEME_KEY = 'theme';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  public readonly title = signal('frontend');
  public readonly theme = signal<string>(localStorage.getItem(THEME_KEY) ?? 'light');

  constructor() {
    this.applyTheme(this.theme());
  }

  public onThemeChange(e: Event) {
    const value = (e.target as HTMLSelectElement).value;
    this.theme.set(value);
    try { localStorage.setItem(THEME_KEY, value); } catch { }
    this.applyTheme(value);
  }

  private applyTheme(theme: string) {
    const root = document.documentElement;
    const body = document.body;

    // update root class and data attribute
    root.classList.remove('theme-light', 'theme-dark');
    root.classList.add(theme === 'dark' ? 'theme-dark' : 'theme-light');
    root.setAttribute('data-theme', theme);

    // update body class so global styles can target body as well
    body.classList.remove('theme-light', 'theme-dark', 'theme-applied');
    body.classList.add(theme === 'dark' ? 'theme-dark' : 'theme-light', 'theme-applied');

    // set color-scheme for browser form controls / scrollbar
    try { document.documentElement.style.setProperty('color-scheme', theme === 'dark' ? 'dark' : 'light'); } catch { }

    // Ensure the native <select> reflects the current theme value after render
    requestAnimationFrame(() => {
      const sel = document.getElementById('theme') as HTMLSelectElement | null;
      if (sel) sel.value = theme;
    });
  }
}
