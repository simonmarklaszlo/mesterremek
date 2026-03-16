import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { Capacitor } from '@capacitor/core';
import { Filesystem, Directory, Encoding } from '@capacitor/filesystem';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private isDarkSubject = new BehaviorSubject<boolean>(true);
  readonly theme$ = this.isDarkSubject.asObservable();

  get isDark(): boolean {
    return this.isDarkSubject.value;
  }

  async init(): Promise<void> {
    const initial = await this.readThemeFromStorage();
    this.applyTheme(initial);
  }

  async setTheme(isDark: boolean): Promise<void> {
    this.applyTheme(isDark);
    await this.writeThemeToStorage(isDark ? 'dark' : 'light');
  }

  private applyTheme(isDark: boolean): void {
    // Remove both theme classes first from body
    document.body.classList.remove('dark-theme', 'light-theme');
    // Apply Ionic dark mode to html element using ion-palette-dark class
    const htmlElement = document.documentElement;
    htmlElement.classList.toggle('ion-palette-dark', isDark);

    if (isDark) {
      document.body.classList.add('dark-theme');
    } else {
      document.body.classList.add('light-theme');
    }
    this.isDarkSubject.next(isDark);
  }

  private async readThemeFromStorage(): Promise<boolean> {
    const platform = Capacitor.getPlatform();
    try {
      if (platform !== 'web') {
        const { data } = await Filesystem.readFile({
          path: 'theme.txt',
          directory: Directory.Data,
          encoding: Encoding.UTF8,
        });
        // Filesystem.readFile can return a string or Blob depending on platform/environment.
        // Convert Blob -> string safely to avoid TS errors (trim is not on Blob).
        let dataStr = '';
        if (typeof data === 'string') {
          dataStr = data;
        } else if (data instanceof Blob) {
          dataStr = await this.blobToText(data as Blob);
        } else {
          dataStr = String(data || '');
        }
        const val = (dataStr || '').trim().toLowerCase();
        if (val === 'dark') return true;
        if (val === 'light') return false;
      } else {
        const v = localStorage.getItem('theme');
        if (v === 'dark') return true;
        if (v === 'light') return false;
      }
    } catch {
      // Ignore missing file or errors and fall back to default
    }
    return true; // default to dark
  }

  private async writeThemeToStorage(val: string): Promise<void> {
    const platform = Capacitor.getPlatform();
    try {
      if (platform !== 'web') {
        await Filesystem.writeFile({
          path: 'theme.txt',
          data: val,
          directory: Directory.Data,
          encoding: Encoding.UTF8,
          recursive: true,
        });
      } else {
        localStorage.setItem('theme', val);
      }
    } catch {
      // Silently ignore write errors
    }
  }

  // Convert Blob to text via FileReader. This is used when the
  // Filesystem.readFile returns a Blob in certain web environments.
  private blobToText(blob: Blob): Promise<string> {
    return new Promise((resolve, reject) => {
      try {
        const reader = new FileReader();
        reader.onload = () => {
          resolve(typeof reader.result === 'string' ? reader.result : '');
        };
        reader.onerror = () => reject(reader.error);
        reader.readAsText(blob);
      } catch (e) {
        reject(e);
      }
    });
  }
}
